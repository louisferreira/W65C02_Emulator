using System;
using W65C02S.Bus;
using W65C02S.CPU;

namespace W65C02S.MemoryMappedDevice
{
    public abstract class BaseIODevice : IDisposable
    {
        private readonly Bus.Bus bus;
        protected readonly byte[] memory;
        private readonly ushort startAddress;
        private readonly int endAddress;
        private readonly DataBusMode mode;

        public ushort StartAddress => startAddress;
        public int EndAddress => endAddress;
        public DataBusMode Mode => mode;
        protected abstract string DeviceName {get; }
        public BaseIODevice(Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode)
        {
            this.bus = bus;
            this.startAddress = startAddress;
            this.endAddress = endAddress;
            this.mode = mode;
            this.memory = new byte[(endAddress - startAddress) + 1];

            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);

        }

        private void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (arg.Address >= startAddress && arg.Address <= endAddress)
            {
                if(arg.Mode  == DataBusMode.Write && this.mode == DataBusMode.Read)
                {
                    throw new InvalidOperationException($"Attempt to write data to a readonly device ({DeviceName})");
                }

                if (arg.Mode == DataBusMode.Read && ((this.mode & DataBusMode.Read) == DataBusMode.Read))
                {
                    arg.Data = memory[arg.Address - startAddress];
                }
                
                if (arg.Mode == DataBusMode.Write && ((this.mode & DataBusMode.Write) == DataBusMode.Write))
                {
                    memory[arg.Address - startAddress] = arg.Data;
                }
                arg.DeviceName = DeviceName;
            }
        }

        public void Dispose()
        {
            this.bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
        }
    }
}
