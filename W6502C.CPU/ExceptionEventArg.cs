using System;


namespace W6502C.CPU
{
    public class ExceptionEventArg : EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}
