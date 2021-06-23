using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02.API.Models;
using W65C02.API.Parsers;

namespace W65C02S.CPU
{
    public partial class CPUCore : IDisposable
    {
        private readonly SemaphoreSlim semaphore;
        private ulong clockTicks = 0;           // 0 to 18,446,744,073,709,551,615
        private byte? fetchedByte;
        private ushort? operandAddress;
        private List<Instruction> instructionTable;
        private Instruction currentInstruction;
        private bool stopCmdAsserted = false;
        private bool interuptRequested = false;
        private bool interuptMasked = false;

        private const ushort IRQ_Vect = 0x0FFFE;
        private const ushort Rest_Vect = 0x0FFFC;
        private const ushort NMI_Vect = 0x0FFFA;

        private readonly IBus bus;

        /// <summary>
        /// Accumulator Register
        /// </summary>
        public byte A { get; set; }             // Accumulator Register

        /// <summary>
        /// X Index Register
        /// </summary>
        public byte X { get; set; }             // X Register

        /// <summary>
        /// Y Index Register
        /// </summary>
        public byte Y { get; set; }             // Y Register

        /// <summary>
        /// Stack Pointer Register
        /// </summary>
        public ushort SP { get; set; }          // Stack Pointer

        /// <summary>
        /// Program Counter Register
        /// </summary>
        public ushort PC { get; set; }          // Program Counter

        /// <summary>
        /// Processor Status Register
        /// </summary>
        public ProcessorFlags ST { get; set; }  // Status Register

        public List<Instruction> InstructionTable => this.instructionTable;


        public CPUCore(IBus bus)
        {
            semaphore = new SemaphoreSlim(1, 1);
            this.bus = bus;
            bus?.Subscribe<InteruptRequestEventArgs>(OnInteruptRequest);
            bus?.Subscribe<ResetEventArgs>(OnReset);

            Initialise();
            SetupInstructionTable();
        }

        private void OnReset(ResetEventArgs obj)
        {
            Reset();
        }

        private void Initialise()
        {
            clockTicks = 0;
            A = 0;
            X = 0;
            Y = 0;
            SP = 0x0000;
            ST = ProcessorFlags.U;
        }

        public void Reset()
        {
            Initialise();

            // initialise stack
            SP = 0x01FF;

            clockTicks = 5;
            PC = Rest_Vect;
            ReadValueFromAddress(PC);
            var lo = fetchedByte;
            clockTicks++;

            PC++;
            ReadValueFromAddress(PC);
            var hi = fetchedByte;
            clockTicks++;

            PC = (ushort)((hi << 8) | lo);
            stopCmdAsserted = false;
        }
        public void Step()
        {
            if (stopCmdAsserted)
            {
                stopCmdAsserted = true;
                var e = new ExceptionEventArg() { ErrorMessage = $"Proccessor has STOPed . A Reset is required." };
                bus?.Publish(e);
                return;
            }

            Execute();
        }
        private void Execute()
        {
            semaphore.Wait();
            if (interuptRequested)
            {
                if (interuptMasked)
                {
                    HandleNMI();
                }
                else
                {
                    HandleIRQ();
                }

                interuptRequested = false;
                semaphore.Release();
                return;
            }

            ReadValueFromAddress(PC);

            if (instructionTable.Any(x => x.OpCode == fetchedByte.Value))
            {
                currentInstruction = instructionTable.First(x => x.OpCode == fetchedByte.Value);
                if (currentInstruction.Length > 1)
                {
                    ReadValueFromAddress((ushort)(PC + 1));
                    currentInstruction.Operand1 = fetchedByte.Value;
                }
                if (currentInstruction.Length > 2)
                {
                    ReadValueFromAddress((ushort)(PC + 2));
                    currentInstruction.Operand2 = fetchedByte.Value;
                }

                try
                {
                    RaiseInstructionExecuting();
                    Execute(currentInstruction);
                }
                catch (NotImplementedException)
                {
                    stopCmdAsserted = true;
                    var e = new ExceptionEventArg() { ErrorMessage = $"OpCode [${currentInstruction.OpCode:X2} ({currentInstruction.Mnemonic})]  not Implemented".PadRight(100, ' ') };
                    bus?.Publish(e);
                }
                catch (Exception x)
                {
                    stopCmdAsserted = true;
                    var e = new ExceptionEventArg() { ErrorMessage = x.Message.PadRight(100, ' ') };
                    bus?.Publish(e);
                }
                finally
                {
                    RaiseInstructionExecuted();
                    if (currentInstruction.Mnemonic == "STP" || currentInstruction.Mnemonic == "WAI")
                    {
                        var e = new ExceptionEventArg() { ErrorMessage = $"Processor halted with SToP/WAIt instruction...".PadRight(100, ' '), ExceptionType = ExceptionType.Warning };
                        bus?.Publish(e);
                    }
                    operandAddress = null;
                    fetchedByte = null;
                }
            }
            else
            {
                stopCmdAsserted = true;
                var e = new ExceptionEventArg() { ErrorMessage = $"Unknown instruction: ${fetchedByte:X2}".PadRight(100, ' ') };
                bus?.Publish(e);
            }
            semaphore.Release();
        }
        public void Execute(Instruction newInstruction)
        {
            if (currentInstruction == null)
            {
                currentInstruction = newInstruction;
            }
            currentInstruction.AddressModeAction();
            currentInstruction.InstructionAction();
            clockTicks += currentInstruction.Length;
        }

        private void IncrementPC(sbyte amount = 1)
        {
            if (amount == 0)
            {
                return;
            }

            PC += (ushort)amount;

            if (PC > 0xFFFF)
            {
                PC = 0xFFFF;
            }
        }

        private void IncrementPC(byte amount = 1)
        {
            if (amount == 0)
            {
                return;
            }

            PC += amount;

            if (PC > 0xFFFF)
            {
                PC = 0xFFFF;
            }
        }

        private void IncreaseSP()
        {
            SP += 1;
            if (SP > 0x01FF)
            {
                SP = 0x0100;
            }
        }

        private void DecreaseSP()
        {
            SP -= 1;
            if (SP < 0x0100)
            {
                SP = 0x01FF;
            }
        }

        void SetFlag(ProcessorFlags flag, bool isOn)
        {
            if (isOn)
            {
                ST |= flag;
            }
            else
            {
                ST &= ~flag;
            }
        }

        public bool IsFlagSet(ProcessorFlags flag)
        {
            return ((ST & flag) == flag);
        }

        private void ReadValueFromAddress(ushort address)
        {
            fetchedByte = null;
            var arg = new AddressBusEventArgs
            {
                Address = address,
                Mode = DataBusMode.Read
            };
            bus?.Publish(arg);
            fetchedByte = arg.Data;
            clockTicks++;
        }

        private void WriteValueToAddress(ushort address, byte data)
        {
            var arg = new AddressBusEventArgs
            {
                Address = address,
                Mode = DataBusMode.Write,
                Data = data
            };
            bus?.Publish(arg);
            clockTicks++;
        }

        private void RaiseInstructionExecuting()
        {
            var arg = new OnInstructionExecutingEventArg
            {
                CurrentInstruction = currentInstruction,
                DecodedInstruction = InstructionParser.Parse(currentInstruction),
                A = A,
                X = X,
                Y = Y,
                PC = PC,
                SP = SP,
                ST = ST,
                RawData = $"{currentInstruction.OpCode:X2} {currentInstruction.Operand1:X2} {currentInstruction.Operand2:X2}".TrimEnd(),
                ClockTicks = clockTicks
            };
            bus?.Publish(arg);
        }

        private void RaiseInstructionExecuted()
        {
            var arg = new OnInstructionExecutedEventArg
            {
                CurrentInstruction = currentInstruction,
                DecodedInstruction = InstructionParser.Parse(currentInstruction),
                A = A,
                X = X,
                Y = Y,
                PC = PC,
                SP = SP,
                ST = ST,
                RawData = $"{currentInstruction.OpCode:X2} {currentInstruction.Operand1:X2} {currentInstruction.Operand2:X2}".TrimEnd(),
                ClockTicks = clockTicks
            };
            bus?.Publish(arg);
        }

        private void OnInteruptRequest(InteruptRequestEventArgs arg)
        {
            if (arg.InteruptType == InteruptType.IRQ)
            {
                interuptRequested = true;
                interuptMasked = false;
            }
            if (arg.InteruptType == InteruptType.NMI)
            {
                interuptRequested = true;
                interuptMasked = true;
            }
        }

        private void HandleIRQ()
        {

            // save the Processor Flags to stack
            WriteValueToAddress(SP, (byte)ST);
            DecreaseSP();

            // save return address to stack, hi byte first then lo byte
            var retAddr = (PC); // + currentInstruction.Length
            WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
            DecreaseSP();

            WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
            DecreaseSP();

            //hardware interrupts IRQ & NMI will push the B flag as being 0.
            SetFlag(ProcessorFlags.B, false);


            // disable further interupts
            SetFlag(ProcessorFlags.I, true);

            // set the PC to the IRQ vector
            PC = IRQ_Vect;
            ReadValueFromAddress(PC);
            var lo = fetchedByte;
            clockTicks++;

            PC++;
            ReadValueFromAddress(PC);
            var hi = fetchedByte;
            clockTicks++;

            PC = (ushort)((hi << 8) | lo);

            var arg = new OnInstructionExecutedEventArg
            {
                CurrentInstruction = currentInstruction,
                DecodedInstruction = "IRQ Request",
                A = A,
                X = X,
                Y = Y,
                PC = PC,
                SP = SP,
                ST = ST,
                RawData = $"{currentInstruction.OpCode:X2} {currentInstruction.Operand1:X2} {currentInstruction.Operand2:X2}".TrimEnd(),
                ClockTicks = clockTicks
            };
            bus?.Publish(arg);

        }

        private void HandleNMI()
        {
            // save return address to stack, hi byte first then lo byte
            var retAddr = (PC + currentInstruction.Length);
            WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
            DecreaseSP();

            WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
            DecreaseSP();

            //hardware interrupts IRQ & NMI will push the B flag as being 0.
            SetFlag(ProcessorFlags.B, false);

            // save the Processor Flags to stack
            WriteValueToAddress(SP, (byte)ST);
            DecreaseSP();

            // disable further interupts
            SetFlag(ProcessorFlags.I, true);

            // set the PC to the IRQ vector
            PC = NMI_Vect;
            ReadValueFromAddress(PC);
            var lo = fetchedByte;
            clockTicks++;

            PC++;
            ReadValueFromAddress(PC);
            var hi = fetchedByte;
            clockTicks++;

            PC = (ushort)((hi << 8) | lo);

            var arg = new OnInstructionExecutedEventArg
            {
                CurrentInstruction = currentInstruction,
                DecodedInstruction = "NMI Request",
                A = A,
                X = X,
                Y = Y,
                PC = PC,
                SP = SP,
                ST = ST,
                RawData = $"{currentInstruction.OpCode:X2} {currentInstruction.Operand1:X2} {currentInstruction.Operand2:X2}".TrimEnd(),
                ClockTicks = clockTicks
            };
            bus?.Publish(arg);
        }

        public void Dispose()
        {
            bus?.UnSubscribe<InteruptRequestEventArgs>(OnInteruptRequest);
            bus?.UnSubscribe<ResetEventArgs>(OnReset);
        }
    }
}
