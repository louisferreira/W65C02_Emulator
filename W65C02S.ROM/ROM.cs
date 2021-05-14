using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.ROM
{
    public class ROM : BaseIODevice
    {
        public ROM(Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(bus, startAddress, endAddress, mode)
        {
            bus.Subscribe<RomLoadArgs>(Load);
        }
        protected override string DeviceName => nameof(ROM);

        public void Load(RomLoadArgs arg)
        {
            if (arg.Data.Length > memory.Length)
            {
                throw new InvalidOperationException($"Attempting to load ROM with to large a data size. ROM size is {memory.Length} bytes, and data load size is {arg.Data.Length} bytes.");
            }

            var offset = memory.Length - arg.Data.Length;
            for (int index = 0; index < arg.Data.Length; index++)
            {
                memory[offset + index] = arg.Data[index];
            }
        }


    }
}
