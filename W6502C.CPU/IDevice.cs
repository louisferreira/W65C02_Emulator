using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W6502C.CPU
{
    public interface IDevice
    {
        void Write(ushort address, byte data);
        byte Read(ushort address);
    }
}
