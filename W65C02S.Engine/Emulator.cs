using System;
using System.Collections.Generic;
using W65C02S.CPU;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.ObjectModel;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02S.MappingManager;


namespace W65C02S.Engine
{
    public class Emulator : IDisposable, IEmulator
    {
        private object locker = new object();
        private RunMode mode = RunMode.Debug;
        private readonly IBus bus;
        private AddressDecoder addressDecoder;
        private CPUCore cpu;
        private List<ushort> BreakPoints;
        private List<IMemoryMappedDevice> connectedDevices;
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



        public Emulator(IBus bus, List<W65C02.API.Models.MapConfig> mappings)
        {
            BreakPoints = new List<ushort>();
            connectedDevices = new List<IMemoryMappedDevice>();
            this.bus = bus;
            addressDecoder = new AddressDecoder(bus, mappings);

            cpu = new CPUCore(this.bus);
            bus.Subscribe<ExceptionEventArg>(OnError);

        }


        public void AddDevice(IMemoryMappedDevice device)
        {
            connectedDevices.Add(device);
        }
        public ReadOnlyCollection<IMemoryMappedDevice> GetConnectedDevices()
        {
            return new ReadOnlyCollection<IMemoryMappedDevice>(connectedDevices);
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
            return Task.Factory.StartNew(() =>
            {
                //cpu.Reset();
                var resetEvnt = new ResetEventArgs() { ResetType = ResetType.Hardware };
                bus.Publish(resetEvnt);
            });
        }
        public Task Step()
        {
            return Task.Factory.StartNew(() => cpu.Step());
        }

        public void SendIRQ()
        {
            bus.Publish(new InteruptRequestEventArgs { InteruptType = InteruptType.IRQ });
        }

        public void SendNMI()
        {
            bus.Publish(new InteruptRequestEventArgs { InteruptType = InteruptType.NMI });
        }

        public Task Run()
        {
            mode = RunMode.Run;

            return Task.Factory.StartNew(() =>
            {
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

        public void ClearInteruptFlag()
        {
            cpu.ST = (cpu.ST ^ ProcessorFlags.I);
        }

        public bool IsFlagSet(ProcessorFlags flag)
        {
            return ((cpu.ST & flag) == flag);
        }

        private void OnError(ExceptionEventArg obj)
        {
            mode = RunMode.Debug;
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

        public void Dispose()
        {
            bus.UnSubscribe<ExceptionEventArg>(OnError);
        }
    }
}
