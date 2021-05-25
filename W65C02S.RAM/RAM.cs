using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.RAM
{
    public class RAM : BaseIODevice
    {

        public RAM(string deviceName, Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(deviceName, bus, startAddress, endAddress, mode)
        {
            // initialise 
            //for (int index = startAddress; index < endAddress; index++)
            //{
            //    memory[index] = 0xFF;
            //}
        }
    }
}
