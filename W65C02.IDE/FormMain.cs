using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FontAwesome.Sharp;
using Newtonsoft.Json;
using Plugin.Manager;
using W65C02.API.Interfaces;
using W65C02.API.Models;

namespace W65C02.IDE
{
    public partial class FormMain : EmulatorBusForm
    {
        private IconButton currentMenuButton;
        private Panel leftBorderMenuButton;
        private Form activeForm = null;
        private readonly IBus bus;
        private IROM rom;

        private struct RGBColors
        {
            public static Color ButtonDefaultForeColor = Color.Gainsboro;
            public static Color ButtonDefaultBackColor = Color.FromArgb(31, 30, 68);
            public static Color ButtonActiveForeColor = Color.FromArgb(24,161,251);

            public static Color color1 = Color.FromArgb(172, 126, 241);
            public static Color color2 = Color.FromArgb(249,118,176);
            public static Color color3 = Color.FromArgb(253,138,114);
            public static Color color4 = Color.FromArgb(95,77,221);
            public static Color color5 = Color.FromArgb(249,88,155);
        }

        public FormMain(IBus bus, IEmulator emulator): base (bus, emulator)
        {
            InitializeComponent();
            leftBorderMenuButton = new Panel();
            leftBorderMenuButton.Size = new Size(2, 40);
            pnlMenu.Controls.Add(leftBorderMenuButton);
            this.bus = bus;
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            // load plugin devices
            var mappings = LoadConfig();
            var extraDevices = new GenericPluginLoader<IMemoryMappedDevice>().LoadAll(bus);
            foreach (var device in extraDevices)
            {
                //check if device name is listed in config, and add it if found
                if (device.Enabled)
                {
                    if (device is IROM)
                        rom = (IROM)device;

                    emulator.AddDevice(device);
                }
            }

            var connectedDevices = emulator.GetConnectedDevices();
            if (connectedDevices == null || connectedDevices.Count == 0)
                throw new InvalidOperationException("There are no devices connected to the system. Please add at least one plugin to the plugins folder.");
            if (rom == null)
                throw new InvalidOperationException("There is no ROM connected to the system. Please add at least one plugin that implements IROM interface to the plugins folder.");

            // map devices to memory locations
            foreach (var plugin in connectedDevices)
            {
                var mapping = mappings.FirstOrDefault(x => x.ChipSelect == plugin.MappedIO.ToString());
                if (mapping == null)
                    continue;

                var startAddress = ushort.Parse(mapping.StartAddress, System.Globalization.NumberStyles.HexNumber);
                var endAddress = ushort.Parse(mapping.EndAddress, System.Globalization.NumberStyles.HexNumber);
                plugin.SetIOAddress(startAddress, endAddress);
            }

        }
        private List<MapConfig> LoadConfig()
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

        private void ActivateMenuButton(object sender, Color color)
        {
            if(sender != null)
            {
                DeativateMenuButton();
                currentMenuButton = (IconButton)sender;
                currentMenuButton.BackColor = Color.FromArgb(37, 36, 81);
                currentMenuButton.ForeColor = color;
                currentMenuButton.IconColor = color;

                leftBorderMenuButton.BackColor = color;
                leftBorderMenuButton.Location = new Point(0, currentMenuButton.Location.Y);
                leftBorderMenuButton.Visible = true;
                leftBorderMenuButton.BringToFront();
            }
        }
        private void DeativateMenuButton()
        {
            if(currentMenuButton != null)
            {
                currentMenuButton.BackColor = RGBColors.ButtonDefaultBackColor;
                currentMenuButton.ForeColor = RGBColors.ButtonDefaultForeColor;
                currentMenuButton.IconColor = RGBColors.ButtonDefaultForeColor;
            }
        }

        private void iconButton_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(sender, RGBColors.ButtonActiveForeColor);
            ShowForm(sender);
        }
        private void ShowForm(object sender)
        {
            if (activeForm != null)
                activeForm.Close();

            Form childForm = null;

            switch (((IconButton)sender).Name)
            {
                case "btnMemoryMonitor":
                    childForm = new FormMemoryMonitor(bus);
                    break;
                default:
                    return;
            }

            activeForm = childForm;
            
            childForm.TopMost = true;
            childForm.StartPosition = FormStartPosition.CenterScreen;
            childForm.Show();
        }

    }
}
