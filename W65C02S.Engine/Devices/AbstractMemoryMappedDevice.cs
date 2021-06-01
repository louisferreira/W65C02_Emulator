using System;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;

namespace W65C02S.Engine.Devices
{
    public abstract class AbstractMemoryMappedDevice : IDisposable, IMemoryMappedDevice
    {
        protected readonly IBus bus;
        protected byte[] memory;

        public string DeviceName { get; set; }
        public ushort StartAddress { get; protected set; }
        public ushort EndAddress { get; protected set; }
        public bool Enabled { get; protected set; }

        public IOMapping MappedIO { get; protected set; }
        public DataBusMode Mode { get; protected set; }

        public AbstractMemoryMappedDevice(IBus bus)
        {
            this.bus = bus;
            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);

        }

        protected abstract void OnAddressChanged(AddressBusEventArgs arg);

        public void Dispose()
        {
            if(Enabled)
                bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
        }

        public abstract void SetIOAddress(ushort startAddress, ushort endAddress);
    }
}
