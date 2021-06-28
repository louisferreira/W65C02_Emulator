using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using W65C02.API.Enums;

namespace W65C02.API.Interfaces
{
    public interface IEmulator
    {
        RunMode Mode { get; set; }

        void AddDevice(IMemoryMappedDevice device);
        void AddRemoveBreakPoint(ushort breakPoint);
        void ClearBreakFlag();
        void ClearInteruptFlag();
        void Dispose();
        List<ushort> GetBreakPoints();
        ReadOnlyCollection<IMemoryMappedDevice> GetConnectedDevices();
        bool IsFlagSet(ProcessorFlags flag);
        void LoadROM(byte[] data, bool offset);
        byte ReadMemoryLocation(ushort address);
        Task Reset();
        Task Run();
        void SendIRQ();
        void SendNMI();
        void SetPCValue(ushort inputValue);
        Task Step();
        void WriteMemoryLocation(ushort address, byte data);
    }
}