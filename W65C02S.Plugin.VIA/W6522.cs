using System.Threading.Tasks;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02S.Engine.Devices;

namespace W65C02S.InputOutput.Devices
{
    public class W6522 : AbstractMemoryMappedDevice
    {
        internal const string name = "VIA6522";
        internal const ushort start = 0x00;
        internal const ushort end = 0x0F;


        public W6522(IBus bus) : base(bus)
        {
            this.DeviceName = name;
            this.MappedIO = IOMapping.IO0;
            Mode = DataBusMode.ReadWrite;
            memory = new byte[16];
            Enabled = true;
        }

        public override void SetIOAddress(ushort startAddress, ushort endAddress)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        protected override void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (Enabled && arg.AddressDecoder.IsMappedTo(this.MappedIO))
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
                    if(arg.Address == 0x8000) 
                    {
                        // simulate busy flag being updated to no longer busy
                        Task.Run(() => {
                            Task.Delay(500);
                            memory[arg.Address - StartAddress] = 0x88;
                        });
                    }
                }
                arg.DeviceName = DeviceName;
            }
        }


    }
}
