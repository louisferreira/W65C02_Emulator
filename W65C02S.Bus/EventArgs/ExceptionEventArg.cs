using System;
using W65C02S.Bus;

namespace W65C02S.CPU
{
    public class ExceptionEventArg : EventArgs
    {
        public string ErrorMessage { get; set; }
        public ExceptionType ExceptionType { get; set; }
    }
}