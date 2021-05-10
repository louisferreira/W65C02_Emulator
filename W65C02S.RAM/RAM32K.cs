using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.RAM
{
    public class RAM32K : BaseIODevice
    {
        public RAM32K(Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(bus, startAddress, endAddress, mode)
        {
            // initialise stack
            for (int index = startAddress; index < endAddress; index++)
            {
                memory[index] = 0xEA;
            }
            for (ushort index = 0x0100; index <= 0x01FF; index++)
            {
                memory[index] = 0x00;
            }
        }
    }
}
