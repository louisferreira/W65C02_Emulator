using System;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;


namespace W65C02S.Engine
{
    public enum RunMode
    {
        Debug = 0,
        Run = 1
    }

    public class Emulator : IDisposable
    {

        private RunMode mode = RunMode.Debug;
        private readonly Bus.Bus bus;
        private CPUCore cpu;

        public RunMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        public Emulator(Bus.Bus bus)
        {
            this.bus = bus;
            cpu = new CPUCore(this.bus);
        }

        public byte ReadMemoryLocation(ushort address)
        {
            
            var arg = new AddressBusEventArgs
            {
                Address = address,
                Mode = DataBusMode.Read
            };
            bus.Publish(arg);
            return arg.Data;
        }
        public void WriteMemoryLocation(ushort address, byte data)
        {
            var arg = new AddressBusEventArgs
            {
                Address = address,
                Mode = DataBusMode.Write,
                Data = data
            };
            bus.Publish(arg);
        }

        public void Reset()
        {
            cpu.Reset();
        }
        public void Step()
        {
            cpu.Step();
        }

        public void SendIRQ()
        {
            bus.Publish(new InteruptRequestEventArgs { InteruptType = InteruptType.IRQ});
        }

        public void SendNMI()
        {
            bus.Publish(new InteruptRequestEventArgs { InteruptType = InteruptType.NMI });
        }

        public void Run()
        {
            mode = RunMode.Run;

            while ((cpu.ST & Bus.ProcessorFlags.B) != Bus.ProcessorFlags.B && mode == RunMode.Run)
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
                cpu.Step();
            }
        }

        public void ClearBreakFlag()
        {
            cpu.ST = (cpu.ST ^ ProcessorFlags.B);
        }

        public bool IsFlagSet(Bus.ProcessorFlags flag)
        {
            return ((cpu.ST & flag) == flag);
        }

        public void Dispose()
        {
            
        }

        public void LoadROM(byte[] data)
        {
            var arg = new RomLoadArgs
            {
                Data = data
            };
            bus.Publish(arg);
        }

        
    }
}
