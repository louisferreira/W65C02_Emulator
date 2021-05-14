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
        private void ADC() 
        {
            if (IsFlagSet(ProcessorFlags.D))
            {
                ADC_DecimalMode();
            }
            else
            {
                ADC_BinaryMode();
            }

            IncrementPC(currentInstruction.Length);
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
        private void AND()
        {
            A = ((byte)(A & fetchedByte.Value));

            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(currentInstruction.Length);
        }

        // Arithmetic Shift one bit Left, memory or accumulator
        private void ASL() 
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

            IncrementPC(currentInstruction.Length); 
        }

        // Branch on Bit Reset
        private void BBR() 
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
                IncrementPC(currentInstruction.Length); 
        }

        // Branch on Bit Set
        private void BBS()
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
                IncrementPC(currentInstruction.Length);
        }

        // Branch on Carry Clear (Pc=0)
        private void BCC() 
        {
            if ( !IsFlagSet(ProcessorFlags.C))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(currentInstruction.Length);
        }

        // Branch on Carry Set (Pc=1)
        private void BCS() 
        {
            if (IsFlagSet(ProcessorFlags.C))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(currentInstruction.Length);
        }

        // Branch if EQual (Pz=1)
        private void BEQ()
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
                IncrementPC(currentInstruction.Length);
            }
        }

        // BIt Test
        private void BIT() 
        {
            byte M = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, ((A ^ M) == M));
            IncrementPC(currentInstruction.Length); 
        }

        // Branch if result MInus (Pn=1)
        private void BMI() 
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
                IncrementPC(currentInstruction.Length);
            }
        }

        // Branch if Not Equal (Pz=0)
        private void BNE()
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
                IncrementPC(currentInstruction.Length);
            }

        }

        // Branch if result PLus (Pn=0)
        private void BPL() 
        {
            if (IsFlagSet(ProcessorFlags.N))
            {
                // N = 0 (take branch)
                var amount = (sbyte)(fetchedByte.Value + 2);
                IncrementPC(amount);
            }
            else
            {
                IncrementPC(currentInstruction.Length);
            }
        }

        // BRanch Always
        private void BRA()
        {
            var amount = (sbyte)(fetchedByte.Value + 2);
            IncrementPC(amount);
        }

        // BReaK instruction
        private void BRK()
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
            //IncrementPC(currentInstruction.Length);
        }

        // Branch on oVerflow Clear (Pv=0)
        private void BVC() 
        {
            if ( !IsFlagSet(ProcessorFlags.V))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(currentInstruction.Length);
        }

        // Branch on oVerflow Set (Pv=1)
        private void BVS() 
        {
            if (IsFlagSet(ProcessorFlags.V))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
                IncrementPC(currentInstruction.Length);
        }

        // CLear Cary flag
        private void CLC()
        {
            SetFlag(ProcessorFlags.C, false);
            IncrementPC(currentInstruction.Length);
        }

        // CLear Decimal mode
        private void CLD() {
            SetFlag(ProcessorFlags.D, false);
            IncrementPC(currentInstruction.Length);
        } 

        // CLear Interrupt disable bit
        private void CLI()
        {
            SetFlag(ProcessorFlags.I, false);
            IncrementPC(currentInstruction.Length);
        }

        // CLear oVerflow flag
        private void CLV() 
        {
            SetFlag(ProcessorFlags.V, false);
            IncrementPC(currentInstruction.Length); 
        }

        // CoMPare memory and accumulator
        private void CMP() 
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

            IncrementPC(currentInstruction.Length);
        }

        // ComPare memory and X register
        private void CPX() 
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

            IncrementPC(currentInstruction.Length);
        }

        // ComPare memory and Y register
        private void CPY() 
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

            IncrementPC(currentInstruction.Length);
        }

        // DECrement memory or accumulate by one
        private void DEC() 
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

            IncrementPC(currentInstruction.Length);
        }

        // DEcrement X by one
        private void DEX()
        {
            X--;
            SetFlag(ProcessorFlags.Z, X == 0x00);
            SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);
            IncrementPC(currentInstruction.Length);
        }

        // DEcrement Y by one
        private void DEY()
        {
            Y--;
            SetFlag(ProcessorFlags.Z, Y == 0x00);
            SetFlag(ProcessorFlags.N, (Y & 0x80) == 0x80);
            IncrementPC(currentInstruction.Length);
        }

        // "Exclusive OR" memory with accumulate
        private void EOR() 
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
            IncrementPC(currentInstruction.Length);
        }

        // INCrement memory or accumulator by one
        private void INC()
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

            IncrementPC(currentInstruction.Length);
        }

        // INcrement X register by one
        private void INX()
        {
            X++;
            SetFlag(ProcessorFlags.Z, X == 0x00);
            SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);
            IncrementPC(currentInstruction.Length);
        }

        // INcrement Y register by one
        private void INY()
        {
            Y++;
            SetFlag(ProcessorFlags.Z, Y == 0x00);
            SetFlag(ProcessorFlags.N, (Y & 0x80) == 0x80);
            IncrementPC(currentInstruction.Length);
        }

        // JuMP to new location
        private void JMP()
        {
            PC = operandAddress.Value;
        }

        // Jump to new location Saving Return (Jump to SubRoutine)
        private void JSR()
        {
            // save return address to stack, hi byte first then lo byte
            var retAddr = (PC + currentInstruction.Length);
            WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
            DecreaseSP();

            WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
            DecreaseSP();

            PC = operandAddress.Value;

        }

        // LoaD Accumulator with memory
        private void LDA()
        {
            if (operandAddress.HasValue)
                ReadValueFromAddress(operandAddress.Value);
            if (fetchedByte.HasValue)
            {
                A = fetchedByte.Value;
                SetFlag(ProcessorFlags.Z, A == 0x00);
                SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            }
            IncrementPC(currentInstruction.Length);
        }

        // LoaD the X register with memory
        private void LDX()
        {
            if (operandAddress.HasValue)
                ReadValueFromAddress(operandAddress.Value);
            if (fetchedByte.HasValue)
            {
                X = fetchedByte.Value;
                SetFlag(ProcessorFlags.Z, X == 0x00);
                SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);
            }
            IncrementPC(currentInstruction.Length);
        }

        // LoaD the Y register with memory
        private void LDY()
        {
            if (operandAddress.HasValue)
                ReadValueFromAddress(operandAddress.Value);
            if (fetchedByte.HasValue)
            {
                Y = fetchedByte.Value;
                SetFlag(ProcessorFlags.Z, Y == 0x00);
                SetFlag(ProcessorFlags.N, (Y & 0x80) == 0x80);
            }
            IncrementPC(currentInstruction.Length);
        }

        // Logical Shift one bit Right memory or accumulator
        private void LSR() 
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

            IncrementPC(currentInstruction.Length);
        }

        // No OPeration
        private void NOP()
        {
            IncrementPC(currentInstruction.Length);
        }

        // "OR" memory with Accumulator
        private void ORA() 
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
            
            IncrementPC(currentInstruction.Length);
        }

        // PusH Accumulator on stack
        private void PHA()
        {
            WriteValueToAddress(SP, A);
            DecreaseSP();
            IncrementPC(currentInstruction.Length);
        }

        // PusH Processor status on stack
        private void PHP() 
        {
            WriteValueToAddress(SP, (byte)ST);
            DecreaseSP();
            IncrementPC(currentInstruction.Length); 
        }

        // PusH X register on stack
        private void PHX()
        {
            WriteValueToAddress(SP, X);
            DecreaseSP();
            IncrementPC(currentInstruction.Length);
        }

        // PusH Y register on stack
        private void PHY()
        {
            WriteValueToAddress(SP, Y);
            DecreaseSP();
            IncrementPC(currentInstruction.Length);
        }

        // PuLl Accumulator from stack
        private void PLA()
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            A = fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(currentInstruction.Length);
        }

        // PuLl Processor status from stack
        private void PLP() 
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            ST = (ProcessorFlags)fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(currentInstruction.Length);
        }

        // PuLl X register from stack
        private void PLX()
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            X = fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(currentInstruction.Length);
        }

        // PuLl Y register from stack
        private void PLY()
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            Y = fetchedByte.Value;
            WriteValueToAddress(SP, 0x00); //clear that cell
            IncrementPC(currentInstruction.Length);
        }

        // Reset Memory Bit (M => 0)
        private void RMB() 
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

            IncrementPC(currentInstruction.Length);
        }

        // ROtate one bit Left memory or accumulator
        private void ROL() 
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
            
            IncrementPC(currentInstruction.Length); 
        }

        // ROtate one bit Right memory or accumulator
        private void ROR() 
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
            IncrementPC(currentInstruction.Length); 
        }

        // ReTurn from Interrupt
        private void RTI() 
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
        private void RTS()
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
        private void SBC() { throw new NotImplementedException(); }  // IncrementPC(currentInstruction.Length); }

        // SEt Carry
        private void SEC()
        {
            SetFlag(ProcessorFlags.C, true);
            IncrementPC(currentInstruction.Length);
        }

        // SEt Decimal mode
        private void SED()
        {
            SetFlag(ProcessorFlags.D, true);
            IncrementPC(currentInstruction.Length);
        }

        // SEt Interrupt disable status
        private void SEI()
        {
            SetFlag(ProcessorFlags.I, true);
            IncrementPC(currentInstruction.Length);
        }

        // Set Memory Bit (M => 1)
        private void SMB() 
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

            IncrementPC(currentInstruction.Length); 
        }

        // STore Accumulator in memory
        private void STA()
        {
            WriteValueToAddress(operandAddress.Value, A);
            IncrementPC(currentInstruction.Length);
        }

        // STore the X register in memory
        private void STX()
        {
            WriteValueToAddress(operandAddress.Value, X);
            IncrementPC(currentInstruction.Length);
        }

        // STore the Y register in memory
        private void STY()
        {
            WriteValueToAddress(operandAddress.Value, Y);
            IncrementPC(currentInstruction.Length);
        }

        // SToP mode
        private void STP() {
            //SetFlag(ProcessorFlags.B, true);
            stopCmdEffected = true;
        }

        // STore Zero in memory
        private void STZ() {

            WriteValueToAddress(operandAddress.Value, 0x00);
            IncrementPC(currentInstruction.Length); 
        }

        // Transfer the Accumulator to the X register
        private void TAX() 
        {
            X = A;
            SetFlag(ProcessorFlags.Z, X == 0);
            SetFlag(ProcessorFlags.N, ((X & 0x80) == 0x80));

            IncrementPC(currentInstruction.Length); 
        }

        // Transfer the Accumulator to the Y register
        private void TAY() 
        {
            Y = A;
            SetFlag(ProcessorFlags.Z, Y == 0);
            SetFlag(ProcessorFlags.N, ((Y & 0x80) == 0x80));
            IncrementPC(currentInstruction.Length); 
        }

        // Test and Reset memory Bit
        private void TRB() 
        {
            ReadValueFromAddress(operandAddress.Value);
            var val = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, ((val & ~A) == ~A));
            val = ((byte)(val & ~A));
            WriteValueToAddress(operandAddress.Value, val);

            IncrementPC(currentInstruction.Length);
        }

        // Test and Set memory Bit
        private void TSB() 
        {
            ReadValueFromAddress(operandAddress.Value);
            var val = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, ((val & A) == A));
            val = ((byte)(val & A));
            WriteValueToAddress(operandAddress.Value, val);

            IncrementPC(currentInstruction.Length); 
        }

        // Transfer the Stack pointer to the X register
        private void TSX()
        {
            X = ((byte)(SP & 0xFF));

            SetFlag(ProcessorFlags.Z, X == 0x00);
            SetFlag(ProcessorFlags.N, (X & 0x80) == 0x80);

            IncrementPC(currentInstruction.Length);
        }

        // Transfer the X register to the Accumulator
        private void TXA() 
        {
            A = X;
            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);
            IncrementPC(currentInstruction.Length);
        }

        // Transfer the X register to the Stack pointer register
        private void TXS()
        {
            SP = ((ushort)(0x0100 | X));
            IncrementPC(currentInstruction.Length);
        }

        // Transfer Y register to the Accumulator
        private void TYA() 
        {
            A = Y;
            
            SetFlag(ProcessorFlags.Z, A == 0x00);
            SetFlag(ProcessorFlags.N, (A & 0x80) == 0x80);

            IncrementPC(currentInstruction.Length);
        }

        // WAit for Interrupt
        private void WAI() {
            stopCmdEffected = true;
            SetFlag(ProcessorFlags.B, true);
            IncrementPC(currentInstruction.Length); 
        }
    }
}
