using System;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02S.Engine.Devices;

namespace W65C02S.Plugin.ROM
{
    public class ROM32K : AbstractMemoryMappedDevice, IROM, IDisposable
    {
        private const int memorySizeKb = 32;
        public ROM32K(IBus bus) : base(bus)
        {
            DeviceName = $"ROM {memorySizeKb}Kb";
            MappedIO = IOMapping.ROM;
            Mode = DataBusMode.Read;
            Enabled = true;

            if(Enabled)
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
            var size = (1024 * memorySizeKb);

            if((endAddress - startAddress) > size)
            {
                throw new InvalidOperationException($"Start and End address range exceeds the range of device {DeviceName}. Requested range is {(endAddress - startAddress)} bytes, device memory size is {size} bytes");
            }

            memory = new byte[size];
            StartAddress = startAddress;
            EndAddress = endAddress; // (ushort)((StartAddress - 1) + size);
        }

        protected override void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (Enabled && arg.AddressDecoder.ROM)
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
        public void Disposed()
        {
            if (Enabled)
                bus.UnSubscribe<FlashROMArgs>(FlashROM);
        }
    }
}
