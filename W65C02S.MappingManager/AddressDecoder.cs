using System;
using System.Collections.Generic;
using System.Linq;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02.API.Models;

namespace W65C02S.MappingManager
{
    public class AddressDecoder : IDisposable
    {
        private readonly IBus bus;
        private readonly List<MapConfig> mappings;

        public AddressDecoder(IBus bus, List<MapConfig> mappings)
        {
            this.bus = bus;
            this.mappings = mappings;
            this.bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
        }

        private void OnAddressChanged(AddressBusEventArgs arg)
        {
            arg.AddressDecoder = new AddressDecoderArgs();

            var map = mappings.FirstOrDefault(x => 
                ushort.Parse(x.StartAddress, System.Globalization.NumberStyles.HexNumber) <= arg.Address 
                && 
                ushort.Parse(x.EndAddress, System.Globalization.NumberStyles.HexNumber) >= arg.Address
            );

            if (map != null)
            {
                switch (map.ChipSelect)
                {
                    case "ROM":
                        arg.AddressDecoder.ROM = true;
                        break;
                    case "RAM":
                        arg.AddressDecoder.RAM = true;
                        break;
                    default:
                        arg.AddressDecoder.IO0 = map.ChipSelect == "IO0";
                        arg.AddressDecoder.IO1 = map.ChipSelect == "IO1";
                        arg.AddressDecoder.IO2 = map.ChipSelect == "IO2";
                        arg.AddressDecoder.IO3 = map.ChipSelect == "IO3";
                        arg.AddressDecoder.IO4 = map.ChipSelect == "IO4";
                        arg.AddressDecoder.IO5 = map.ChipSelect == "IO5";
                        arg.AddressDecoder.IO6 = map.ChipSelect == "IO6";
                        arg.AddressDecoder.IO7 = map.ChipSelect == "IO7";
                        arg.AddressDecoder.IO8 = map.ChipSelect == "IO8";
                        arg.AddressDecoder.IO9 = map.ChipSelect == "IO9";
                        arg.AddressDecoder.IOA = map.ChipSelect == "IOA";
                        arg.AddressDecoder.IOB = map.ChipSelect == "IOB";
                        arg.AddressDecoder.IOC = map.ChipSelect == "IOC";
                        arg.AddressDecoder.IOD = map.ChipSelect == "IOD";
                        arg.AddressDecoder.IOE = map.ChipSelect == "IOE";
                        arg.AddressDecoder.IOF = map.ChipSelect == "IOF";
                        break;
                }
               }
            }

        public void Dispose()
        {
            this.bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
        }
    }
}
