using System;
using static W6502C.CPU.W65C02S;

namespace W6502C.CPU
{
    public class OutputEventArg : EventArgs
    {
        public ushort Address { get; set; }
        public byte Data { get; set; }
        public string Type { get; set; }
        public string Nmeumonic { get; set; }
    }

    public class InstructionEventArg : EventArgs
    {
        public string Nmeumonic { get; set; }
        public byte OpCode { get; set; }
        public byte InstructionLength { get; set; }
        public ushort InstructionAddress { get; set; }
        public byte? Operand1 { get; set; }
        public byte? Operand2 { get; set; }
        public string AddressMode { get; set; }
        public string DisAssembledInstruction { get; set; }
        public bool RunMode { get; set; }
    }
}
