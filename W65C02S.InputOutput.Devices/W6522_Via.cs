using System;
using W65C02S.Bus;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.InputOutput.Devices
{
    public class W6522_Via : BaseIODevice
    {
        public W6522_Via(Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(bus, startAddress, endAddress, mode)
        {
        }

    }
}
