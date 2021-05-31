using System.Collections.Generic;
using W65C02.API.Enums;
using W65C02.API.EventArgs;
using Xunit;

namespace W65C02S.CPU.Tests
{
    public class LDA_Tests : CPUCoreAbstractTests
    {
        private const byte LDA_Abs          = 0xAD;
        private const byte LDA_AbsIdxX      = 0xBD;
        private const byte LDA_AbsIdxY      = 0xB9;
        private const byte LDA_Imm          = 0xA9;
        private const byte LDA_ZP           = 0xA5;
        private const byte LDA_ZPIdxInd     = 0xA1;
        private const byte LDA_IndX         = 0xB5;
        private const byte LDA_ZP_Ind       = 0xB2;
        private const byte LDA_IndIdxY      = 0xA9;

        private Dictionary<ushort, byte> testMemory;

        protected override void OnAddressBusEventArgs(AddressBusEventArgs args)
        {
            args.Data = testMemory[args.Address];
        }

        [Fact]
        public void ShouldLoadAccumWithPositiveImmediateValue()
        {
            var instr = GetInstruction(LDA_Imm);
            instr.Operand1 = 0x20;
            target.ST = (ProcessorFlags.N | ProcessorFlags.Z);

            target.Execute(instr);

            Assert.True(target.A == 0x20, "Accumulator value not loaded with expected value");
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
        public void ShouldLoadAccumWithNegativeImmediateValue()
        {
            var instr = GetInstruction(LDA_Imm);
            instr.Operand1 = 0xF9;
            target.ST = (ProcessorFlags.N | ProcessorFlags.Z);

            target.Execute(instr);

            Assert.True(target.A == 0xF9, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.N) > 0, "Expected N flag to be set, but was reset.");
            Assert.True((target.ST & ProcessorFlags.Z) == 0, "Expected Z flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) == 0, "Expected C flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");

        }

        [Fact]
        public void ShouldLoadAccumWithZeroImmediateValue()
        {
            var instr = GetInstruction(LDA_Imm);
            instr.Operand1 = 0;
            target.ST = (ProcessorFlags.N);

            target.Execute(instr);

            Assert.True(target.A == 0, "Accumulator value not loaded with expected value");
            Assert.True(target.X == 0, "X Register expected to be 0, but was not.");
            Assert.True(target.Y == 0, "Y Register expected to be 0, but was not.");

            Assert.True((target.ST & ProcessorFlags.Z) > 0, "Expected Z flag to be set, but was reset.");

            Assert.True((target.ST & ProcessorFlags.N) == 0, "Expected N flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.B) == 0, "Expected B flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.I) == 0, "Expected I flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.C) == 0, "Expected C flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.V) == 0, "Expected V flag to be reset, but was set.");
            Assert.True((target.ST & ProcessorFlags.D) == 0, "Expected D flag to be reset, but was set.");

        }

        [Fact]
        public void ShouldLoadAccumWithPositiveAbsoluteValue()
        {
            testMemory = new Dictionary<ushort, byte>() {{ 0x8000, 0x20}};

            var instr = GetInstruction(LDA_Abs);
            instr.Operand1 = 0x00;
            instr.Operand2 = 0x80;

            target.ST = (ProcessorFlags.N | ProcessorFlags.Z);

            target.Execute(instr);

            Assert.Equal(0x20, target.A);
            Assert.Equal(0x0, target.X);
            Assert.Equal(0x0, target.Y);

            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.N));
            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.Z));
            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.B));
            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.I));
            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.C));
            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.V));
            Assert.Equal(0, (byte)(target.ST & ProcessorFlags.D));

        }


    }
}
