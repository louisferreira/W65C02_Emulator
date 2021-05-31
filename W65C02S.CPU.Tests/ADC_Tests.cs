using System.Collections.Generic;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using Xunit;

namespace W65C02S.CPU.Tests
{
    public class ADC_Tests : CPUCoreAbstractTests
    {
        private const byte ADC_Abs      = 0x6D;
        private const byte ADC_AbsIdxX  = 0x7D;
        private const byte ADC_AbsIdxY  = 0x79;
        private const byte ADC_Imm      = 0x69;
        private const byte ADC_ZP       = 0x65;
        private const byte ADC_ZPIdxInd = 0x61;
        private const byte ADC_IndX     = 0x75;
        private const byte ADC_ZP_Ind   = 0x72;
        private const byte ADC_IndIdxY  = 0x71;

        private Dictionary<ushort, byte> testMemory;

        protected override void OnAddressBusEventArgs(AddressBusEventArgs args)
        {
            args.Data = testMemory[args.Address];
        }

        [Fact]
        public void ShouldAddTwoPositiveNumbers_AccAndImm()
        {
            var instr = GetInstruction(ADC_Imm);
            instr.Operand1 = 0x20;
            
            target.A = 0x20;
            target.ST = 0;

            target.Execute(instr);

            Assert.True(target.A == 0x40, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) == 0, "Expected C flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldAddTwoPositiveNumbersWithCarryIn_AccAndImm()
        {
            var instr = GetInstruction(ADC_Imm);
            instr.Operand1 = 0x20;

            target.A = 0x20;
            target.ST = ProcessorFlags.C;

            target.Execute(instr);

            Assert.True(target.A == 0x41, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) == 0, "Expected C flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldAddTwoPositiveNumbersWithCarryOut_AccAndImm()
        {
            var instr = GetInstruction(ADC_Imm);
            instr.Operand1 = 0x01; //1

            target.A = 0xFF; //255
            target.ST = 0;

            target.Execute(instr);

            Assert.True(target.A == 0x00, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.Z) >  0, "Expected Z flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) >  0, "Expected C flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldAddTwoNegativeNumbers_AccAndImm()
        {
            /*
             Cin   0    00000000
             A   -20    11101100
             #   -20    11101100
            --------------------
                 -40    11011000
            --------------------
            Cout       1
            */

            var instr = GetInstruction(ADC_Imm);
            instr.Operand1 = 0xEC; // -20

            target.A = 0xEC; // -20
            target.ST = 0;

            target.Execute(instr);

            Assert.True(target.A == 0xD8 /* -40 */, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) > 0, "Expected N flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) >  0, "Expected C flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldAddTwoNegativeNumbersWithCarryIn_AccAndImm()
        {
            /*
             Cin   1    00000001
             A   -20    11100000
             #   -20    11100000
            --------------------
                 -39    11011001
            --------------------
            Cout       1
            */

            var instr = GetInstruction(ADC_Imm);
            instr.Operand1 = 0xEC; // -20

            target.A = 0xEC; // -20
            target.ST = ProcessorFlags.C;

            target.Execute(instr);

            Assert.True(target.A == 0xD9, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) > 0, "Expected N flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) > 0, "Expected C flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldAddTwoPositiveNumbersAndSetOverFlow_AccAndImm()
        {
            var instr = GetInstruction(ADC_Imm);
            instr.Operand1 = 0xFF;

            target.A = 0x80;
            target.ST = 0;

            target.Execute(instr);

            Assert.True(target.A == 0x7F, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) >  0, "Expected C flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.V) >  0, "Expected V flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }
    }
}
