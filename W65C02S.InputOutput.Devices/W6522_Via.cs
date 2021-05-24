using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.InputOutput.Devices
{
    public class W6522_Via : BaseIODevice
    {

        public W6522_Via(string deviceName, Bus.Bus bus, ushort startAddress, ushort endAddress, DataBusMode mode) : base(deviceName, bus, startAddress, endAddress, mode)
        {
        }

        protected override void ExecuteAddressAction(AddressBusEventArgs arg)
        {
            base.ExecuteAddressAction(arg);

            if (arg.Mode == DataBusMode.Write && arg.Address == 0x8000)
            {
                memory[arg.Address - startAddress] = 0xAF;
            }
        }
    }
}
