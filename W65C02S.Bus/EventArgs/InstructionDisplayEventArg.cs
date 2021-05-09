using System;
using W65C02S.Bus;
using W65C02S.CPU.Models;

namespace W65C02S.CPU
{
    public class InstructionDisplayEventArg
    {
        public Instruction CurrentInstruction { get; set; }
        public string DecodedInstruction { get; set; }
        public string RawData { get; set; }
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public ProcessorFlags ST { get; set; }
        public ushort PC { get; set; }
        public ushort SP { get; set; }
        public double ClockTicks { get; set; }


    }
}