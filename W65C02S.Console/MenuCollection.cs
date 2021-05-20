using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02S.Console
{
    public class MenuCollection
    {
        public List<MenuItem> Items { get; set; }
        public int NumberOfLines { get; set; }

        public MenuCollection()
        {
            Items = new List<MenuItem>();
            NumberOfLines = 5;

        }
    }
}
