using System;
using System.Linq;
using W65C02S.CPU.Models;

namespace W65C02S.CPU.Tests
{
    public abstract class CPUCoreAbstractTests : IDisposable
    {
        protected CPUCore target;
        protected Bus.Bus bus;
        public CPUCoreAbstractTests()
        {
            bus = new Bus.Bus();
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
