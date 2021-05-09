using System;
using W65C02S.CPU.Models;

namespace W65C02S.CPU
{
    public class InstructionEventArg : EventArgs
    {
        public Instruction CurrentInstruction { get; set; }
    }

    public class InstructionDisplayEventArg : InstructionEventArg
    {
        public string DecodedInstruction { get; set; }
        public string RawData { get; set; }
        public byte A_Reg { get; set; }
        public byte X_Reg { get; set; }
        public byte Y_Reg { get; set; }
        public byte ST_Reg { get; set; }
        public ushort PC { get; set; }
        public ushort SP { get; set; }
        public double ClockTicks { get; set; }


    }
}