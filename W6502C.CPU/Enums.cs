using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W6502C.CPU
{
    [Flags]
    public enum ProcessorFlags
    {
        C = (1 << 0), // Carry Bit
        Z = (1 << 1), // Zero
        I = (1 << 2), // Disable Interrupts
        D = (1 << 3), // Decimal Mode (unused in this implementation)
        B = (1 << 4), // Break
        U = (1 << 5), // Unused
        V = (1 << 6), // Overflow
        N = (1 << 7), // Negative
    }

    public enum AddressModes
    {
        None = 0,
        Absolute,
        AbsoluteIndexedIndirect,
        AbsoluteIndexedWithX,
        AbsoluteIndexedWithY,
        AbsoluteIndirect,
        Accumulator,
        Immediate,
        Implied,
        ProgramCounterRelative,
        Stack,
        ZeroPage,
        ZeroPageIndexedIndirect,
        ZeroPageIndexedWithX,
        ZeroPageIndexedWithY,
        ZeroPageIndirect,
        ZeroPageIndirectIndexedWithY
    }
}
