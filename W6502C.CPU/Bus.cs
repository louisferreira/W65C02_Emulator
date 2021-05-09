using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W6502C.CPU
{
    public class Bus
    {
        private ushort currentAddress = 0x00;
        private byte currentData = 0x00;
        private List<IDevice> devices = new List<IDevice>();

        public void Write(ushort address, byte data)
        {
            if(address >= 0x0000 && address <= 0xFFFF)
            {
                currentAddress = address;
                devices[0].Write(currentAddress, data);
            }
        }
        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0xFFFF)
            {
                currentAddress = address;
                currentData = GetDataFromDevice();
                return currentData;
            }
            return 0x0000;
        }

        private byte GetDataFromDevice()
        {
            return devices[0].Read(currentAddress);
        }

        public void Connect(IDevice device)
        {
            devices.Add(device);
        }
    }
}
