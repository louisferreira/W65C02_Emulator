using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02S.Engine.Devices;

namespace W65C02S.Plugin.RAM
{
    public class RAM16K : AbstractMemoryMappedDevice
    {
        private const int memorySizeKb = 16;
        public RAM16K(IBus bus): base(bus)
        {
            DeviceName = $"RAM {memorySizeKb}Kb";
            MappedIO = IOMapping.RAM;
            Mode = DataBusMode.ReadWrite;
            Enabled = false;
        }

        public override void SetIOAddress(ushort startAddress, ushort endAddress)
        {
            var size = (1024 * memorySizeKb);
            memory = new byte[size];
            StartAddress = startAddress;
            EndAddress = (ushort)((StartAddress - 1) + size);
        }

        protected override void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (Enabled && arg.AddressDecoder.RAM)
            {
                if (arg.Mode == DataBusMode.Read)
                {
                    arg.Data = memory[arg.Address - StartAddress];
                }
                if (arg.Mode == DataBusMode.Write)
                {
                    memory[arg.Address - StartAddress] = arg.Data;
                }
                arg.DeviceName = DeviceName;
            }
        }
    }
}
