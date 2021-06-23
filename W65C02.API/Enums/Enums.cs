using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02.API.Enums
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
    public enum RunMode
    {
        Debug = 0,
        Run = 1
    }

    public enum IOMapping
    {
                Unknown = -1,
                IO0 = 0,
                IO1 = 1,
                IO2 = 2,
                IO3 = 3,
                IO4 = 4,
                IO5 = 5,
                IO6 = 6,
                IO7 = 7,
                IO8 = 8,
                IO9 = 9,
                IOA = 10,
                IOB = 11,
                IOC = 12,
                IOD = 13,
                IOE = 14,
                IOF = 15,
                RAM = 99,
                ROM = 100
    }

    public enum ResetType
    {
        Hardware = 0,
        Software = 1
    }
}
