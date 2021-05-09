using System;
using W6502C.CPU;
using Xunit;

namespace W65C02.CPU.Tests
{
    public class CPU
    {
        private W65C02S target;
        private Bus bus;
        private RAM ram;

        public CPU()
        {
            bus = new Bus();
            ram = new RAM(64);
            bus.Connect(ram);

            target = new W65C02S(bus);
        }


        [Fact]
        public void LDA_Absolute()
        {
            ram.Write(0xfffc, 0x00);
            ram.Write(0xfffd, 0x00);
            
            ram.Write(0x0000, 0x0A);



        }
    }
}
