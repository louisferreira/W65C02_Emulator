using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.ROM
{
    public class ROM : BaseIODevice
    {
        public ROM(string deviceName, Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(deviceName, bus, startAddress, endAddress, mode)
        {
            bus.Subscribe<RomLoadArgs>(Load);
        }

        public void Load(RomLoadArgs arg)
        {
            if (arg.Data.Length > memory.Length)
            {
                throw new InvalidOperationException($"Attempting to load ROM with to large a data size. ROM size is {memory.Length} bytes, and data load size is {arg.Data.Length} bytes.");
            }

            var offset = arg.UseOffset ? memory.Length - arg.Data.Length : 0;
            for (int index = 0; index < arg.Data.Length; index++)
            {
                memory[offset + index] = arg.Data[index];
            }
        }


    }
}
