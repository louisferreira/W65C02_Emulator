using System;
using W65C02.API.Enums;

namespace W65C02.API.EventArgs
{
    public class AddressBusEventArgs
    {
        public ushort Address { get; set; }
        public byte Data { get; set; }
        public DataBusMode Mode { get; set; }
        public string DeviceName { get; set; }
        public AddressDecoderArgs AddressDecoder { get; set; }
    }
}