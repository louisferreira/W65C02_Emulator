using System;
using W65C02.API.Enums;

namespace W65C02.API.EventArgs
{
    public class ExceptionEventArg : System.EventArgs
    {
        public string ErrorMessage { get; set; }
        public ExceptionType ExceptionType { get; set; }
    }
}