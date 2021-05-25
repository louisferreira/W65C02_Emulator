using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using W65C02S.Engine;

namespace W65C02S.Simulator
{
    static class Program
    {
        private static Bus.Bus bus;
        private static Emulator emulator;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var bus = new Bus.Bus())
            {
                using (Emulator emulator = new Emulator(bus))
                {
                    Application.Run(new frmMDIMain(bus, emulator));
                }
            }

        }
    }
}
