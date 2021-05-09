using System;
using System.Collections.Generic;
using System.Linq;

namespace W6502C.CPU
{
    public partial class W65C02S
    {
        public event EventHandler<OutputEventArg> OnDataRead;
        public event EventHandler<OutputEventArg> OnDataWrite;
        public event EventHandler<OutputEventArg> OnBreakPoint;

        public event EventHandler<ExceptionEventArg> OnError;
        public event EventHandler<InstructionEventArg> OnInstructionExecuted;

        private readonly Bus bus = null;
        private byte? fetchedByte;
        private ushort? operandAddress;
        private bool runMode = false;
        private List<Instruction> Records;
        public List<ushort> BreakPoints;
        private Instruction currentInstruction = null;
        

        public byte A { get; set; }             // Accumulator Register
        public byte X { get; set; }             // X Register
        public byte Y { get; set; }             // Y Register
        public ushort SP { get; set; }          // Stack Pointer
        public ushort PC { get; set; }          // Program Counter
        public ProcessorFlags ST { get; set; }  // Status Register

        public W65C02S(Bus bus)
        {
            this.bus = bus;
            BreakPoints = new List<ushort>();
            SetupInstructionTable();
            Reset();
        }


        public void Reset()
        {

            A = 0;
            X = 0;
            Y = 0;
            SP = 0x0000;
            ST = ProcessorFlags.U;

            PC = 0xFFFC;
            operandAddress = PC;
            FetchByte(PC);
            var lo = fetchedByte;

            PC++;
            operandAddress = PC;
            FetchByte(PC);
            var hi = fetchedByte;

            PC = (ushort)((hi << 8) | lo);

            // initialise stack
            for (ushort index = 0x0100; index <= 0x01FF; index++)
            {
                bus.Write(index, 0x00);
            }
        }

        public void Run()
        {
            while ((ST & ProcessorFlags.B) != ProcessorFlags.B & runMode)
            {
                if (BreakPoints.Contains(PC))
                {
                    if(OnBreakPoint != null)
                    {
                        var e = new OutputEventArg
                        {
                            Address = PC
                        };
                        OnBreakPoint.Invoke(this, e);
                    }
                    return;
                }
                else
                    Execute();
            }
        }

        public void Step()
        {
            Execute();
            runMode = true;
        }

        public void AddBreakPoint(ushort brkPnt)
        {
            if (!BreakPoints.Contains(brkPnt))
            {
                BreakPoints.Add(brkPnt);
            }

        }
        public void RemoveBreakPoint(ushort brkPnt)
        {
            if (BreakPoints.Contains(brkPnt))
            {
                BreakPoints.Remove(brkPnt);
            }
        }

        private void Execute()
        {
            byte incrPC = 0x00;
            if (IsFlagSet(ProcessorFlags.B))
            {
                if (OnError != null)
                {
                    var e = new ExceptionEventArg() { ErrorMessage = $"Proccessor is in BREAK mode. Reset is required." };
                    OnError(this, e);
                    return;
                }
            }

            operandAddress = null;
            fetchedByte = null;
            FetchByte(PC);
            if (Records.Any(x => x.OpCode == fetchedByte.Value))
            {
                currentInstruction = Records.First(x => x.OpCode == fetchedByte.Value);

                if (currentInstruction.Length > 1)
                {
                    var opcode = fetchedByte.Value;
                    byte[] operands = new byte[currentInstruction.Length -1];
                    for (ushort index = 1; index < currentInstruction.Length; index++)
                    {
                        FetchByte((ushort)(PC + index));
                        operands[index - 1] = fetchedByte.Value;
                    }
                    currentInstruction.Set(operands);
                }
                else
                {
                    currentInstruction.Set(null);
                }

                RaiseInstructionExecuted();
                try
                {
                    incrPC = currentInstruction.AddressMode();
                    currentInstruction.Action(incrPC);
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
                if (OnError != null)
                {
                    ST = (ST | ProcessorFlags.B);
                    var e = new ExceptionEventArg() { ErrorMessage = $"OpCode [0x{fetchedByte:X2}] not Implemented" };
                    OnError(this, e);
                }
            }

        }

        private void FetchByte(ushort address)
        {
            fetchedByte = bus.Read(address);
            if(OnDataRead != null)
            {
                var e = new OutputEventArg
                {
                    Address = address,
                    Data = fetchedByte.Value,
                    Type = "r"
                };
                OnDataRead.Invoke(this, e);
            }
        }

        private void WriteByte(ushort address, byte data)
        {
            bus.Write(address, data);
            if (OnDataWrite != null)
            {
                var e = new OutputEventArg
                {
                    Address = address,
                    Data = data,
                    Type = "W"
                };
                OnDataWrite.Invoke(this, e);
            }
        }

        void SetFlag(ProcessorFlags flag, bool isOn)
        {
            if (isOn)
                ST |= flag;
            else
                ST &= ~flag;
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

        private bool IsFlagSet(ProcessorFlags flag)
        {
            return ((ST & flag) == flag);
        }


        private void RaiseInstructionExecuted()
        {
            if(OnInstructionExecuted != null)
            {
                var bytes = $"{currentInstruction.OpCode:X2} {currentInstruction.Operand1:X2} {currentInstruction.Operand2:X2}";

                var arg = new InstructionEventArg
                {
                    Nmeumonic = currentInstruction.Mnemonic,
                    InstructionLength = currentInstruction.Length,
                    InstructionAddress = PC,
                    OpCode = currentInstruction.OpCode,
                    AddressMode = currentInstruction.AddressCode,
                    Operand1 = currentInstruction.Operand1,
                    Operand2 = currentInstruction.Operand2,
                    DisAssembledInstruction = DisAssemble(currentInstruction.AddressCode) + "  ...[" + bytes.TrimEnd() + "]..."
                };
                OnInstructionExecuted.Invoke(this, arg);
                runMode = !arg.RunMode;
            }
        }

        private string DisAssemble(string addressCode)
        {
            var operandFormatString = GetAddressModeFormat(addressCode);
            var operands = currentInstruction.Operand2.HasValue 
                ?String.Format(operandFormatString, currentInstruction.Operand2, currentInstruction.Operand1)
                : String.Format(operandFormatString, currentInstruction.Operand1, "");
            return $"{currentInstruction.Mnemonic} {operands}";
        }

        private string GetAddressModeFormat(string addMode)
        {
            switch (addMode)
            {
                case "a":
                    return "${0:X2}{1:X2} (Absolute)";
                case "(a,x)":
                    return "(${0:X2},${1:X2}) (Absolute Indexed Indirect)";
                case "a,x":
                    return "${0:X2},${1:X2} (Absolute Indexed with X)";
                case "a,y":
                    return "${0:X2},${1:X2} (Absolute Indexed with Y)";
                case "(a)":
                    return "(${0:X2}) (Absolute Indirect)";
                case "A":
                    return "${0:X2} (Accumulator)";
                case "#":
                    return "#${0:X2} (Immediate)";
                case "i":
                    return "(Implied)";
                case "r":
                    return "${0:X2} (Program Counter Relative)";
                case "s":
                    return "(Stack Pointer)";
                case "zp":
                    return "${0:X2} (Zero Page)";
                case "(zp,x)":
                    return "(${0:X2},X) (Zero Page Indexed Indirect)";
                case "zp,x":
                    return "${0:X2},X (Zero Page Indexed with X)";
                case "zp,y":
                    return "${0:X2},Y (Zero Page Indexed with Y)";
                case "(zp)":
                    return "(${0:X2}) (Zero Page Indirect)";
                case "(zp),y":
                    return "(${0:X2}),Y (Zero Page Indirect Indexed with Y)";
                default:
                    return "";
            }
        }

        
    }

}
