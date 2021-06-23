using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using W65C02.API.Interfaces;
using W65C02S.Engine;

namespace W65C02.IDE
{
    public class BusForm : Form
    {
        protected IBus bus;

        public BusForm()
        {

        }

        public BusForm(IBus bus)
        {
            this.bus = bus;
        }
    }

    public class EmulatorBusForm : BusForm
    {
        protected IEmulator emulator;

        public EmulatorBusForm()
        {

        }

        public EmulatorBusForm(IBus bus, IEmulator emulator) : base(bus)
        {
            this.bus = bus;
            this.emulator = emulator;
        }
    }
}
