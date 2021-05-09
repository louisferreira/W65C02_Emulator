using System;

namespace W65C02S.CPU
{
    public class ExceptionEventArg : EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}