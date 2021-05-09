using System;

namespace W65C02S.CPU
{
    public enum RW
    {
        Read = 0,
        Write = 1
    }

    public class DataBusEventArgs : EventArgs
    {
        public RW Mode { get; internal set; }
        public byte Data { get; set; }
    }
}