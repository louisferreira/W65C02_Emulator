﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU.Models;
using W65C02S.Engine.Parsers;

namespace W65C02S.CPU
{
    public partial class CPUCore : IDisposable
    {
        private ulong clockTicks = 0;           // 0 to 18,446,744,073,709,551,615
        private byte? fetchedByte;
        private ushort? operandAddress;
        private List<Instruction> InstructionTable;
        private Instruction currentInstruction;
        private readonly Bus.Bus bus;
        private bool stopCmdEffected = false;
        private bool interuptRequested = false;


        private const ushort IRQ_Vect = 0x0FFFE;
        private const ushort Rest_Vect = 0x0FFFC;
        private const ushort NMI_Vect = 0x0FFFA;

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

        public CPUCore(Bus.Bus bus)
        {
            this.bus = bus;
            bus.Subscribe<InteruptRequestEventArgs>(OnInteruptRequest);
            Initialise();
            SetupInstructionTable();
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
            stopCmdEffected = false;
        }
        public void Step()
        {
            if (stopCmdEffected)
            {
                var e = new ExceptionEventArg() { ErrorMessage = $"Proccessor has STOPed . A Reset is required." };
                bus.Publish(e);
                return;
            }
            Execute();
        }

        private void Execute()
        {
            if (IsFlagSet(ProcessorFlags.B) & !interuptRequested)
            {
                var e = new ExceptionEventArg() { ErrorMessage = $"Proccessor is in BREAK mode. Waiting for IRQ/NMI/Reset." };
                bus.Publish(e);
                return;
            }

            if (interuptRequested)
            {
                HandleIRQ();
                interuptRequested = false;
                return;
            }

            ReadValueFromAddress(PC);

            if (InstructionTable.Any(x => x.OpCode == fetchedByte.Value))
            {
                currentInstruction = InstructionTable.First(x => x.OpCode == fetchedByte.Value);
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
                    currentInstruction.AddressModeAction();
                    currentInstruction.InstructionAction();
                    clockTicks += currentInstruction.Length;
                    
                }
                catch (NotImplementedException ex)
                {
                    stopCmdEffected = true;
                    var e = new ExceptionEventArg() { ErrorMessage = $"OpCode [${currentInstruction.OpCode:X2} ({currentInstruction.Mnemonic})]  not Implemented".PadRight(100, ' ') };
                    bus.Publish(e);
                }
                catch (Exception x)
                {
                    stopCmdEffected = true;
                    var e = new ExceptionEventArg() { ErrorMessage = x.Message.PadRight(100, ' ') };
                    bus.Publish(e);
                }
                finally {
                    RaiseInstructionExecuted();
                    if(currentInstruction.Mnemonic == "BRK" || currentInstruction.Mnemonic == "WAI")
                    {
                        var e = new ExceptionEventArg() { ErrorMessage = $"Processor halted with BRK/WAI instruction...".PadRight(100, ' '), ExceptionType = ExceptionType.Warning };
                        bus.Publish(e);
                    }
                    operandAddress = null;
                    fetchedByte = null;
                }
            }
            else
            {
                stopCmdEffected = true;
                var e = new ExceptionEventArg() { ErrorMessage = $"Unknown instruction: ${fetchedByte:X2}".PadRight(100, ' ')};
                bus.Publish(e);
            }

        }

        private void IncrementPC(sbyte amount = 1)
        {
            if (amount == 0)
                return;

            PC += (ushort)amount;

            if (PC > 0xFFFF)
            {
                PC = 0xFFFF;
            }
        }

        private void IncrementPC(byte amount = 1)
        {
            if (amount == 0)
                return;

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
                ST |= flag;
            else
                ST &= ~flag;
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
            bus.Publish(arg);
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
            bus.Publish(arg);
            clockTicks++;
        }

        private void RaiseInstructionExecuted()
        {
            var arg = new InstructionDisplayEventArg
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
            bus.Publish(arg);
        }

        private void OnInteruptRequest(InteruptRequestEventArgs arg)
        {
            if(arg.InteruptType == InteruptType.IRQ)
            {
                interuptRequested = true;
            }
            if (arg.InteruptType == InteruptType.NMI)
            {
                interuptRequested = true;
            }
        }

        private void HandleIRQ()
        {
            // save return address to stack, hi byte first then lo byte
            var retAddr = (PC + currentInstruction.Length);
            WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
            DecreaseSP();

            WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
            DecreaseSP();

            // save the Processor Flags to stack
            WriteValueToAddress(SP, (byte)ST);
            DecreaseSP();

            PC = IRQ_Vect;
            ReadValueFromAddress(PC);
            var lo = fetchedByte;
            clockTicks++;

            PC++;
            ReadValueFromAddress(PC);
            var hi = fetchedByte;
            clockTicks++;

            PC = (ushort)((hi << 8) | lo);

            var arg = new InstructionDisplayEventArg
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
            bus.Publish(arg);
            SetFlag(ProcessorFlags.B, false);
        }

        private void HandleNMI()
        {
            
        }

        private void SetupInstructionTable()
        {
            InstructionTable = new List<Instruction>() {
                { new Instruction{OpCode = 0X6D, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = Absolute, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X7D, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X79, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0X69, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = Immediate, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0X65, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = ZeroPage, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X61, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0X75, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X72, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = ZeroPageIndirect, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0X71, Mnemonic = "ADC", InstructionAction = ADC, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0X2D, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = Absolute, OperationDescription = "A & M->A", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X3D, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X39, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0X29, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = Immediate, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0X25, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = ZeroPage, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X21, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0X35, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X32, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = ZeroPageIndirect, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0X31, Mnemonic = "AND", InstructionAction = AND, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0X0E, Mnemonic = "ASL", InstructionAction = ASL, AddressModeAction = Absolute, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X1E, Mnemonic = "ASL", InstructionAction = ASL, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X0A, Mnemonic = "ASL", InstructionAction = ASL, AddressModeAction = Accumulator, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "A", Length = 3} },
                { new Instruction{OpCode = 0X06, Mnemonic = "ASL", InstructionAction = ASL, AddressModeAction = ZeroPage, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X16, Mnemonic = "ASL", InstructionAction = ASL, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X0F, Mnemonic = "BBR0", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 0 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X1F, Mnemonic = "BBR1", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 1 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X2F, Mnemonic = "BBR2", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 2 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X3F, Mnemonic = "BBR3", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 3 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X4F, Mnemonic = "BBR4", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 4 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X5F, Mnemonic = "BBR5", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 5 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X6F, Mnemonic = "BBR6", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 6 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X7F, Mnemonic = "BBR7", InstructionAction = BBR, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 7 reset", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X8F, Mnemonic = "BBS0", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 0 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X9F, Mnemonic = "BBS1", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 1 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XAF, Mnemonic = "BBS2", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 2 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XBF, Mnemonic = "BBS3", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 3 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XCF, Mnemonic = "BBS4", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 4 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XDF, Mnemonic = "BBS5", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 5 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XEF, Mnemonic = "BBS6", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 6 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XFF, Mnemonic = "BBS7", InstructionAction = BBS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch on bit 7 set", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X90, Mnemonic = "BCC", InstructionAction = BCC, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch C = 0", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XB0, Mnemonic = "BCS", InstructionAction = BCS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if C=1", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XF0, Mnemonic = "BEQ", InstructionAction = BEQ, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if Z=1", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X2C, Mnemonic = "BIT", InstructionAction = BIT, AddressModeAction = Absolute, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X3C, Mnemonic = "BIT", InstructionAction = BIT, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X89, Mnemonic = "BIT", InstructionAction = BIT, AddressModeAction = Immediate, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0X24, Mnemonic = "BIT", InstructionAction = BIT, AddressModeAction = ZeroPage, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X34, Mnemonic = "BIT", InstructionAction = BIT, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X30, Mnemonic = "BMI", InstructionAction = BMI, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if N=1", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0XD0, Mnemonic = "BNE", InstructionAction = BNE, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if Z=0", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X10, Mnemonic = "BPL", InstructionAction = BPL, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if N=0", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X80, Mnemonic = "BRA", InstructionAction = BRA, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch Always", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X00, Mnemonic = "BRK", InstructionAction = BRK, AddressModeAction = Stack, OperationDescription = "Break", FlagsAffected = "BZ", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X50, Mnemonic = "BVC", InstructionAction = BVC, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if V=0", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X70, Mnemonic = "BVS", InstructionAction = BVS, AddressModeAction = ProgramCounterRelative, OperationDescription = "Branch if V=1", FlagsAffected = "", AddressCode = "r", Length = 2} },
                { new Instruction{OpCode = 0X18, Mnemonic = "CLC", InstructionAction = CLC, AddressModeAction = Implied, OperationDescription = "C -> 0", FlagsAffected = "C", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XD8, Mnemonic = "CLD", InstructionAction = CLD, AddressModeAction = Implied, OperationDescription = "0 -> D", FlagsAffected = "D", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X58, Mnemonic = "CLI", InstructionAction = CLI, AddressModeAction = Implied, OperationDescription = "0 -> 1", FlagsAffected = "I", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XB8, Mnemonic = "CLV", InstructionAction = CLV, AddressModeAction = Implied, OperationDescription = "0 -> V", FlagsAffected = "V", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XCD, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = Absolute, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XDD, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0XD9, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0XC9, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = Immediate, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XC5, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = ZeroPage, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XC1, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0XD5, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XD2, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = ZeroPageIndirect, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0XD1, Mnemonic = "CMP", InstructionAction = CMP, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0XEC, Mnemonic = "CPX", InstructionAction = CPX, AddressModeAction = Absolute, OperationDescription = "X - M", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XE0, Mnemonic = "CPX", InstructionAction = CPX, AddressModeAction = Immediate, OperationDescription = "X - M", FlagsAffected = "NZC", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XE4, Mnemonic = "CPX", InstructionAction = CPX, AddressModeAction = ZeroPage, OperationDescription = "X - M", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XCC, Mnemonic = "CPY", InstructionAction = CPY, AddressModeAction = Absolute, OperationDescription = "Y - M", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XC0, Mnemonic = "CPY", InstructionAction = CPY, AddressModeAction = Immediate, OperationDescription = "Y - M", FlagsAffected = "NZC", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XC4, Mnemonic = "CPY", InstructionAction = CPY, AddressModeAction = ZeroPage, OperationDescription = "Y - M", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XCE, Mnemonic = "DEC", InstructionAction = DEC, AddressModeAction = Absolute, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XDE, Mnemonic = "DEC", InstructionAction = DEC, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X3A, Mnemonic = "DEC", InstructionAction = DEC, AddressModeAction = Accumulator, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "A", Length = 3} },
                { new Instruction{OpCode = 0XC6, Mnemonic = "DEC", InstructionAction = DEC, AddressModeAction = ZeroPage, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XD6, Mnemonic = "DEC", InstructionAction = DEC, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XCA, Mnemonic = "DEX", InstructionAction = DEX, AddressModeAction = Implied, OperationDescription = "X - 1 -> X", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X88, Mnemonic = "DEY", InstructionAction = DEY, AddressModeAction = Implied, OperationDescription = "Y - 1 -> Y", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X4D, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = Absolute, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X5D, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X59, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0X49, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = Immediate, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0X45, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = ZeroPage, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X41, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0X55, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X52, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = ZeroPageIndirect, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0X51, Mnemonic = "EOR", InstructionAction = EOR, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0XEE, Mnemonic = "INC", InstructionAction = INC, AddressModeAction = Absolute, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XFE, Mnemonic = "INC", InstructionAction = INC, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X1A, Mnemonic = "INC", InstructionAction = INC, AddressModeAction = Accumulator, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "A", Length = 3} },
                { new Instruction{OpCode = 0XE6, Mnemonic = "INC", InstructionAction = INC, AddressModeAction = ZeroPage, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XF6, Mnemonic = "INC", InstructionAction = INC, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XE8, Mnemonic = "INX", InstructionAction = INX, AddressModeAction = Implied, OperationDescription = "X + 1 -> X", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XC8, Mnemonic = "INY", InstructionAction = INY, AddressModeAction = Implied, OperationDescription = "Y + 1 -> Y", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X4C, Mnemonic = "JMP", InstructionAction = JMP, AddressModeAction = Absolute, OperationDescription = "Jump to new location", FlagsAffected = "", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X7C, Mnemonic = "JMP", InstructionAction = JMP, AddressModeAction = AbsoluteIndexedIndirect, OperationDescription = "Jump to new location", FlagsAffected = "", AddressCode = "(a,x)", Length = 3} },
                { new Instruction{OpCode = 0X6C, Mnemonic = "JMP", InstructionAction = JMP, AddressModeAction = AbsoluteIndirect, OperationDescription = "Jump to new location", FlagsAffected = "", AddressCode = "(a)", Length = 3} },
                { new Instruction{OpCode = 0X20, Mnemonic = "JSR", InstructionAction = JSR, AddressModeAction = Absolute, OperationDescription = "Jump to Subroutine", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XAD, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = Absolute, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XBD, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0XB9, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0XA9, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = Immediate, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XA5, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = ZeroPage, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XA1, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0XB5, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XB2, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = ZeroPageIndirect, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0XB1, Mnemonic = "LDA", InstructionAction = LDA, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0XAE, Mnemonic = "LDX", InstructionAction = LDX, AddressModeAction = Absolute, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XBE, Mnemonic = "LDX", InstructionAction = LDX, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0XA2, Mnemonic = "LDX", InstructionAction = LDX, AddressModeAction = Immediate, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XA6, Mnemonic = "LDX", InstructionAction = LDX, AddressModeAction = ZeroPage, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XB6, Mnemonic = "LDX", InstructionAction = LDX, AddressModeAction = ZeroPageIndexedWithY, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "zp,y", Length = 2} },
                { new Instruction{OpCode = 0XAC, Mnemonic = "LDY", InstructionAction = LDY, AddressModeAction = Absolute, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XBC, Mnemonic = "LDY", InstructionAction = LDY, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0XA0, Mnemonic = "LDY", InstructionAction = LDY, AddressModeAction = Immediate, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XA4, Mnemonic = "LDY", InstructionAction = LDY, AddressModeAction = ZeroPage, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XB4, Mnemonic = "LDY", InstructionAction = LDY, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X4E, Mnemonic = "LSR", InstructionAction = LSR, AddressModeAction = Absolute, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X5E, Mnemonic = "LSR", InstructionAction = LSR, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X4A, Mnemonic = "LSR", InstructionAction = LSR, AddressModeAction = Accumulator, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "A", Length = 3} },
                { new Instruction{OpCode = 0X46, Mnemonic = "LSR", InstructionAction = LSR, AddressModeAction = ZeroPage, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X56, Mnemonic = "LSR", InstructionAction = LSR, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XEA, Mnemonic = "NOP", InstructionAction = NOP, AddressModeAction = Implied, OperationDescription = "No Operation", FlagsAffected = "", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X0D, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = Absolute, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X1D, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X19, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0X09, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = Immediate, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0X05, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = ZeroPage, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X01, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0X15, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X12, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = ZeroPageIndirect, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0X11, Mnemonic = "ORA", InstructionAction = ORA, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0X48, Mnemonic = "PHA", InstructionAction = PHA, AddressModeAction = Stack, OperationDescription = "A -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X08, Mnemonic = "PHP", InstructionAction = PHP, AddressModeAction = Stack, OperationDescription = "P -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0XDA, Mnemonic = "PHX", InstructionAction = PHX, AddressModeAction = Stack, OperationDescription = "X -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X5A, Mnemonic = "PHY", InstructionAction = PHY, AddressModeAction = Stack, OperationDescription = "Y -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X68, Mnemonic = "PLA", InstructionAction = PLA, AddressModeAction = Stack, OperationDescription = "S + 1->S, Ms -> A", FlagsAffected = "NZ", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X28, Mnemonic = "PLP", InstructionAction = PLP, AddressModeAction = Stack, OperationDescription = "S + 1->S, Ms -> P", FlagsAffected = "NVDIZC", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0XFA, Mnemonic = "PLX", InstructionAction = PLX, AddressModeAction = Stack, OperationDescription = "S + 1->S, Ms -> X", FlagsAffected = "NZ", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X7A, Mnemonic = "PLY", InstructionAction = PLY, AddressModeAction = Stack, OperationDescription = "S + 1->S, Ms -> Y", FlagsAffected = "NZ", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X07, Mnemonic = "RMB0", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 0", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X17, Mnemonic = "RMB1", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 1", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X27, Mnemonic = "RMB2", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 2", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X37, Mnemonic = "RMB3", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 3", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X47, Mnemonic = "RMB4", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 4", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X57, Mnemonic = "RMB5", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 5", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X67, Mnemonic = "RMB6", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 6", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X77, Mnemonic = "RMB7", InstructionAction = RMB, AddressModeAction = ZeroPage, OperationDescription = "Reset Memory Bit 7", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X2E, Mnemonic = "ROL", InstructionAction = ROL, AddressModeAction = Absolute, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X3E, Mnemonic = "ROL", InstructionAction = ROL, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X2A, Mnemonic = "ROL", InstructionAction = ROL, AddressModeAction = Accumulator, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "A", Length = 3} },
                { new Instruction{OpCode = 0X26, Mnemonic = "ROL", InstructionAction = ROL, AddressModeAction = ZeroPage, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X36, Mnemonic = "ROL", InstructionAction = ROL, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X6E, Mnemonic = "ROR", InstructionAction = ROR, AddressModeAction = Absolute, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X7E, Mnemonic = "ROR", InstructionAction = ROR, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X6A, Mnemonic = "ROR", InstructionAction = ROR, AddressModeAction = Accumulator, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "A", Length = 3} },
                { new Instruction{OpCode = 0X66, Mnemonic = "ROR", InstructionAction = ROR, AddressModeAction = ZeroPage, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X76, Mnemonic = "ROR", InstructionAction = ROR, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X40, Mnemonic = "RTI", InstructionAction = RTI, AddressModeAction = Stack, OperationDescription = "Return from Interrupt", FlagsAffected = "NVDIZC", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0X60, Mnemonic = "RTS", InstructionAction = RTS, AddressModeAction = Stack, OperationDescription = "Return from Subroutine", FlagsAffected = "", AddressCode = "s", Length = 1} },
                { new Instruction{OpCode = 0XED, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = Absolute, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0XFD, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0XF9, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0XE9, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = Immediate, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "#", Length = 2} },
                { new Instruction{OpCode = 0XE5, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = ZeroPage, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XE1, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0XF5, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XF2, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = ZeroPageIndirect, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0XF1, Mnemonic = "SBC", InstructionAction = SBC, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0X38, Mnemonic = "SEC", InstructionAction = SEC, AddressModeAction = Implied, OperationDescription = "1 -> C", FlagsAffected = "C", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XF8, Mnemonic = "SED", InstructionAction = SED, AddressModeAction = Implied, OperationDescription = "1 -> D", FlagsAffected = "D", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X78, Mnemonic = "SEI", InstructionAction = SEI, AddressModeAction = Implied, OperationDescription = "1 -> I", FlagsAffected = "I", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X87, Mnemonic = "SMB0", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 0", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X97, Mnemonic = "SMB1", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 1", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XA7, Mnemonic = "SMB2", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 2", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XB7, Mnemonic = "SMB3", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 3", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XC7, Mnemonic = "SMB4", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 4", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XD7, Mnemonic = "SMB5", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 5", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XE7, Mnemonic = "SMB6", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 6", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XF7, Mnemonic = "SMB7", InstructionAction = SMB, AddressModeAction = ZeroPage, OperationDescription = "Set Memory Bit 7", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X8D, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = Absolute, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X9D, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X99, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = AbsoluteIndexedWithY, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "a,y", Length = 3} },
                { new Instruction{OpCode = 0X85, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = ZeroPage, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X81, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = ZeroPageIndexedIndirect, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "(zp, x)", Length = 2} },
                { new Instruction{OpCode = 0X95, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X92, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = ZeroPageIndirect, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "(zp)", Length = 2} },
                { new Instruction{OpCode = 0X91, Mnemonic = "STA", InstructionAction = STA, AddressModeAction = ZeroPageIndirectIndexedWithY, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "(zp),y", Length = 2} },
                { new Instruction{OpCode = 0XDB, Mnemonic = "STP", InstructionAction = STP, AddressModeAction = Implied, OperationDescription = "STOP (1-> PHI2)", FlagsAffected = "", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X8E, Mnemonic = "STX", InstructionAction = STX, AddressModeAction = Absolute, OperationDescription = "X -> M", FlagsAffected = "", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X86, Mnemonic = "STX", InstructionAction = STX, AddressModeAction = ZeroPage, OperationDescription = "X -> M", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X96, Mnemonic = "STX", InstructionAction = STX, AddressModeAction = ZeroPageIndexedWithY, OperationDescription = "X -> M", FlagsAffected = "", AddressCode = "zp,y", Length = 2} },
                { new Instruction{OpCode = 0X8C, Mnemonic = "STY", InstructionAction = STY, AddressModeAction = Absolute, OperationDescription = "Y -> M", FlagsAffected = "", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X84, Mnemonic = "STY", InstructionAction = STY, AddressModeAction = ZeroPage, OperationDescription = "Y -> M", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X94, Mnemonic = "STY", InstructionAction = STY, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "Y -> M", FlagsAffected = "", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0X9C, Mnemonic = "STZ", InstructionAction = STZ, AddressModeAction = Absolute, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X9E, Mnemonic = "STZ", InstructionAction = STZ, AddressModeAction = AbsoluteIndexedWithX, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "a,x", Length = 3} },
                { new Instruction{OpCode = 0X64, Mnemonic = "STZ", InstructionAction = STZ, AddressModeAction = ZeroPage, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X74, Mnemonic = "STZ", InstructionAction = STZ, AddressModeAction = ZeroPageIndexedWithX, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "zp,x", Length = 2} },
                { new Instruction{OpCode = 0XAA, Mnemonic = "TAX", InstructionAction = TAX, AddressModeAction = Implied, OperationDescription = "A -> X", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XA8, Mnemonic = "TAY", InstructionAction = TAY, AddressModeAction = Implied, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X1C, Mnemonic = "TRB", InstructionAction = TRB, AddressModeAction = Absolute, OperationDescription = "~A ^ M -> M", FlagsAffected = "Z", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X14, Mnemonic = "TRB", InstructionAction = TRB, AddressModeAction = ZeroPage, OperationDescription = "~A ^ M -> M", FlagsAffected = "Z", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0X0C, Mnemonic = "TSB", InstructionAction = TSB, AddressModeAction = Absolute, OperationDescription = "A V M -> M", FlagsAffected = "Z", AddressCode = "a", Length = 3} },
                { new Instruction{OpCode = 0X04, Mnemonic = "TSB", InstructionAction = TSB, AddressModeAction = ZeroPage, OperationDescription = "A V M -> M", FlagsAffected = "Z", AddressCode = "zp", Length = 2} },
                { new Instruction{OpCode = 0XBA, Mnemonic = "TSX", InstructionAction = TSX, AddressModeAction = Implied, OperationDescription = "S -> X", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X8A, Mnemonic = "TXA", InstructionAction = TXA, AddressModeAction = Implied, OperationDescription = "X -> A", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X9A, Mnemonic = "TXS", InstructionAction = TXS, AddressModeAction = Implied, OperationDescription = "X -> S", FlagsAffected = "", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0X98, Mnemonic = "TYA", InstructionAction = TYA, AddressModeAction = Implied, OperationDescription = "Y -> A", FlagsAffected = "NZ", AddressCode = "i", Length = 1} },
                { new Instruction{OpCode = 0XCB, Mnemonic = "WAI", InstructionAction = WAI, AddressModeAction = Implied, OperationDescription = "0 -> RDY", FlagsAffected = "", AddressCode = "i", Length = 1} }

            };
        }

        public void Dispose()
        {
            bus.UnSubscribe<InteruptRequestEventArgs>(OnInteruptRequest);
        }
    }
}
