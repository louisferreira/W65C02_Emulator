﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02.API.EventArgs
{
    public class FlashROMArgs
    {
        public byte[] Data { get; set; }
        public bool UseOffset { get; set; }
    }
}