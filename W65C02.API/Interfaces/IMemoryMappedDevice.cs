using W65C02.API.Enums;
using W65C02.API.EventArgs;

namespace W65C02.API.Interfaces
{
    public interface IMemoryMappedDevice
    {
        string DeviceName { get; }
        ushort StartAddress { get; }
        ushort EndAddress { get; }
        bool Enabled { get; }
        IOMapping MappedIO { get; }
        DataBusMode Mode { get; }
        void SetIOAddress(ushort startAddress, ushort endAddress);
        void Dispose();
    }
}