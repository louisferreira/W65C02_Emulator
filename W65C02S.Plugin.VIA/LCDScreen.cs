using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;

namespace W65C02S.Plugin.VIA
{
    public class LCDScreen : IMemoryMappedDevice
    {
        private byte[] memory;
        protected readonly IBus bus;

        public LCDScreen(IBus bus)
        {
            this.DeviceName = "LCD2004";
            this.Mode = DataBusMode.ReadWrite;
            this.MappedIO = IOMapping.IO4;
            memory = new byte[16];

            this.bus = bus;
            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);

        }

        public string DeviceName { get; }
        public ushort StartAddress { get; private set; }
        public ushort EndAddress { get; private set; }
        public IOMapping MappedIO { get; private set; }

        

        public DataBusMode Mode { get; private set; }

        public void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (arg.AddressDecoder.IsMappedTo(this.MappedIO))
            {
                if (arg.Mode == DataBusMode.Read && (Mode == DataBusMode.Read || Mode == DataBusMode.ReadWrite))
                {
                    // return data
                    arg.Data = memory[arg.Address - StartAddress];
                }
                if (arg.Mode == DataBusMode.Write && (Mode == DataBusMode.Write || Mode == DataBusMode.ReadWrite))
                {
                    // set data
                    memory[arg.Address - StartAddress] = arg.Data;
                }
                arg.DeviceName = DeviceName;
            }
        }

        public void SetIOAddress(ushort startAddress, ushort endAddress)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        public void Dispose()
        {
            this.bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
        }


    }
}
