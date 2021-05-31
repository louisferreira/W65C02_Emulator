using System;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;

namespace W65C02S.Engine.Devices
{
    public class ROM : AbstractMemoryMappedDevice, IROM
    {
        public ROM(IBus bus, ushort startAddress) : base(bus)
        {
            this.DeviceName = "ROM 28k";
            MappedIO = IOMapping.ROM;
            var size = (1024 * 28);
            memory = new byte[size];

            StartAddress = startAddress;
            EndAddress = (ushort)((StartAddress - 1) + size);
            
            bus.Subscribe<FlashROMArgs>(FlashROM);
        }


        

        public void FlashROM(FlashROMArgs arg)
        {
            if (arg.Data.Length > memory.Length)
            {
                throw new InvalidOperationException($"Attempting to load ROM with to large a data size. ROM size is {memory.Length} bytes, and data load size is {arg.Data.Length} bytes.");
            }

            var offset = arg.UseOffset ? memory.Length - arg.Data.Length : 0;
            for (int index = 0; index < arg.Data.Length; index++)
            {
                memory[offset + index] = arg.Data[index];
            }
        }

        public override void SetIOAddress(ushort startAddress, ushort endAddress)
        {
            
        }

        protected override void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (arg.AddressDecoder.ROM)
            {
                if(arg.Mode == DataBusMode.Read)
                {
                    arg.Data = memory[arg.Address - StartAddress];
                }
                arg.DeviceName = DeviceName;
            }
        }
    }
}
