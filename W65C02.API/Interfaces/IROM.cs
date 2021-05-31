using W65C02.API.EventArgs;

namespace W65C02.API.Interfaces
{
    public interface IROM : IMemoryMappedDevice
    {
        ushort StartAddress { get; }
        ushort EndAddress { get; }
        void FlashROM(FlashROMArgs arg);
    }
}