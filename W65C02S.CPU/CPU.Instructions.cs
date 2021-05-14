using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W65C02S.Bus;

namespace W65C02S.CPU
{
    public partial class CPUCore
    {

        // ADd memory to accumulator with Carry
        private void ADC(byte instructionLength) 
        {
            if (IsFlagSet(ProcessorFlags.D))
            {
                ADC_DecimalMode();
            }
            else
            {
                ADC_BinaryMode();
            }

            IncrementPC(instructionLength);
        }
        private void ADC_BinaryMode()
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else if (fetchedByte.HasValue)
            {
                operand = fetchedByte.Value;
            }

            var carryFlag = IsFlagSet(ProcessorFlags.C) ? 1 : 0;

            ushort sum = (ushort)(A + operand + carryFlag);
            byte result = (byte)((sum & 0x00FF));

            var c = (sum >> 8) == 0x01;
            var v = ((A ^ result) & (operand ^ result) & 0x80) == 1;
            var n = (result & 0x80) == 0x80;
            var z = result == 0;

            SetFlag(ProcessorFlags.C, c);
            SetFlag(ProcessorFlags.Z, z);
            SetFlag(ProcessorFlags.N, z);
            SetFlag(ProcessorFlags.V, v);

            A = result;

        }
        private void ADC_DecimalMode()
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else if (fetchedByte.HasValue)
            {
                operand = fetchedByte.Value;
            }

            var carryFlag = IsFlagSet(ProcessorFlags.C) ? 1 : 0;

            ushort sum = (ushort)(A + operand + carryFlag);
            byte result = (byte)((sum & 0x00FF));

            var c = (sum >> 8) == 0x01;
            var v = ((A ^ result) & (operand ^ result) & 0x80) == 1;
            var n = (result & 0x80) == 0x80;
            var z = result == 0;

            SetFlag(ProcessorFlags.C, c);
            SetFlag(ProcessorFlags.Z, z);
            SetFlag(ProcessorFlags.N, z);
            SetFlag(ProcessorFlags.V, v);

            A = result;

        }


        // "AND" memory with accumulator
        private void AND(byte instructionLength)
        {
            A = ((byte)(A & fetchedByte.Value));

            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // Arithmetic Shift one bit Left, memory or accumulator
        private void ASL(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else
            {
                operand = A;
            }

            var c = (((operand >> 7) & 0x01) == 1);
            SetFlag(ProcessorFlags.C, c);
            A = (byte)(operand << 1);

            IncrementPC(instructionLength); 
        }

        // Branch on Bit Reset
        private void BBR(byte instructionLength) 
        {
            byte mask = 0x00;
            switch (currentInstruction.OpCode)
            {
                case 0x0F:
                    mask = 1 << 0;
                    break;
                case 0x1F:
                    mask = 1 << 1;
                    break;
                case 0x2F:
                    mask = 1 << 2;
                    break;
                case 0x3F:
                    mask = 1 << 3;
                    break;
                case 0x4F:
                    mask = 1 << 4;
                    break;
                case 0x5F:
                    mask = 1 << 5;
                    break;
                case 0x6F:
                    mask = 1 << 6;
                    break;
                case 0x7F:
                    mask = 1 << 7;
                    break;
                default:
                    break;
            }

            if( (fetchedByte.Value & mask) == 0)
            {
                var amount = (sbyte)(currentInstruction.Operand2);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(instructionLength); 
        }

        // Branch on Bit Set
        private void BBS(byte instructionLength)
        {
            byte mask = 0x00;
            switch (currentInstruction.OpCode)
            {
                case 0x0F:
                    mask = 1 << 0;
                    break;
                case 0x1F:
                    mask = 1 << 1;
                    break;
                case 0x2F:
                    mask = 1 << 2;
                    break;
                case 0x3F:
                    mask = 1 << 3;
                    break;
                case 0x4F:
                    mask = 1 << 4;
                    break;
                case 0x5F:
                    mask = 1 << 5;
                    break;
                case 0x6F:
                    mask = 1 << 6;
                    break;
                case 0x7F:
                    mask = 1 << 7;
                    break;
                default:
                    break;
            }

            if ((fetchedByte.Value & mask) == mask)
            {
                var amount = (sbyte)(currentInstruction.Operand2);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(instructionLength);
        }

        // Branch on Carry Clear (Pc=0)
        private void BCC(byte instructionLength) 
        {
            if ( !IsFlagSet(ProcessorFlags.C))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(instructionLength);
        }

        // Branch on Carry Set (Pc=1)
        private void BCS(byte instructionLength) 
        {
            if (IsFlagSet(ProcessorFlags.C))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(instructionLength);
        }

        // Branch if EQual (Pz=1)
        private void BEQ(byte instructionLength)
        {
            // Branch if Z=1 Branch if EQual (Pz=1)   
            if (IsFlagSet(ProcessorFlags.Z))
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
        private void BIT(byte instructionLength) 
        {
            byte M = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, ((A ^ M) == M));
            IncrementPC(instructionLength); 
        }

        // Branch if result MInus (Pn=1)
        private void BMI(byte instructionLength) 
        {
            //Branch if Z=0 Branch if Not Equal (Pz=1)
            if (IsFlagSet(ProcessorFlags.Z))
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

        // Branch if Not Equal (Pz=0)
        private void BNE(byte instructionLength)
        {

            //Branch if Z=0 Branch if Not Equal (Pz=0)
            if (!IsFlagSet(ProcessorFlags.Z))
            {
                // Z = 0 (take branch)
                var amount = (sbyte)(fetchedByte.Value + 2);
                IncrementPC(amount);
            }
            else
            {
                IncrementPC(instructionLength);
            }

        }

        // Branch if result PLus (Pn=0)
        private void BPL(byte instructionLength) 
        {
            if (IsFlagSet(ProcessorFlags.N))
            {
                // N = 0 (take branch)
                var amount = (sbyte)(fetchedByte.Value + 2);
                IncrementPC(amount);
            }
            else
            {
                IncrementPC(instructionLength);
            }
        }

        // BRanch Always
        private void BRA(byte instructionLength)
        {
            var amount = (sbyte)(fetchedByte.Value + 2);
            IncrementPC(amount);
        }

        // BReaK instruction
        private void BRK(byte instructionLength)
        {
            //// save return address (for when IRQ resumes from BRK)
            //var retAddr = (PC + instructionLength);
            //WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
            //DecreaseSP();

            //WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
            //DecreaseSP();


            //// set ST flags
            SetFlag(ProcessorFlags.B, true);
            //SetFlag(ProcessorFlags.D, false);

            ////push ST to stack
            //WriteValueToAddress(SP, A);
            //DecreaseSP();

            //// now set Interupt flag
            //SetFlag(ProcessorFlags.I, true);
            //IncrementPC(instructionLength);
        }

        // Branch on oVerflow Clear (Pv=0)
        private void BVC(byte instructionLength) 
        {
            if ( !IsFlagSet(ProcessorFlags.V))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(instructionLength);
        }

        // Branch on oVerflow Set (Pv=1)
        private void BVS(byte instructionLength) 
        {
            if (IsFlagSet(ProcessorFlags.V))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(instructionLength);
        }

        // CLear Cary flag
        private void CLC(byte instructionLength)
        {
            SetFlag(ProcessorFlags.C, false);
            IncrementPC(instructionLength);
        }

        // CLear Decimal mode
        private void CLD(byte instructionLength) {
            SetFlag(ProcessorFlags.D, false);
            IncrementPC(instructionLength);
        } 

        // CLear Interrupt disable bit
        private void CLI(byte instructionLength)
        {
            SetFlag(ProcessorFlags.I, false);
            IncrementPC(instructionLength);
        }

        // CLear oVerflow flag
        private void CLV(byte instructionLength) 
        {
            SetFlag(ProcessorFlags.V, false);
            IncrementPC(instructionLength); 
        }

        // CoMPare memory and accumulator
        private void CMP(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else if (fetchedByte.HasValue)
            {
                operand = fetchedByte.Value;
            }

            var result = (A - operand);


            SetFlag(ProcessorFlags.Z, result == 0x00);
            SetFlag(ProcessorFlags.N, (result & 0x80) == 0x80);
            SetFlag(ProcessorFlags.C, (A >= operand));

            IncrementPC(instructionLength);
        }

        // ComPare memory and X register
        private void CPX(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else if (fetchedByte.HasValue)
            {
                operand = fetchedByte.Value;
            }

            var result = (X - operand);


            SetFlag(ProcessorFlags.Z, result == 0x00);
            SetFlag(ProcessorFlags.N, (result & 0x80) == 0x80);
            SetFlag(ProcessorFlags.C, (X >= operand));

            IncrementPC(instructionLength);
        }

        // ComPare memory and Y register
        private void CPY(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else if (fetchedByte.HasValue)
            {
                operand = fetchedByte.Value;
            }

            var result = (Y - operand);


            SetFlag(ProcessorFlags.Z, result == 0x00);
            SetFlag(ProcessorFlags.N, (result & 0x80) == 0x80);
            SetFlag(ProcessorFlags.C, (X >= operand));

            IncrementPC(instructionLength);
        }

        // DECrement memory or accumulate by one
        private void DEC(byte instructionLength) 
        {
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                fetchedByte--;
                WriteValueToAddress(operandAddress.Value, fetchedByte.Value);
            }
            else
            {
                A--;
            }

            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // DEcrement X by one
        private void DEX(byte instructionLength)
        {
            X--;
            SetFlag(ProcessorFlags.Z, X == 0x00);
            SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // DEcrement Y by one
        private void DEY(byte instructionLength)
        {
            Y--;
            SetFlag(ProcessorFlags.Z, Y == 0x00);
            SetFlag(ProcessorFlags.N, (Y & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // "Exclusive OR" memory with accumulate
        private void EOR(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else if (fetchedByte.HasValue)
            {
                operand = fetchedByte.Value;
            }

            A = ((byte)(A ^ operand));

            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

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

            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // INcrement X register by one
        private void INX(byte instructionLength)
        {
            X++;
            SetFlag(ProcessorFlags.Z, X == 0x00);
            SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // INcrement Y register by one
        private void INY(byte instructionLength)
        {
            Y++;
            SetFlag(ProcessorFlags.Z, Y == 0x00);
            SetFlag(ProcessorFlags.N, (Y & 0x80) == 0x80);
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
                SetFlag(ProcessorFlags.Z, A == 0x00);
                SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
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
                SetFlag(ProcessorFlags.Z, X == 0x00);
                SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);
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
                SetFlag(ProcessorFlags.Z, Y == 0x00);
                SetFlag(ProcessorFlags.N, (Y & 0x80) == 0x80);
            }
            IncrementPC(instructionLength);
        }

        // Logical Shift one bit Right memory or accumulator
        private void LSR(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else
            {
                operand = A;
            }

            var c = ((operand & 0x01)  == 0x01);
            SetFlag(ProcessorFlags.C, c);
            A = (byte)(operand >> 1);
            SetFlag(ProcessorFlags.Z, (A == 0));
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // No OPeration
        private void NOP(byte instructionLength)
        {
            IncrementPC(instructionLength);
        }

        // "OR" memory with Accumulator
        private void ORA(byte instructionLength) 
        {
            byte operand = 0x00;
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;
            }
            else
            {
                operand = fetchedByte.Value;
            }
            
            A = (byte)(A ^ operand);

            SetFlag(ProcessorFlags.Z, (A == 0));
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            
            IncrementPC(instructionLength);
        }

        // PusH Accumulator on stack
        private void PHA(byte instructionLength)
        {
            WriteValueToAddress(SP, A);
            DecreaseSP();
            IncrementPC(instructionLength);
        }

        // PusH Processor status on stack
        private void PHP(byte instructionLength) 
        {
            WriteValueToAddress(SP, (byte)ST);
            DecreaseSP();
            IncrementPC(instructionLength); 
        }

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
        private void PLP(byte instructionLength) 
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            ST = (ProcessorFlags)fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(instructionLength);
        }

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

        // Reset Memory Bit (M => 0)
        private void RMB(byte instructionLength) 
        {
            byte mask = 0x00;
            switch (currentInstruction.OpCode)
            {
                case 0x07:
                    mask = 1 << 0;
                    break;
                case 0x17:
                    mask = 1 << 1;
                    break;
                case 0x27:
                    mask = 1 << 2;
                    break;
                case 0x37:
                    mask = 1 << 3;
                    break;
                case 0x47:
                    mask = 1 << 4;
                    break;
                case 0x57:
                    mask = 1 << 5;
                    break;
                case 0x67:
                    mask = 1 << 6;
                    break;
                case 0x77:
                    mask = 1 << 7;
                    break;
                default:
                    break;
            }

            ReadValueFromAddress(operandAddress.Value);
            fetchedByte = ((byte)(fetchedByte.Value ^ mask));
            WriteValueToAddress(operandAddress.Value, fetchedByte.Value);

            IncrementPC(instructionLength);
        }

        // ROtate one bit Left memory or accumulator
        private void ROL(byte instructionLength) 
        {
            // Bit 0 is filled with the current value of the carry flag, whilst the old bit 7 becomes the new carry flag value
            ushort operand = 0x00;

            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;

                var c = (byte)((operand & 0x80) >> 7);          // get old bit 7
                operand = (ushort)(operand << 1);               // leftshift by 1
                operand = (ushort)(operand | ((byte)ST & 1));   // add on the Carry bit
                byte result = (byte)operand;
                WriteValueToAddress(operandAddress.Value, result);

                SetFlag(ProcessorFlags.C, (c == 1));
                SetFlag(ProcessorFlags.N, (operand & 0x80) == 0x80);
            }
            else
            {
                var c = (byte)((operand & 0x80) >> 7);          // get old bit 7
                operand = (ushort)(operand << 1);               // leftshift by 1
                operand = (ushort)(operand | ((byte)ST & 1));   // add on the Carry bit
                byte result = (byte)operand;
                A = result;

                SetFlag(ProcessorFlags.C, (c == 1));
                SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            }
            
            IncrementPC(instructionLength); 
        }

        // ROtate one bit Right memory or accumulator
        private void ROR(byte instructionLength) 
        {
            // Bit 7 is filled with the current value of the carry flag whilst the old bit 0 becomes the new carry flag value.
            ushort operand = 0x00;

            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
                operand = fetchedByte.Value;

                byte currC = (byte)((byte)ST & 0x01);
                operand = (ushort)(operand | (currC << 8));
                bool c = (operand & 1) == 1;
                operand = (ushort)(operand >> 1);
                byte result = (byte)operand;
                WriteValueToAddress(operandAddress.Value, result);

                SetFlag(ProcessorFlags.C, c);
                SetFlag(ProcessorFlags.N, (operand & 0x80) == 0x80);
            }
            else
            {
                byte currC = (byte)((byte)ST & 0x01);
                operand = (ushort)(operand | (currC << 8));
                bool c = (operand & 1) == 1;
                operand = (ushort)(operand >> 1);
                byte result = (byte)operand;
                A = result;

                SetFlag(ProcessorFlags.C, c);
                SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            }
            IncrementPC(instructionLength); 
        }

        // ReTurn from Interrupt
        private void RTI(byte instructionLength) 
        {
            ReadValueFromAddress(SP);
            ST = (ProcessorFlags)fetchedByte.Value;
            WriteValueToAddress(SP, 0x00);
            IncreaseSP();

            ReadValueFromAddress(SP);
            WriteValueToAddress(SP, 0x00);
            IncreaseSP();
            var lo = fetchedByte.Value;

            ReadValueFromAddress(SP);
            WriteValueToAddress(SP, 0x00);
            IncreaseSP();
            var hi = fetchedByte.Value;

            PC = ((ushort)((hi << 8) + lo));
            
        }

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
        private void SEC(byte instructionLength)
        {
            SetFlag(ProcessorFlags.C, true);
            IncrementPC(instructionLength);
        }

        // SEt Decimal mode
        private void SED(byte instructionLength)
        {
            SetFlag(ProcessorFlags.D, true);
            IncrementPC(instructionLength);
        }

        // SEt Interrupt disable status
        private void SEI(byte instructionLength)
        {
            SetFlag(ProcessorFlags.I, true);
            IncrementPC(instructionLength);
        }

        // Set Memory Bit (M => 1)
        private void SMB(byte instructionLength) 
        {
            byte mask = 0x00;
            switch (currentInstruction.OpCode)
            {
                case 0x87:
                    mask = 1 << 0;
                    break;
                case 0x97:
                    mask = 1 << 1;
                    break;
                case 0xA7:
                    mask = 1 << 2;
                    break;
                case 0xB7:
                    mask = 1 << 3;
                    break;
                case 0xC7:
                    mask = 1 << 4;
                    break;
                case 0xD7:
                    mask = 1 << 5;
                    break;
                case 0xE7:
                    mask = 1 << 6;
                    break;
                case 0xF7:
                    mask = 1 << 7;
                    break;
                default:
                    break;
            }

            ReadValueFromAddress(operandAddress.Value);
            var val = fetchedByte.Value;
            val = ((byte)(val ^ mask));
            WriteValueToAddress(operandAddress.Value, val);

            IncrementPC(instructionLength); 
        }

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
        private void STP(byte instructionLength) {
            //SetFlag(ProcessorFlags.B, true);
            stopCmdEffected = true;
        }

        // STore Zero in memory
        private void STZ(byte instructionLength) {

            WriteValueToAddress(operandAddress.Value, 0x00);
            IncrementPC(instructionLength); 
        }

        // Transfer the Accumulator to the X register
        private void TAX(byte instructionLength) 
        {
            X = A;
            SetFlag(ProcessorFlags.Z, X == 0);
            SetFlag(ProcessorFlags.N, ((X & 0x80) == 0x80));

            IncrementPC(instructionLength); 
        }

        // Transfer the Accumulator to the Y register
        private void TAY(byte instructionLength) 
        {
            Y = A;
            SetFlag(ProcessorFlags.Z, Y == 0);
            SetFlag(ProcessorFlags.N, ((Y & 0x80) == 0x80));
            IncrementPC(instructionLength); 
        }

        // Test and Reset memory Bit
        private void TRB(byte instructionLength) 
        {
            ReadValueFromAddress(operandAddress.Value);
            var val = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, ((val & ~A) == ~A));
            val = ((byte)(val & ~A));
            WriteValueToAddress(operandAddress.Value, val);

            IncrementPC(instructionLength);
        }

        // Test and Set memory Bit
        private void TSB(byte instructionLength) 
        {
            ReadValueFromAddress(operandAddress.Value);
            var val = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, ((val & A) == A));
            val = ((byte)(val & A));
            WriteValueToAddress(operandAddress.Value, val);

            IncrementPC(instructionLength); 
        }

        // Transfer the Stack pointer to the X register
        private void TSX(byte instructionLength)
        {
            X = ((byte)(SP & 0xFF));

            SetFlag(ProcessorFlags.Z, X == 0x00);
            SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // Transfer the X register to the Accumulator
        private void TXA(byte instructionLength) 
        {
            A = X;
            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            IncrementPC(instructionLength);
        }

        // Transfer the X register to the Stack pointer register
        private void TXS(byte instructionLength)
        {
            SP = ((ushort)(0x0100 | X));
            IncrementPC(instructionLength);
        }

        // Transfer Y register to the Accumulator
        private void TYA(byte instructionLength) 
        {
            A = Y;
            
            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(instructionLength);
        }

        // WAit for Interrupt
        private void WAI(byte instructionLength) {
            stopCmdEffected = true;
            SetFlag(ProcessorFlags.B, true);
            IncrementPC(instructionLength); 
        }
    }
}
