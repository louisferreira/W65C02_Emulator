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
        /// <summary>
        /// Carry Bit
        /// </summary>
        C = 1 << 0,
        /// <summary>
        /// Zero
        /// </summary>
        Z = 1 << 1,
        /// <summary>
        /// Disable Interrupts
        /// </summary>
        I = 1 << 2,
        /// <summary>
        /// Decimal Mode
        /// </summary>
        D = 1 << 3,
        /// <summary>
        /// Break
        /// </summary>
        B = 1 << 4,
        /// <summary>
        /// Unused
        /// </summary>
        U = 1 << 5,
        /// <summary>
        /// Overflow
        /// </summary>
        V = 1 << 6,
        /// <summary>
        /// Negative
        /// </summary>
        N = 1 << 7, 
    }
    public enum DataBusMode
    {
        Read = 1,
        Write = 2,
        ReadWrite = 3
    }
    public enum InteruptType
    {
        IRQ = 1,
        NMI = 2
    }
    public enum ExceptionType
    {
        Error = 0,
        Warning = 1,
        Debug = 2
    }
}
