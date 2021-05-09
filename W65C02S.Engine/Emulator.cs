using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W65C02S.CPU;
using W65C02S.Clock;
using W65C02S.Engine.Parsers;
using static W65C02S.CPU.Enums.Enums;
using W65C02S.Bus;

namespace W65C02S.Engine
{
    public class Emulator : IDisposable
    {
        private byte[] ram = new byte[1024 * 64];


        private readonly AddressBus addressBus = new AddressBus();
        private readonly DataBus dataBus = new DataBus();

        private CPUCore CPU;
        private byte lastDataRead;
        private ushort currentAddress;
        private Guid addressToken;
        private Guid dataToken;

        public event EventHandler<AddressBusEventArgs> OnAddressChanged;
        public event EventHandler<InstructionDisplayEventArg> OnInstructionExecuted;
        public event EventHandler<ExceptionEventArg> OnError;
        
        public byte[] Ram
        {
            get
            {
                return ram;
            }
            set
            {
                ram = value;
            }
        }

        public Emulator()
        {
            for (int index = 0; index < ram.Length; index++)
            {
                ram[index] = 0xEA;
            }

            // initialise stack
            for (ushort index = 0x0100; index <= 0x01FF; index++)
            {
                ram[index] = 0x00;
            }

            addressToken = addressBus.Subscribe(OnCPUAddressChanged);
            dataToken = dataBus.Subscribe(OnCPUDataRequest);
            CPU = new CPUCore(addressBus, dataBus);
;
            CPU.OnInstructionExecuted += CPU_OnInstructionExecuted;
            CPU.OnError += CPU_OnError;
        }

        

        private void CPU_OnInstructionExecuted(object sender, InstructionEventArg e)
        {
            if (OnInstructionExecuted != null)
            {
                var x = new InstructionDisplayEventArg
                {
                    CurrentInstruction = e.CurrentInstruction,
                    DecodedInstruction = InstructionParser.Parse(e.CurrentInstruction),
                    RawData = $"{e.CurrentInstruction.OpCode:X2} {e.CurrentInstruction.Operand1:X2} {e.CurrentInstruction.Operand2:X2}".TrimEnd(),
                    A_Reg = CPU.A,
                    X_Reg = CPU.X,
                    Y_Reg = CPU.Y,
                    PC = CPU.PC,
                    ST_Reg = (byte)CPU.ST,
                    SP = CPU.SP,
                    ClockTicks = CPU.ClockTicks
                };
                OnInstructionExecuted.Invoke(this, x);
            }
        }

        public void Reset()
        {
            CPU.Reset();
        }
        public void Step()
        {
            CPU.Step();
        }

        public void Run()
        {
            while ((CPU.ST & ProcessorFlags.B) != ProcessorFlags.B )
            {
                //if (BreakPoints.Contains(PC))
                //{
                //    if (OnBreakPoint != null)
                //    {
                //        var e = new OutputEventArg
                //        {
                //            Address = PC
                //        };
                //        OnBreakPoint.Invoke(this, e);
                //    }
                //    return;
                //}
                //else
                    CPU.Step();
            }
        }

        public bool IsFlagSet(ProcessorFlags flag)
        {
            return CPU.IsFlagSet(flag);
        }

        //private void CPU_OnDataBusAccess(object sender, DataBusEventArgs e)
        //{
        //    if(e.Mode == RW.Read)
        //    {
        //        // read the currentAddress location and set the e.Data property to the value
        //        e.Data = ram[currentAddress];
        //    }
        //    else
        //    {
        //        // save e.Data to the the currentAddress location
        //        ram[currentAddress] = e.Data;
        //    }
        //}

        private void OnCPUAddressChanged(ushort address)
        {
            currentAddress = address;

            OnAddressChanged?.Invoke(this, new AddressBusEventArgs { Address = address });

            var data = ram[address];
            dataBus.Publish(data);
        }
        private void OnCPUDataRequest(byte data)
        {
            ram[currentAddress] = data;
        }

        //private void CPU_OnAddressChanged(object sender, AddressBusEventArgs e)
        //{
        //    currentAddress = e.Address;

        //    // notify each component connected about new address

        //    // raise the event
        //    if (OnAddressChanged != null)
        //    {
        //        OnAddressChanged.Invoke(this, e);
        //    }
        //}

        private void CPU_OnError(object sender, ExceptionEventArg e)
        {
            if (OnError != null)
            {
                OnError.Invoke(sender, e);
            }
        }

        public void Dispose()
        {
            //CPU.OnAddressChanged -= CPU_OnAddressChanged;
            //CPU.OnDataBussAccess -= CPU_OnDataBusAccess;
            CPU.OnInstructionExecuted -= CPU_OnInstructionExecuted;
            CPU.OnError -= CPU_OnError;

            addressBus.UnSubscribe(addressToken);
        }

        public void LoadROM(byte[] data)
        {
            var x = 0;
            for (int index = 0x8000; index < ram.Length; index++)
            {
                ram[index] = data[x];
                x++;
            }
        }
    }
}
