using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W65C02.API.Enums;

namespace W65C02.API.EventArgs
{
    public class InteruptRequestEventArgs
    {
        public InteruptType InteruptType { get; set; }
    }
}
