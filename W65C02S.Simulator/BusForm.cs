using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using W65C02S.Engine;

namespace W65C02S.Simulator
{
    public class BusForm : Form
    {
        protected Bus.Bus bus;
        protected Emulator emulator;

        public BusForm()
        {

        }

        public BusForm(Bus.Bus bus, Emulator emulator)
        {
            if(this.bus == null) 
                this.bus = bus;
            if(this.emulator == null) 
                this.emulator = emulator;
        }

    }
}
