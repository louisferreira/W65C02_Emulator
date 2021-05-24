using System;
using W65C02S.Bus;

namespace W65C02S.Bus.EventArgs
{
    public class AddressBusEventArgs
    {
        public ushort Address { get; set; }
        public byte Data { get; set; }
        public DataBusMode Mode { get; set; }
        public string DeviceName { get; set; }
    }
}