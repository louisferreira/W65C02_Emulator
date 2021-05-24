using System;
using W65C02S.Bus;

namespace W65C02S.Bus.EventArgs
{
    public class ExceptionEventArg : System.EventArgs
    {
        public string ErrorMessage { get; set; }
        public ExceptionType ExceptionType { get; set; }
    }
}