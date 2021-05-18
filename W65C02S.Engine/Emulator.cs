using System;
using System.Collections.Generic;
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
        private List<ushort> BreakPoints;

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
            BreakPoints = new List<ushort>();
            this.bus = bus;
            cpu = new CPUCore(this.bus);
            bus.Subscribe<ExceptionEventArg>(OnError);
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

            while (mode == RunMode.Run)
            {
                if (BreakPoints.Contains(cpu.PC))
                {
                    mode = RunMode.Debug;
                    var e = new ExceptionEventArg() { ErrorMessage = $"Breakpoint ${cpu.PC:X4} hit....".PadRight(100, ' '), ExceptionType = ExceptionType.Warning };
                    bus?.Publish(e);
                    return;
                }
                else
                    cpu.Step();
            }
            if (cpu.IsFlagSet(ProcessorFlags.B) && !cpu.IsFlagSet(ProcessorFlags.I))
            {
                var e = new ExceptionEventArg() { ErrorMessage = $"Processor halted with BRK/WAI instruction...".PadRight(100, ' '), ExceptionType = ExceptionType.Warning };
                bus?.Publish(e);
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

        private void OnError(ExceptionEventArg obj)
        {
            mode = RunMode.Debug;
        }

        public void Dispose()
        {
            bus.UnSubscribe<ExceptionEventArg>(OnError);
        }

        public void LoadROM(byte[] data)
        {
            var arg = new RomLoadArgs
            {
                Data = data
            };
            bus.Publish(arg);
        }

        public void AddRemoveBreakPoint(ushort breakPoint)
        {
            if (BreakPoints.Contains(breakPoint))
                BreakPoints.Remove(breakPoint);
            else
                BreakPoints.Add(breakPoint);
        }
        public List<ushort> GetBreakPoints()
        {
            return BreakPoints;
        }

        public void SetPCValue(ushort inputValue)
        {
            cpu.PC = inputValue;
        }
    }
}
