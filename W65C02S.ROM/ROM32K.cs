using System;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;

namespace W65C02S.ROM
{
    public class ROM32K : IDisposable
    {
        private readonly Bus.Bus bus;
        private readonly byte[] memory = new byte[1024 * 32];
        private readonly ushort startAddress;
        private readonly int endAddress;
        public ROM32K(Bus.Bus bus, ushort startAddress)
        {
            this.bus = bus;
            this.startAddress = startAddress;
            this.endAddress = (int)(startAddress + memory.Length);

            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
            this.bus.Subscribe<RomLoadArgs>(Load);
        }

        public int Size => memory.Length;
        public ushort StartAddress => startAddress;
        public int EndAddress => endAddress;

        private void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (arg.Mode == Bus.DataBusMode.Read && arg.Address >= StartAddress && arg.Address < EndAddress)
            {
                var actualAddress = arg.Address - StartAddress;
                arg.Data = memory[actualAddress];
            }
        }

        public void Load(RomLoadArgs arg)
        {
            if (arg.Data.Length != memory.Length)
            {
                throw new InvalidOperationException($"Attempting to load ROM with incorrect data size. ROM size is {Size} bytes, and data load size id {arg.Data.Length} bytes.");
            }

            for (int index = 0; index < memory.Length; index++)
            {
                memory[index] = arg.Data[index];
            }
        }

        public void Dispose()
        {
            this.bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
            this.bus.UnSubscribe<RomLoadArgs>(Load);
        }
    }
}
