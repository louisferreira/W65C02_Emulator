using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02S.CPU
{
    public partial class CPUCore
    {
        // ADd memory to accumulator with Carry
        private void ADC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // "AND" memory with accumulator
        private void AND(byte instructionLength)
        {
            A = ((byte)(A & fetchedByte.Value));

            SetFlag(Enums.Enums.ProcessorFlags.Z, A == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // Arithmetic Shift one bit Left, memory or accumulator
        private void ASL(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch on Bit Reset
        private void BBR(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch on Bit Set
        private void BBS(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch on Carry Clear (Pc=0)
        private void BCC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch on Carry Set (Pc=1)
        private void BCS(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch if EQual (Pz=1)
        private void BEQ(byte instructionLength)
        {
            // Branch if Z=1 Branch if EQual (Pz=1)   
            if (IsFlagSet(Enums.Enums.ProcessorFlags.Z))
            {
                // Z = 1 (take branch)
                var amount = (sbyte)(fetchedByte.Value + 2);
                IncrementPC(amount);
            }
            else
            {
                // Z = 0 (next instruction)
                IncrementPC(instructionLength);
            }
        }

        // BIt Test
        private void BIT(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch if result MInus (Pn=1)
        private void BMI(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch if Not Equal (Pz=0)
        private void BNE(byte instructionLength)
        {

            //Branch if Z=0 Branch if Not Equal (Pz=0)
            if (!IsFlagSet(Enums.Enums.ProcessorFlags.Z))
            {
                // Z = 0 (take branch)
                var amount = (sbyte)(fetchedByte.Value + 2);
                IncrementPC(amount);
            }
            else
            {
                // Z = 0 (next instruction)
                IncrementPC(instructionLength);
            }

        }

        // Branch if result PLus (Pn=0)
        private void BPL(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // BRanch Always
        private void BRA(byte instructionLength)
        {
            // do nothing
        }

        // BReaK instruction
        private void BRK(byte instructionLength)
        {
            SetFlag(Enums.Enums.ProcessorFlags.B, true);
            SetFlag(Enums.Enums.ProcessorFlags.Z, false);
        }

        // Branch on oVerflow Clear (Pv=0)
        private void BVC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Branch on oVerflow Set (Pv=1)
        private void BVS(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // CLear Cary flag
        private void CLC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // CLear Decimal mode
        private void CLD(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // CLear Interrupt disable bit
        private void CLI(byte instructionLength)
        {
            SetFlag(Enums.Enums.ProcessorFlags.I, false);
            IncrementPC(instructionLength);
        }

        // CLear oVerflow flag
        private void CLV(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // CoMPare memory and accumulator
        private void CMP(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // ComPare memory and X register
        private void CPX(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // ComPare memory and Y register
        private void CPY(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // DECrement memory or accumulate by one
        private void DEC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // DEcrement X by one
        private void DEX(byte instructionLength)
        {
            X--;
            SetFlag(Enums.Enums.ProcessorFlags.Z, X == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (X & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // DEcrement Y by one
        private void DEY(byte instructionLength)
        {
            Y--;
            SetFlag(Enums.Enums.ProcessorFlags.Z, Y == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (Y & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // "Exclusive OR" memory with accumulate
        private void EOR(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // INCrement memory or accumulator by one
        private void INC(byte instructionLength)
        {
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                fetchedByte++;
                WriteValueToAddress(operandAddress.Value, fetchedByte.Value);
            }
            else
            {
                A++;
            }

            SetFlag(Enums.Enums.ProcessorFlags.Z, A == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // INcrement X register by one
        private void INX(byte instructionLength)
        {
            X++;
            SetFlag(Enums.Enums.ProcessorFlags.Z, X == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (X & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // INcrement Y register by one
        private void INY(byte instructionLength)
        {
            Y++;
            SetFlag(Enums.Enums.ProcessorFlags.Z, Y == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (Y & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // JuMP to new location
        private void JMP(byte instructionLength)
        {
            PC = operandAddress.Value;
        }

        // Jump to new location Saving Return (Jump to SubRoutine)
        private void JSR(byte instructionLength)
        {
            // save return address to stack, hi byte first then lo byte
            var retAddr = (PC + instructionLength);
            WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
            DecreaseSP();

            WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
            DecreaseSP();

            PC = operandAddress.Value;

        }

        // LoaD Accumulator with memory
        private void LDA(byte instructionLength)
        {
            if (operandAddress.HasValue)
                ReadValueFromAddress(operandAddress.Value);
            if (fetchedByte.HasValue)
            {
                A = fetchedByte.Value;
                SetFlag(Enums.Enums.ProcessorFlags.Z, A == 0x00);
                SetFlag(Enums.Enums.ProcessorFlags.N, (A & 0x80) == 0x80);
            }
            IncrementPC(instructionLength);
        }

        // LoaD the X register with memory
        private void LDX(byte instructionLength)
        {
            if (operandAddress.HasValue)
                ReadValueFromAddress(operandAddress.Value);
            if (fetchedByte.HasValue)
            {
                X = fetchedByte.Value;
                SetFlag(Enums.Enums.ProcessorFlags.Z, X == 0x00);
                SetFlag(Enums.Enums.ProcessorFlags.N, (X & 0x80) == 0x80);
            }
            IncrementPC(instructionLength);
        }

        // LoaD the Y register with memory
        private void LDY(byte instructionLength)
        {
            if (operandAddress.HasValue)
                ReadValueFromAddress(operandAddress.Value);
            if (fetchedByte.HasValue)
            {
                Y = fetchedByte.Value;
                SetFlag(Enums.Enums.ProcessorFlags.Z, Y == 0x00);
                SetFlag(Enums.Enums.ProcessorFlags.N, (Y & 0x80) == 0x80);
            }
            IncrementPC(instructionLength);
        }

        // Logical Shift one bit Right memory or accumulator
        private void LSR(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // No OPeration
        private void NOP(byte instructionLength)
        {
            IncrementPC(instructionLength);
        }

        // "OR" memory with Accumulator
        private void ORA(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // PusH Accumulator on stack
        private void PHA(byte instructionLength)
        {
            WriteValueToAddress(SP, A);
            DecreaseSP();
            IncrementPC(instructionLength);
        }

        // PusH Processor status on stack
        private void PHP(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // PusH X register on stack
        private void PHX(byte instructionLength)
        {
            WriteValueToAddress(SP, X);
            DecreaseSP();
            IncrementPC(instructionLength);
        }

        // PusH Y register on stack
        private void PHY(byte instructionLength)
        {
            WriteValueToAddress(SP, Y);
            DecreaseSP();
            IncrementPC(instructionLength);
        }

        // PuLl Accumulator from stack
        private void PLA(byte instructionLength)
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            A = fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(instructionLength);
        }

        // PuLl Processor status from stack
        private void PLP(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // PuLl X register from stack
        private void PLX(byte instructionLength)
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            X = fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(instructionLength);
        }

        // PuLl Y register from stack
        private void PLY(byte instructionLength)
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            Y = fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(instructionLength);
        }

        // Reset Memory Bit
        private void RMB(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // ROtate one bit Left memory or accumulator
        private void ROL(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // ROtate one bit Right memory or accumulator
        private void ROR(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // ReTurn from Interrupt
        private void RTI(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // ReTurn from Subroutine
        private void RTS(byte instructionLength)
        {
            // retrieve return address from stack, lo byte first then hi byte
            IncreaseSP();
            ReadValueFromAddress(SP);
            var lo = fetchedByte.Value;
            IncreaseSP();
            ReadValueFromAddress(SP);
            var hi = fetchedByte.Value;

            PC = (ushort)((hi << 8) + lo);

        }

        // SuBtract memory from accumulator with borrow (Carry bit)
        private void SBC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // SEt Carry
        private void SEC(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // SEt Decimal mode
        private void SED(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // SEt Interrupt disable status
        private void SEI(byte instructionLength)
        {
            SetFlag(Enums.Enums.ProcessorFlags.I, true);
            IncrementPC(instructionLength);
        }

        // Set Memory Bit
        private void SMB(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // STore Accumulator in memory
        private void STA(byte instructionLength)
        {
            WriteValueToAddress(operandAddress.Value, A);
            IncrementPC(instructionLength);
        }

        // STore the X register in memory
        private void STX(byte instructionLength)
        {
            WriteValueToAddress(operandAddress.Value, X);
            IncrementPC(instructionLength);
        }

        // STore the Y register in memory
        private void STY(byte instructionLength)
        {
            WriteValueToAddress(operandAddress.Value, Y);
            IncrementPC(instructionLength);
        }

        // SToP mode
        private void STP(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // STore Zero in memory
        private void STZ(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Transfer the Accumulator to the X register
        private void TAX(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Transfer the Accumulator to the Y register
        private void TAY(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Test and Reset memory Bit
        private void TRB(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Test and Set memory Bit
        private void TSB(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Transfer the Stack pointer to the X register
        private void TSX(byte instructionLength)
        {
            ReadValueFromAddress(SP);
            IncreaseSP();
            X = fetchedByte.Value;

            SetFlag(Enums.Enums.ProcessorFlags.Z, X == 0x00);
            SetFlag(Enums.Enums.ProcessorFlags.N, (X & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // Transfer the X register to the Accumulator
        private void TXA(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // Transfer the X register to the Stack pointer register
        private void TXS(byte instructionLength)
        {
            SP = ((ushort)(0x0100 | X));
            IncrementPC(instructionLength);
        }

        // Transfer Y register to the Accumulator
        private void TYA(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }

        // WAit for Interrupt
        private void WAI(byte instructionLength) { throw new NotImplementedException(); }  // IncrementPC(instructionLength); }
    }
}
