using System;
using System.Linq;
using W65C02.API.EventArgs;
using W65C02.API.Interfaces;
using W65C02.API.Models;
using W65C02S.Engine.Devices;

namespace W65C02S.CPU.Tests
{
    public abstract class CPUCoreAbstractTests : IDisposable
    {
        protected CPUCore target;
        protected IBus bus;
        public CPUCoreAbstractTests()
        {
            bus = new Bus();
            bus.Subscribe<AddressBusEventArgs>(OnAddressBusEventArgs);
            target = new CPUCore(bus);
            
        }

        protected Instruction GetInstruction(byte opCode)
        {
            return target.InstructionTable.FirstOrDefault(x => x.OpCode == opCode);
        }

        protected abstract void OnAddressBusEventArgs(AddressBusEventArgs args);

        public void Dispose()
        {
            bus.UnSubscribe<AddressBusEventArgs>(OnAddressBusEventArgs);
            target = null;
        }
    }
}
