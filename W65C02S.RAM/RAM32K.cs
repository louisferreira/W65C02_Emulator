using System;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;

namespace W65C02S.RAM
{
    public class RAM32K : IDisposable
    {
        private readonly Bus.Bus bus;
        private byte[] memory = new byte[1024 * 32];
        private ushort startAddress;
        private ushort endAddress;
        public RAM32K(Bus.Bus bus, ushort startAddress)
        {
            this.bus = bus;
            this.startAddress = startAddress;
            this.endAddress = (ushort)(startAddress + memory.Length);

            // initialise stack
            for (int index = startAddress; index < endAddress; index++)
            {
                memory[index] = 0xEA;
            }
            for (ushort index = 0x0100; index <= 0x01FF; index++)
            {
                memory[index] = 0x00;
            }

            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
        }

        public int Size => memory.Length;
        public ushort StartAddress => startAddress;
        public ushort EndAddress => endAddress;

        private void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (arg.Address >= StartAddress && arg.Address < EndAddress)
            {
                if (arg.Mode == Bus.DataBusMode.Read)
                    arg.Data = memory[arg.Address];
                else
                    arg.Data = memory[arg.Address];
            }
        }


        public void Dispose()
        {
            this.bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
        }

    }
}
