using System.Threading.Tasks;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02S.Engine.Devices;

namespace W65C02S.Plugin.VIA
{
    public class W6522 : AbstractMemoryMappedDevice
    {
        internal const string name = "VIA6522";
        internal const ushort start = 0x00;
        internal const ushort end = 0x0F;

        private const byte PORTB = 0x00;
        private const byte PORTA = 0x01;
        private const byte DDRB = 0x02;
        private const byte DDRA = 0x03;
        private const byte T1CL = 0x04;
        private const byte T1CH = 0x05;
        private const byte T1LL = 0x06;
        private const byte T1LH = 0x07;
        private const byte T2CL = 0x08;
        private const byte T2CH = 0x09;
        private const byte SR = 0x0A;
        private const byte ACR = 0x0B;
        private const byte PCR = 0x0C;
        private const byte IFR = 0x0D;
        private const byte IER = 0x0E;
        private const byte PORTA_ = 0x0F;


        public W6522(IBus bus) : base(bus)
        {
            DeviceName = name;
            MappedIO = IOMapping.IO0;
            Mode = DataBusMode.ReadWrite;
            //memory = new byte[end - start];
            memory = new byte[16];
            Enabled = true;
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
                for (byte index = 0; index < 16; index++)
                {
                    if ((arg.Address & index) == index)
                    {
                        register = index;
                        break;
                    }
                }

                //Read
                if (arg.Mode == DataBusMode.Read && (Mode == DataBusMode.Read || Mode == DataBusMode.ReadWrite))
                {
                    //if((arg.Address - StartAddress) >= (end - start))
                    //{
                    //    // return default data (non mapped)
                    //    arg.Data = 0x00;
                    //}
                    //else
                    //{
                    //    // return data
                    //    arg.Data = memory[arg.Address - StartAddress];
                    //}

                    arg.Data = memory[register];


                }
                //Write
                if (arg.Mode == DataBusMode.Write && (Mode == DataBusMode.Write || Mode == DataBusMode.ReadWrite))
                {
                    // set data
                    memory[register] = arg.Data;

                }
                arg.DeviceName = $"{DeviceName} ({GetRegisterName(register)})";
            }
        }

        private string GetRegisterName(byte register)
        {
            switch (register)
            {
                case 0x00:
                    return "PORTB";
                case 0x01:
                    return "PORTA";
                case 0x02:
                    return "DDRB";
                case 0x03:
                    return "DDRA";
                case 0x04:
                    return "T1CL";
                case 0x05:
                    return "T1CH";
                case 0x06:
                    return "T1LL";
                case 0x07:
                    return "T1LH";
                case 0x08:
                    return "T2CL";
                case 0x09:
                    return "T2CH";
                case 0x0A:
                    return "SR";
                case 0x0B:
                    return "ACR";
                case 0x0C:
                    return "PCR";
                case 0x0D:
                    return "IFR";
                case 0x0E:
                    return "IER";
                case 0x0F:
                    return "PORTA*";
                default:
                    return "Unknown register";
            }
        }

    }
}
