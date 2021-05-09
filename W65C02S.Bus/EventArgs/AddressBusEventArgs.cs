﻿using System;
using W65C02S.Bus;

namespace W65C02S.CPU
{
    public class AddressBusEventArgs
    {
        public ushort Address { get;  set; }
        public DataBusMode Mode { get; set; }
        public byte Data { get; set; }
    }
}