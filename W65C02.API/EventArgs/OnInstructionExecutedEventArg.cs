using System;
using W65C02.API.Enums;
using W65C02.API.Models;

namespace W65C02.API.EventArgs
{
    public class OnInstructionExecutedEventArg
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