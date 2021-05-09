using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using W65C02S.Bus;
using W65C02S.CPU.Models;
using static W65C02S.CPU.Enums.Enums;

namespace W65C02S.CPU
{
    public partial class CPUCore : IDisposable
    {
        private ulong clockTicks = 0;           // 0 to 18,446,744,073,709,551,615
        private byte? fetchedByte;
        private byte? lastDataBusByte;
        private ushort? operandAddress;
        private List<Instruction> Records;
        private Instruction currentInstruction;
        private readonly AddressBus addressBus;
        private readonly DataBus dataBus;
        private Guid busToken;

        public byte A { get; set; }             // Accumulator Register
        public byte X { get; set; }             // X Register
        public byte Y { get; set; }             // Y Register
        public ushort SP { get; set; }          // Stack Pointer
        public ushort PC { get; set; }          // Program Counter
        public ProcessorFlags ST { get; internal set; }  // Status Register
        public double ClockTicks => clockTicks;



        //public event EventHandler<AddressBusEventArgs> OnAddressChanged;
        //public event EventHandler<DataBusEventArgs> OnDataBussAccess;
        public event EventHandler<InstructionEventArg> OnInstructionExecuted;
        public event EventHandler<ExceptionEventArg> OnError;

        public CPUCore(AddressBus addressBus, DataBus dataBus)
        {
            Initialise();
            SetupInstructionTable();
            this.addressBus = addressBus;
            this.dataBus = dataBus;

            busToken = this.dataBus.Subscribe(OnDataBusEvent);

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
            
            for (ushort index = 0x0100; index <= 0x01FF; index++)
            {
                PC = index;
                WriteValueToAddress(PC, 0x00);
            }

            clockTicks = 5;
            PC = 0xFFFC;
            ReadValueFromAddress(PC);
            var lo = fetchedByte;
            clockTicks++;

            PC++;
            ReadValueFromAddress(PC);
            var hi = fetchedByte;
            clockTicks++;

            PC = (ushort)((hi << 8) | lo);
        }
        public void Step()
        {
            Execute();
        }

        private void Execute()
        {
            byte incrPC = 0x00;
            if (IsFlagSet(ProcessorFlags.B))
            {
                var e = new ExceptionEventArg() { ErrorMessage = $"Proccessor is in BREAK mode. Reset is required." };
                OnError?.Invoke(this, e);
                return;
            }

            operandAddress = null;
            fetchedByte = null;
            ReadValueFromAddress(PC);

            if (Records.Any(x => x.OpCode == fetchedByte.Value))
            {
                currentInstruction = Records.First(x => x.OpCode == fetchedByte.Value);

                if (currentInstruction.Length > 1)
                {
                    byte[] operands = new byte[currentInstruction.Length - 1];
                    for (ushort index = 1; index < currentInstruction.Length; index++)
                    {
                        ReadValueFromAddress((ushort)(PC + index));
                        operands[index - 1] = fetchedByte.Value;
                    }
                    currentInstruction.Set(operands);
                }

                try
                {
                    incrPC = currentInstruction.AddressMode();
                    currentInstruction.Action(incrPC);
                    RaiseInstructionExecuted();
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                    {
                        var e = new ExceptionEventArg { ErrorMessage = ex.Message };
                        OnError.Invoke(this, e);
                    }
                }
            }
            else
            {
                ST = (ST | ProcessorFlags.B);
                var e = new ExceptionEventArg() { ErrorMessage = $"OpCode [0x{fetchedByte:X2}] not Implemented" };
                OnError?.Invoke(this, e);
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
            addressBus.Publish(address);
            while (fetchedByte == null)
            {
                Thread.Sleep(100);
            }

        }
        private void OnDataBusEvent(byte data)
        {
            lastDataBusByte = data;
            fetchedByte = data;
        }

        private void WriteValueToAddress(ushort address, byte data)
        {
            addressBus.Publish(address);
            dataBus.Publish(data);
        }

        private void RaiseInstructionExecuted()
        {
            if (OnInstructionExecuted != null)
            {
                var arg = new InstructionEventArg
                {
                    CurrentInstruction = currentInstruction
            };
                OnInstructionExecuted.Invoke(this, arg);
            }
        }

        private void SetupInstructionTable()
        {
            Records = new List<Instruction>() {
                { new Instruction{OpCode = 0X6D, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X7D, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X79, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0X69, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0X65, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X61, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0X75, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X72, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0X71, Mnemonic = "ADC", Action = ADC, OperationDescription = "A + M + C -> A", FlagsAffected = "NVZC", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0X2D, Mnemonic = "AND", Action = AND, OperationDescription = "A & M->A", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X3D, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X39, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0X29, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0X25, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X21, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0X35, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X32, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0X31, Mnemonic = "AND", Action = AND, OperationDescription = "A & M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0X0E, Mnemonic = "ASL", Action = ASL, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X1E, Mnemonic = "ASL", Action = ASL, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X0A, Mnemonic = "ASL", Action = ASL, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "A", AddressMode = Accumulator, Length = 3} },
                { new Instruction{OpCode = 0X06, Mnemonic = "ASL", Action = ASL, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X16, Mnemonic = "ASL", Action = ASL, OperationDescription = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X0F, Mnemonic = "BBR0", Action = BBR, OperationDescription = "Branch on bit 0 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X1F, Mnemonic = "BBR1", Action = BBR, OperationDescription = "Branch on bit 1 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X2F, Mnemonic = "BBR2", Action = BBR, OperationDescription = "Branch on bit 2 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X3F, Mnemonic = "BBR3", Action = BBR, OperationDescription = "Branch on bit 3 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X4F, Mnemonic = "BBR4", Action = BBR, OperationDescription = "Branch on bit 4 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X5F, Mnemonic = "BBR5", Action = BBR, OperationDescription = "Branch on bit 5 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X6F, Mnemonic = "BBR6", Action = BBR, OperationDescription = "Branch on bit 6 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X7F, Mnemonic = "BBR7", Action = BBR, OperationDescription = "Branch on bit 7 reset", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X8F, Mnemonic = "BBS0", Action = BBS, OperationDescription = "Branch on bit 0 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X9F, Mnemonic = "BBS1", Action = BBS, OperationDescription = "Branch on bit 1 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XAF, Mnemonic = "BBS2", Action = BBS, OperationDescription = "Branch on bit 2 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XBF, Mnemonic = "BBS3", Action = BBS, OperationDescription = "Branch on bit 3 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XCF, Mnemonic = "BBS4", Action = BBS, OperationDescription = "Branch on bit 4 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XDF, Mnemonic = "BBS5", Action = BBS, OperationDescription = "Branch on bit 5 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XEF, Mnemonic = "BBS6", Action = BBS, OperationDescription = "Branch on bit 6 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XFF, Mnemonic = "BBS7", Action = BBS, OperationDescription = "Branch on bit 7 set", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X90, Mnemonic = "BCC", Action = BCC, OperationDescription = "Branch C = 0", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XB0, Mnemonic = "BCS", Action = BCS, OperationDescription = "Branch if C=1", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XF0, Mnemonic = "BEQ", Action = BEQ, OperationDescription = "Branch if Z=1", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X2C, Mnemonic = "BIT", Action = BIT, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X3C, Mnemonic = "BIT", Action = BIT, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X89, Mnemonic = "BIT", Action = BIT, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0X24, Mnemonic = "BIT", Action = BIT, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X34, Mnemonic = "BIT", Action = BIT, OperationDescription = "A ^ M", FlagsAffected = "NVZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X30, Mnemonic = "BMI", Action = BMI, OperationDescription = "Branch if N=1", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0XD0, Mnemonic = "BNE", Action = BNE, OperationDescription = "Branch if Z=0", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X10, Mnemonic = "BPL", Action = BPL, OperationDescription = "Branch if N=0", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X80, Mnemonic = "BRA", Action = BRA, OperationDescription = "Branch Always", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X00, Mnemonic = "BRK", Action = BRK, OperationDescription = "Break", FlagsAffected = "BZ", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X50, Mnemonic = "BVC", Action = BVC, OperationDescription = "Branch if V=0", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X70, Mnemonic = "BVS", Action = BVS, OperationDescription = "Branch if V=1", FlagsAffected = "", AddressCode = "r", AddressMode = ProgramCounterRelative, Length = 2} },
                { new Instruction{OpCode = 0X18, Mnemonic = "CLC", Action = CLC, OperationDescription = "C -> 0", FlagsAffected = "C", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XD8, Mnemonic = "CLD", Action = CLD, OperationDescription = "0 -> D", FlagsAffected = "D", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X58, Mnemonic = "CLI", Action = CLI, OperationDescription = "0 -> 1", FlagsAffected = "I", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XB8, Mnemonic = "CLV", Action = CLV, OperationDescription = "0 -> V", FlagsAffected = "V", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XCD, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XDD, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0XD9, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0XC9, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XC5, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XC1, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0XD5, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XD2, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0XD1, Mnemonic = "CMP", Action = CMP, OperationDescription = "A - M", FlagsAffected = "NZC", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0XEC, Mnemonic = "CPX", Action = CPX, OperationDescription = "X - M", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XE0, Mnemonic = "CPX", Action = CPX, OperationDescription = "X - M", FlagsAffected = "NZC", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XE4, Mnemonic = "CPX", Action = CPX, OperationDescription = "X - M", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XCC, Mnemonic = "CPY", Action = CPY, OperationDescription = "Y - M", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XC0, Mnemonic = "CPY", Action = CPY, OperationDescription = "Y - M", FlagsAffected = "NZC", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XC4, Mnemonic = "CPY", Action = CPY, OperationDescription = "Y - M", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XCE, Mnemonic = "DEC", Action = DEC, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XDE, Mnemonic = "DEC", Action = DEC, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X3A, Mnemonic = "DEC", Action = DEC, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "A", AddressMode = Accumulator, Length = 3} },
                { new Instruction{OpCode = 0XC6, Mnemonic = "DEC", Action = DEC, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XD6, Mnemonic = "DEC", Action = DEC, OperationDescription = "Decrement addressed location", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XCA, Mnemonic = "DEX", Action = DEX, OperationDescription = "X - 1 -> X", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X88, Mnemonic = "DEY", Action = DEY, OperationDescription = "Y - 1 -> Y", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X4D, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X5D, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X59, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0X49, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0X45, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X41, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0X55, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X52, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0X51, Mnemonic = "EOR", Action = EOR, OperationDescription = "A v M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0XEE, Mnemonic = "INC", Action = INC, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XFE, Mnemonic = "INC", Action = INC, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X1A, Mnemonic = "INC", Action = INC, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "A", AddressMode = Accumulator, Length = 3} },
                { new Instruction{OpCode = 0XE6, Mnemonic = "INC", Action = INC, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XF6, Mnemonic = "INC", Action = INC, OperationDescription = "Increment addressed location", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XE8, Mnemonic = "INX", Action = INX, OperationDescription = "X + 1 -> X", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XC8, Mnemonic = "INY", Action = INY, OperationDescription = "Y + 1 -> Y", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X4C, Mnemonic = "JMP", Action = JMP, OperationDescription = "Jump to new location", FlagsAffected = "", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X7C, Mnemonic = "JMP", Action = JMP, OperationDescription = "Jump to new location", FlagsAffected = "", AddressCode = "(a,x)", AddressMode = AbsoluteIndexedIndirect, Length = 3} },
                { new Instruction{OpCode = 0X6C, Mnemonic = "JMP", Action = JMP, OperationDescription = "Jump to new location", FlagsAffected = "", AddressCode = "(a)", AddressMode = AbsoluteIndirect, Length = 3} },
                { new Instruction{OpCode = 0X20, Mnemonic = "JSR", Action = JSR, OperationDescription = "Jump to Subroutine", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XAD, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XBD, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0XB9, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0XA9, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XA5, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XA1, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0XB5, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XB2, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0XB1, Mnemonic = "LDA", Action = LDA, OperationDescription = "M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0XAE, Mnemonic = "LDX", Action = LDX, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XBE, Mnemonic = "LDX", Action = LDX, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0XA2, Mnemonic = "LDX", Action = LDX, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XA6, Mnemonic = "LDX", Action = LDX, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XB6, Mnemonic = "LDX", Action = LDX, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "zp,y", AddressMode = ZeroPageIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0XAC, Mnemonic = "LDY", Action = LDY, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XBC, Mnemonic = "LDY", Action = LDY, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0XA0, Mnemonic = "LDY", Action = LDY, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XA4, Mnemonic = "LDY", Action = LDY, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XB4, Mnemonic = "LDY", Action = LDY, OperationDescription = "M -> Y", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X4E, Mnemonic = "LSR", Action = LSR, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X5E, Mnemonic = "LSR", Action = LSR, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X4A, Mnemonic = "LSR", Action = LSR, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "A", AddressMode = Accumulator, Length = 3} },
                { new Instruction{OpCode = 0X46, Mnemonic = "LSR", Action = LSR, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X56, Mnemonic = "LSR", Action = LSR, OperationDescription = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XEA, Mnemonic = "NOP", Action = NOP, OperationDescription = "No Operation", FlagsAffected = "", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X0D, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X1D, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X19, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0X09, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0X05, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X01, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0X15, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X12, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0X11, Mnemonic = "ORA", Action = ORA, OperationDescription = "A | M -> A", FlagsAffected = "NZ", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0X48, Mnemonic = "PHA", Action = PHA, OperationDescription = "A -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X08, Mnemonic = "PHP", Action = PHP, OperationDescription = "P -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0XDA, Mnemonic = "PHX", Action = PHX, OperationDescription = "X -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X5A, Mnemonic = "PHY", Action = PHY, OperationDescription = "Y -> Ms, S-1 -> S", FlagsAffected = "", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X68, Mnemonic = "PLA", Action = PLA, OperationDescription = "S + 1->S, Ms -> A", FlagsAffected = "NZ", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X28, Mnemonic = "PLP", Action = PLP, OperationDescription = "S + 1->S, Ms -> P", FlagsAffected = "NVDIZC", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0XFA, Mnemonic = "PLX", Action = PLX, OperationDescription = "S + 1->S, Ms -> X", FlagsAffected = "NZ", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X7A, Mnemonic = "PLY", Action = PLY, OperationDescription = "S + 1->S, Ms -> Y", FlagsAffected = "NZ", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X07, Mnemonic = "RMB0", Action = RMB, OperationDescription = "Reset Memory Bit 0", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X17, Mnemonic = "RMB1", Action = RMB, OperationDescription = "Reset Memory Bit 1", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X27, Mnemonic = "RMB2", Action = RMB, OperationDescription = "Reset Memory Bit 2", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X37, Mnemonic = "RMB3", Action = RMB, OperationDescription = "Reset Memory Bit 3", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X47, Mnemonic = "RMB4", Action = RMB, OperationDescription = "Reset Memory Bit 4", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X57, Mnemonic = "RMB5", Action = RMB, OperationDescription = "Reset Memory Bit 5", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X67, Mnemonic = "RMB6", Action = RMB, OperationDescription = "Reset Memory Bit 6", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X77, Mnemonic = "RMB7", Action = RMB, OperationDescription = "Reset Memory Bit 7", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X2E, Mnemonic = "ROL", Action = ROL, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X3E, Mnemonic = "ROL", Action = ROL, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X2A, Mnemonic = "ROL", Action = ROL, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "A", AddressMode = Accumulator, Length = 3} },
                { new Instruction{OpCode = 0X26, Mnemonic = "ROL", Action = ROL, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X36, Mnemonic = "ROL", Action = ROL, OperationDescription = "C <- 7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X6E, Mnemonic = "ROR", Action = ROR, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X7E, Mnemonic = "ROR", Action = ROR, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X6A, Mnemonic = "ROR", Action = ROR, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "A", AddressMode = Accumulator, Length = 3} },
                { new Instruction{OpCode = 0X66, Mnemonic = "ROR", Action = ROR, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X76, Mnemonic = "ROR", Action = ROR, OperationDescription = "C -> 7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X40, Mnemonic = "RTI", Action = RTI, OperationDescription = "Return from Interrupt", FlagsAffected = "NVDIZC", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0X60, Mnemonic = "RTS", Action = RTS, OperationDescription = "Return from Subroutine", FlagsAffected = "", AddressCode = "s", AddressMode = Stack, Length = 1} },
                { new Instruction{OpCode = 0XED, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0XFD, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0XF9, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0XE9, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "#", AddressMode = Immediate, Length = 2} },
                { new Instruction{OpCode = 0XE5, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XE1, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0XF5, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XF2, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0XF1, Mnemonic = "SBC", Action = SBC, OperationDescription = "A - M - (~C) -> A", FlagsAffected = "NVZC", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0X38, Mnemonic = "SEC", Action = SEC, OperationDescription = "1 -> C", FlagsAffected = "C", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XF8, Mnemonic = "SED", Action = SED, OperationDescription = "1 -> D", FlagsAffected = "D", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X78, Mnemonic = "SEI", Action = SEI, OperationDescription = "1 -> I", FlagsAffected = "I", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X87, Mnemonic = "SMB0", Action = SMB, OperationDescription = "Set Memory Bit 0", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X97, Mnemonic = "SMB1", Action = SMB, OperationDescription = "Set Memory Bit 1", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XA7, Mnemonic = "SMB2", Action = SMB, OperationDescription = "Set Memory Bit 2", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XB7, Mnemonic = "SMB3", Action = SMB, OperationDescription = "Set Memory Bit 3", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XC7, Mnemonic = "SMB4", Action = SMB, OperationDescription = "Set Memory Bit 4", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XD7, Mnemonic = "SMB5", Action = SMB, OperationDescription = "Set Memory Bit 5", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XE7, Mnemonic = "SMB6", Action = SMB, OperationDescription = "Set Memory Bit 6", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XF7, Mnemonic = "SMB7", Action = SMB, OperationDescription = "Set Memory Bit 7", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X8D, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X9D, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X99, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "a,y", AddressMode = AbsoluteIndexedWithY, Length = 3} },
                { new Instruction{OpCode = 0X85, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X81, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "(zp, x)", AddressMode = ZeroPageIndexedIndirect, Length = 2} },
                { new Instruction{OpCode = 0X95, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X92, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "(zp)", AddressMode = ZeroPageIndirect, Length = 2} },
                { new Instruction{OpCode = 0X91, Mnemonic = "STA", Action = STA, OperationDescription = "A -> M", FlagsAffected = "", AddressCode = "(zp),y", AddressMode = ZeroPageIndirectIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0XDB, Mnemonic = "STP", Action = STP, OperationDescription = "STOP (1-> PHI2)", FlagsAffected = "", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X8E, Mnemonic = "STX", Action = STX, OperationDescription = "X -> M", FlagsAffected = "", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X86, Mnemonic = "STX", Action = STX, OperationDescription = "X -> M", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X96, Mnemonic = "STX", Action = STX, OperationDescription = "X -> M", FlagsAffected = "", AddressCode = "zp,y", AddressMode = ZeroPageIndexedWithY, Length = 2} },
                { new Instruction{OpCode = 0X8C, Mnemonic = "STY", Action = STY, OperationDescription = "Y -> M", FlagsAffected = "", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X84, Mnemonic = "STY", Action = STY, OperationDescription = "Y -> M", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X94, Mnemonic = "STY", Action = STY, OperationDescription = "Y -> M", FlagsAffected = "", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0X9C, Mnemonic = "STZ", Action = STZ, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X9E, Mnemonic = "STZ", Action = STZ, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "a,x", AddressMode = AbsoluteIndexedWithX, Length = 3} },
                { new Instruction{OpCode = 0X64, Mnemonic = "STZ", Action = STZ, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X74, Mnemonic = "STZ", Action = STZ, OperationDescription = "00 -> M", FlagsAffected = "", AddressCode = "zp,x", AddressMode = ZeroPageIndexedWithX, Length = 2} },
                { new Instruction{OpCode = 0XAA, Mnemonic = "TAX", Action = TAX, OperationDescription = "A -> X", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XA8, Mnemonic = "TAY", Action = TAY, OperationDescription = "M -> X", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X1C, Mnemonic = "TRB", Action = TRB, OperationDescription = "~A ^ M -> M", FlagsAffected = "Z", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X14, Mnemonic = "TRB", Action = TRB, OperationDescription = "~A ^ M -> M", FlagsAffected = "Z", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0X0C, Mnemonic = "TSB", Action = TSB, OperationDescription = "A V M -> M", FlagsAffected = "Z", AddressCode = "a", AddressMode = Absolute, Length = 3} },
                { new Instruction{OpCode = 0X04, Mnemonic = "TSB", Action = TSB, OperationDescription = "A V M -> M", FlagsAffected = "Z", AddressCode = "zp", AddressMode = ZeroPage, Length = 2} },
                { new Instruction{OpCode = 0XBA, Mnemonic = "TSX", Action = TSX, OperationDescription = "S -> X", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X8A, Mnemonic = "TXA", Action = TXA, OperationDescription = "X -> A", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X9A, Mnemonic = "TXS", Action = TXS, OperationDescription = "X -> S", FlagsAffected = "", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0X98, Mnemonic = "TYA", Action = TYA, OperationDescription = "Y -> A", FlagsAffected = "NZ", AddressCode = "i", AddressMode = Implied, Length = 1} },
                { new Instruction{OpCode = 0XCB, Mnemonic = "WAI", Action = WAI, OperationDescription = "0 -> RDY", FlagsAffected = "", AddressCode = "i", AddressMode = Implied, Length = 1} }

            };
        }

        public void Dispose()
        {
            this.dataBus.UnSubscribe(busToken);
        }
    }
}
