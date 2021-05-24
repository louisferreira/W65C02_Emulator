using W65C02S.Bus;

namespace W65C02S.MemoryMappedDevice
{
    public interface IBaseIODevice
    {
        bool ChipSelected { get; set; }
        bool OutputEnabled { get; set; }
        string DeviceName { get; }
        int EndAddress { get; }
        ushort StartAddress { get; }
        DataBusMode Mode { get; }
    }
}