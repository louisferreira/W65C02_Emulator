using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using W65C02.API.Interfaces;
using W65C02.API.Models;
using W65C02S.Engine;

namespace W65C02.IDE
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mappings = LoadConfig();

            using (var bus = new W65C02S.Engine.Devices.Bus())
            using(var emulator = new Emulator(bus, mappings))
            {
                Application.Run(new FormMain(bus, emulator));
            }

        }
        private static List<MapConfig> LoadConfig()
        {
            if (!System.IO.File.Exists(".\\config.json"))
                return default;

            try
            {
                var jsonData = System.IO.File.ReadAllText(".\\config.json");
                var items = JsonConvert.DeserializeObject<IEnumerable<MapConfig>>(jsonData);
                return (List<MapConfig>)items;
            }
            catch (Exception ex)
            {
                //DisplayError(ex.Message, ExceptionType.Error);
            }

            return default;
        }
    }
}
