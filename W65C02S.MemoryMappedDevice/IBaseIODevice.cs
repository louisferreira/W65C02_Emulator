using W65C02S.Bus;

namespace W65C02S.MemoryMappedDevice
{
    public interface IBaseIODevice
    {
        string DeviceName { get; }
        int EndAddress { get; }
        ushort StartAddress { get; }
        DataBusMode Mode { get; }
    }
}