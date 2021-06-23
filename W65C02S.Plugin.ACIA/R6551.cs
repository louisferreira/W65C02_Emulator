using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02S.Engine.Devices;

namespace W65C02S.Plugin.ACIA
{
    public class R6551 : AbstractMemoryMappedDevice
    {
        /*
        Status Register
          7   6   5   4    3    2   1   0
        ----------------------------------
        |IRQ|DSR|DCD|TDRE|RDRF|OVRN|FE|PE|
        ----------------------------------


        */

        internal const string name = "ACIA6551";
        internal const ushort start = 0x00;
        internal const ushort end = 0x03;

        private const byte DATA = 0x00;
        private const byte STATUS = 0x01;
        private const byte COMMAND = 0x02;
        private const byte CONTROL = 0x03;

        private bool _DSR_ = true; // active low
        private bool _DCD_ = true; // active low

        public R6551(IBus bus) : base(bus)
        {
            DeviceName = name;
            MappedIO = IOMapping.IO1;
            Mode = DataBusMode.ReadWrite;
            //memory = new byte[end - start];
            memory = new byte[4];
            Enabled = true;
            bus.Subscribe<ResetEventArgs>(OnReset);
        }

        private void OnReset(ResetEventArgs arg)
        {
            if(arg.ResetType == ResetType.Hardware)
            {
                memory[COMMAND] = 0x0;
                memory[CONTROL] = 0x0;
                memory[DATA] = 0x0;
                memory[STATUS] = 0x70; //0111 0000

            }
            else
            {
                memory[COMMAND] = 0x0;
                memory[CONTROL] = 0x0;
                memory[DATA] = 0x0;
                GenerateRandomReceiveData();
            }

        }

        private void GenerateRandomReceiveData()
        {
            // simulate data recived ramdomly into the Rx buffer and raise IRQ
            Task.Run(() =>
            {
                var testdata = "Hello world from ACIA!";
                byte[] asciiBytes = Encoding.ASCII.GetBytes(testdata);
                for (int index = 0; index < testdata.Length; index++)
                {
                    // send a byte                    
                    memory[DATA] = asciiBytes[index];
                    memory[STATUS] = (byte)(memory[STATUS] ^ 0x88); // set IRQ bit and RDRF bit
                    RaiseInteruptRequest();

                    // wait while IRQ and RDRF set
                    while ((memory[STATUS] & 0x80) == 0x80 || (memory[STATUS] & 0x08) == 0x08)
                    {
                        Thread.Sleep(1000);
                    }

                }
                


            });
        }

        public override void SetIOAddress(ushort startAddress, ushort endAddress)
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        protected override void OnAddressChanged(AddressBusEventArgs arg)
        {
            if (Enabled && arg.AddressDecoder.IsMappedTo(MappedIO))
            {
                // get selected register
                byte register = 0x0;
                byte localaddress = (byte)arg.Address; // gets the lower byte (LSB)

                if (localaddress == 0x00)
                     register = DATA;
                if (localaddress == 0x01)
                    register = STATUS;
                if (localaddress == 0x02)
                    register = COMMAND;
                if (localaddress == 0x03)
                    register = CONTROL;


                //Read
                if (arg.Mode == DataBusMode.Read && (Mode == DataBusMode.Read || Mode == DataBusMode.ReadWrite))
                {
                    // read the register value
                    arg.Data = memory[register];

                    if (register == STATUS)
                    {
                        // clear the IRQ bit in status reg
                        memory[STATUS] = (byte)(memory[STATUS] ^ 0x80);
                    }
                    if (register == DATA)
                    {
                        // clear the RDRF bit in status reg
                        memory[STATUS] = (byte)(memory[STATUS] ^ 0x08);
                    }

                }
                //Write
                if (arg.Mode == DataBusMode.Write && (Mode == DataBusMode.Write || Mode == DataBusMode.ReadWrite))
                {
                    // only data, command and control registers can be written to
                    if(register == DATA || register == COMMAND || register == CONTROL)
                    {
                        // set data
                        memory[register] = arg.Data;
                    }
                    else if(register == STATUS)
                    {
                        // software reset
                        OnReset(new ResetEventArgs() { ResetType = ResetType.Software });
                    }

                }
                arg.DeviceName = $"{DeviceName} ({GetRegisterName(register)})";
            }
        }

        private void RaiseInteruptRequest(InteruptType type = InteruptType.IRQ)
        {
            var irq = new InteruptRequestEventArgs() { InteruptType = type };
            bus.Publish(irq);
        }

        private string GetRegisterName(byte register)
        {
            switch (register)
            {
                case 0x00:
                    return "DATA";
                case 0x01:
                    return "STATUS";
                case 0x02:
                    return "COMMAND";
                case 0x03:
                    return "CONTROL";

                default:
                    return "Unknown register";
            }
        }

        private void Dispose()
        {
            bus.UnSubscribe<ResetEventArgs>(OnReset);
            base.Dispose();
        }
    }
}
