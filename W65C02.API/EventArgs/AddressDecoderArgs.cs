using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W65C02.API.Enums;

namespace W65C02.API.EventArgs
{
    public class AddressDecoderArgs
    {
        public bool ROM { get; set; }
        public bool RAM { get; set; }
        public bool IO0 { get; set; }
        public bool IO1 { get; set; }
        public bool IO2 { get; set; }
        public bool IO3 { get; set; }
        public bool IO4 { get; set; }
        public bool IO5 { get; set; }
        public bool IO6 { get; set; }
        public bool IO7 { get; set; }
        public bool IO8 { get; set; }
        public bool IO9 { get; set; }
        public bool IOA { get; set; }
        public bool IOB { get; set; }
        public bool IOC { get; set; }
        public bool IOD { get; set; }
        public bool IOE { get; set; }
        public bool IOF { get; set; }

        public bool IsMappedTo(IOMapping mapping)
        {
            switch (mapping)
            {
                case IOMapping.IO0:
                    return IO0;
                case IOMapping.IO1:
                    return IO1;
                case IOMapping.IO2:
                    return IO2;
                case IOMapping.IO3:
                    return IO3;
                case IOMapping.IO4:
                    return IO4;
                case IOMapping.IO5:
                    return IO5;
                case IOMapping.IO6:
                    return IO6;
                case IOMapping.IO7:
                    return IO7;
                case IOMapping.IO8:
                    return IO8;
                case IOMapping.IO9:
                    return IO9;
                case IOMapping.IOA:
                    return IOA;
                case IOMapping.IOB:
                    return IOB;
                case IOMapping.IOC:
                    return IOC;
                case IOMapping.IOD:
                    return IOD;
                case IOMapping.IOE:
                    return IOE;
                case IOMapping.IOF:
                    return IOF;
                default:
                    return false;
            }
        }
    }
}
