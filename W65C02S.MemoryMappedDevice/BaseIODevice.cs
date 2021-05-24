using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;

namespace W65C02S.MemoryMappedDevice
{
    public abstract class BaseIODevice : IDisposable, IBaseIODevice
    {
        protected readonly Bus.Bus bus;
        protected readonly byte[] memory;
        protected readonly ushort startAddress;
        protected readonly int endAddress;
        protected readonly DataBusMode mode;
        protected bool RequestedAddressIsInRange;

        public ushort StartAddress => startAddress;
        public int EndAddress => endAddress;
        public DataBusMode Mode => mode;
        public string DeviceName { get; set; }
        public bool ChipSelected { get; set; }
        public bool OutputEnabled { get; set; }

        public BaseIODevice(string deviceName, Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode)
        {
            this.bus = bus;
            this.startAddress = startAddress;
            this.endAddress = endAddress;
            this.mode = mode;
            this.memory = new byte[(endAddress - startAddress) + 1];
            this.DeviceName = deviceName;
            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);

        }
        protected virtual void ExecuteAddressAction(AddressBusEventArgs arg)
        {
            if (arg.Mode == DataBusMode.Write && this.mode == DataBusMode.Read)
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

        private void OnAddressChanged(AddressBusEventArgs arg)
        {
            ValidateAddress(arg.Address);

            if (!RequestedAddressIsInRange)
                return;

            ExecuteAddressAction(arg);
            
        }

        private void ValidateAddress(ushort address)
        {
            RequestedAddressIsInRange = (address >= startAddress && address <= endAddress);
        }

        public void Dispose()
        {
            this.bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
        }
    }
}
