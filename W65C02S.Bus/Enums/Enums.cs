using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02S.Bus
{
    [Flags]
    public enum ProcessorFlags
    {
        C = 1 << 0, // Carry Bit
        Z = 1 << 1, // Zero
        I = 1 << 2, // Disable Interrupts
        D = 1 << 3, // Decimal Mode (unused in this implementation)
        B = 1 << 4, // Break
        U = 1 << 5, // Unused
        V = 1 << 6, // Overflow
        N = 1 << 7, // Negative
    }
    public enum DataBusMode
    {
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }
}
