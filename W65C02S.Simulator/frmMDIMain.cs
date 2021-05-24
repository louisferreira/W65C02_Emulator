using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.Engine;
using W65C02S.MemoryMappedDevice;

namespace W65C02S.Simulator
{
    public partial class frmMDIMain : Form
    {
        private Bus.Bus bus;
        private ROM.ROM rom;
        private Emulator emulator;
        private string appTitle = "W65C02 Simulator";
        private bool imageFileLoaded;

        public frmMDIMain(Bus.Bus bus)
        {
            InitializeComponent();
            this.Text = appTitle;

            this.bus = bus;
            emulator = new Emulator(bus);
            rom = new ROM.ROM("ROM", bus, 0x9000, 0xFFFF, DataBusMode.Read);
            emulator.AddDevice(rom);

            bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
            bus.Subscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
            bus.Subscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
            bus.Subscribe<ExceptionEventArg>(OnError);

            var list = emulator.GetConnectedDevices();

        }

        private void OnError(ExceptionEventArg obj)
        {
            throw new NotImplementedException();
        }

        private void OnInstructionExecuted(OnInstructionExecutedEventArg obj)
        {
            throw new NotImplementedException();
        }

        private void OnInstructionExecuting(OnInstructionExecutingEventArg obj)
        {
            throw new NotImplementedException();
        }

        private void OnAddressChanged(AddressBusEventArgs obj)
        {
            throw new NotImplementedException();
        }



        private void LoadImageFile()
        {
            var devices = emulator.GetConnectedDevices();
            if (devices.Count == 0)
            {
                MessageBox.Show("There are no devices connected to the bus. Please add at least one ROM, and one RAM device", "No ROM Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }
            if (!devices.Any(x => x.GetType().Name == "ROM"))
            {
                MessageBox.Show("There is no ROM devices connected to the bus. Please add a ROM device", "No ROM Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            var fileOpen = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                AddExtension = true,
                DefaultExt = ".bin",
                Filter = "Binary Files (*.bin)|*.bin|Rom files (*.rom)|*.rom",
                Title = "Select ROM image file.",
                InitialDirectory = "C:\\"
            };
            byte[] fileData;
            if (fileOpen.ShowDialog() == DialogResult.OK)
            {
                var file = fileOpen.FileName;
                fileData = System.IO.File.ReadAllBytes(file);
                var rom = devices.First(x => x.GetType().Name == "ROM");
                if (ValidateROMData(fileData, rom))
                    FlashROMData(fileData, rom);
                this.Text = $"{appTitle} - {file}";
                imageFileLoaded = true;
                this.mnuStartEmulator.Enabled = true;
            }
        }
        private bool ValidateROMData(byte[] fileData, IBaseIODevice rom)
        {
            if (fileData.Length > (rom.EndAddress - rom.StartAddress + 1))
            {
                MessageBox.Show("Binary data is too large to fit into ROM.", "Data too Large", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
           
                return true;
        }
        private void FlashROMData(byte[] fileData, IBaseIODevice rom)
        {
            var offset = false;
            if (fileData.Length < (rom.EndAddress - rom.StartAddress))
                offset = (MessageBox.Show("Binary file is smaller than ROM size. Do you want to offset this data to the end of the ROM memory?", "Offset Binary Data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);

            var arg = new FlashROMArgs
            {
                Data = fileData,
                UseOffset = offset
            };
            bus.Publish(arg);

        }

        

        private void frmMDIMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // unregister devices from bus
            bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
            bus.UnSubscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
            bus.UnSubscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
            bus.UnSubscribe<ExceptionEventArg>(OnError);

            // discard local references
            emulator.Dispose();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            
            LoadImageFile();
        }

        private void mnuOpenFile_Click(object sender, EventArgs e)
        {
            LoadImageFile();
        }

        private void mnuStartEmulator_Click(object sender, EventArgs e)
        {
            var formEmulator = new frmEmulator();
            formEmulator.MdiParent = this;
            formEmulator.Show();
        }
    }
}
