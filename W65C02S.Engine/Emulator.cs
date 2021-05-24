using System;
using System.Collections.Generic;
using W65C02S.Bus;
using W65C02S.Bus.EventArgs;
using W65C02S.CPU;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using W65C02S.MemoryMappedDevice;
using System.Collections.ObjectModel;

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
        private List<IBaseIODevice> connectedDevices;
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
            connectedDevices = new List<IBaseIODevice>();
            this.bus = bus;
            cpu = new CPUCore(this.bus);
            bus.Subscribe<ExceptionEventArg>(OnError);
        }

        public void AddDevice(IBaseIODevice device)
        {
            connectedDevices.Add(device);
        }
        public ReadOnlyCollection<IBaseIODevice> GetConnectedDevices()
        {
            return new ReadOnlyCollection<IBaseIODevice>(connectedDevices);
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

        public Task Reset()
        {
            return Task.Factory.StartNew(() => cpu.Reset());
        }
        public Task Step()
        {
            return Task.Factory.StartNew(() => cpu.Step());
        }

        public void SendIRQ()
        {
            bus.Publish(new InteruptRequestEventArgs { InteruptType = InteruptType.IRQ});
        }

        public void SendNMI()
        {
            bus.Publish(new InteruptRequestEventArgs { InteruptType = InteruptType.NMI });
        }

        public Task Run()
        {
            mode = RunMode.Run;

            return Task.Factory.StartNew(() => {
                while (mode == RunMode.Run)
                {
                    if (BreakPoints.Contains(cpu.PC))
                    {
                        mode = RunMode.Debug;
                        var e = new ExceptionEventArg() { ErrorMessage = $"Breakpoint ${cpu.PC:X4} hit....".PadRight(100, ' '), ExceptionType = ExceptionType.Debug };
                        bus?.Publish(e);
                        break;
                    }
                    else
                        cpu.Step();
                }
            });
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

        public void LoadROM(byte[] data, bool offset)
        {
            var arg = new FlashROMArgs
            {
                Data = data,
                UseOffset = offset
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
