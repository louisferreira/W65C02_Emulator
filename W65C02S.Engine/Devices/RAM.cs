using System;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;

namespace W65C02S.Engine.Devices
{
    public class RAM : AbstractMemoryMappedDevice
    {

        public RAM(IBus bus, ushort startAddress = 0x0000) : base(bus)
        {
            this.DeviceName = "RAM 32k";
            MappedIO = IOMapping.RAM;
            
            var size = (1024 * 32);
            memory = new byte[size];

            StartAddress = startAddress;
            EndAddress = (ushort)((StartAddress - 1) + size);

        }

        public override void SetIOAddress(ushort startAddress, ushort endAddress)
        {
            
        }

        protected override void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (arg.AddressDecoder.RAM)
            {
                if(arg.Mode == DataBusMode.Read)
                {
                    arg.Data = memory[arg.Address];
                }
                if (arg.Mode == DataBusMode.Write)
                {
                    memory[arg.Address] = arg.Data;
                }
                arg.DeviceName = DeviceName;
            }
        }
    }
}
