using System;

namespace W65C02S.Clock
{
    public class ClockEventArg : EventArgs
    {
        public ClockEdge Edge { get; internal set; }
    }
}
