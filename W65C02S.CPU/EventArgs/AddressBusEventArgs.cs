using System;

namespace W65C02S.CPU
{
    public class AddressBusEventArgs : EventArgs
    {
        public ushort Address { get;  set; }
    }
}