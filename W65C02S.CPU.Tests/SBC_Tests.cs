using System.Collections.Generic;
using Xunit;

namespace W65C02S.CPU.Tests
{
    public class SBC_Tests : CPUCoreAbstractTests
    {
        private const byte SBC_Abs      = 0xED;
        private const byte SBC_AbsIdxX  = 0xFD;
        private const byte SBC_AbsIdxY  = 0xF9;
        private const byte SBC_Imm      = 0xE9;
        private const byte SBC_ZP       = 0xE5;
        private const byte SBC_ZPIdxInd = 0xE1;
        private const byte SBC_IndX     = 0xF5;
        private const byte SBC_ZP_Ind   = 0xF2;
        private const byte SBC_IndIdxY  = 0xF1;

        private Dictionary<ushort, byte> testMemory;

        protected override void OnAddressBusEventArgs(AddressBusEventArgs args)
        {
            args.Data = testMemory[args.Address];
        }

        [Fact]
        public void ShouldSubtractTwoPositiveNumbers_AccAndImm()
        {
            var instr = GetInstruction(SBC_Imm);
            instr.Operand1 = 60;

            target.A = 100;
            target.ST = Bus.ProcessorFlags.C;

            target.Execute(instr);

            Assert.True(target.A == 40, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & Bus.ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.C) >  0, "Expected C flag to be set, but was reset.");
            Assert.True((target.ST & Bus.ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldSubtractOneFromZeroAndGiveMinusOne_AccAndImm()
        {
            var instr = GetInstruction(SBC_Imm);
            instr.Operand1 = 1;

            target.A = 0;
            target.ST = Bus.ProcessorFlags.C;

            target.Execute(instr);

            Assert.True(target.A == 0xFF, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & Bus.ProcessorFlags.N) >  0, "Expected N flag to be set, but was reset.");
            Assert.True((target.ST & Bus.ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.C) == 0, "Expected C flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldSubtractTwoNegativeNumbersAndGetSignedOverflow_AccAndImm()
        {
            var instr = GetInstruction(SBC_Imm);
            instr.Operand1 = 1;

            target.A = 0x80;
            target.ST = Bus.ProcessorFlags.C;

            target.Execute(instr);

            Assert.True(target.A == 127, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & Bus.ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.C) >  0, "Expected C flag to be set, but was reset.");
            Assert.True((target.ST & Bus.ProcessorFlags.V) >  0, "Expected V flag to be set, but was reset.");
            Assert.True((target.ST & Bus.ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }

        [Fact]
        public void ShouldSubtractPositiveAndNegativeNumbersAndGetSignedOverflow_AccAndImm()
        {
            var instr = GetInstruction(SBC_Imm);
            instr.Operand1 = 0xFF; // -1

            target.A = 0x7F; // 127
            target.ST = Bus.ProcessorFlags.C;

            target.Execute(instr);

            Assert.True(target.A == 128, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & Bus.ProcessorFlags.N) >  0, "Expected N flag to be set, but was reset.");
            Assert.True((target.ST & Bus.ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.C) == 0, "Expected C flag to be reset, but was set.");
            Assert.True((target.ST & Bus.ProcessorFlags.V) > 0, "Expected V flag to be set, but was reset.");
            Assert.True((target.ST & Bus.ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");
        }
    }
}