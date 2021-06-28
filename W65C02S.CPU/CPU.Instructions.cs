using System;
using W65C02.API.Enums;
using W65C02.API.EventArgs;

namespace W65C02S.CPU
{
    public partial class CPUCore
    {
        private const byte CarryFlagBit = 1 << 0;
        private const byte ZeroFlagBit = 1 << 1;
        private const byte IntrptFlagBit = 1 << 2;
        private const byte DecimalFlagBit = 1 << 3;
        private const byte BreakFlagBit = 1 << 4;
        private const byte UnusedFlagBit = 1 << 5;
        private const byte OVerflowFlagBit = 1 << 6;
        private const byte NegativeFlagBit = 1 << 7;

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

            byte Cin = (byte)(IsFlagSet(ProcessorFlags.C) ? 1 : 0);
            bool SameSignBits = ((A ^ operand) & NegativeFlagBit) == 0;
            ushort sum = (ushort)(A + operand + Cin);
            byte result = (byte)(sum & 0xFF);
            
            A = result;

            bool Cout = sum > 0xFF;
            bool n = (result & NegativeFlagBit) == NegativeFlagBit;
            bool z = result == 0;
            bool v = SameSignBits && (((A ^ operand) & NegativeFlagBit) > 0);

            SetFlag(ProcessorFlags.C, Cout);
            SetFlag(ProcessorFlags.Z, z);
            SetFlag(ProcessorFlags.N, n);
            SetFlag(ProcessorFlags.V, v);


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

            throw new NotImplementedException();

        }


        // "AND" memory with accumulator
        private void AND()
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

            A = ((byte)(A & operand));

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
            // fetchedByte contains operand1 (ZP address byte to test)
            // operandAddress contains operand2 (zp address to branch to)

            byte mask = 0x00;
            switch (currentInstruction.OpCode)
            {
                case 0x0F: // bit 0
                    mask = 0xFE;
                    break;
                case 0x1F: // bit 1
                    mask = 0xFD;
                    break;
                case 0x2F: // bit 2
                    mask = 0xFB;
                    break;
                case 0x3F: // bit 3
                    mask = 0xF7;
                    break;
                case 0x4F: // bit 4
                    mask = 0xEF;
                    break;
                case 0x5F: // bit 5
                    mask = 0xDF;
                    break;
                case 0x6F: // bit 6
                    mask = 0xBF;
                    break;
                case 0x7F: // bit 7
                    mask = 0x7F;
                    break;
                default:
                    break;
            }
            ReadValueFromAddress(fetchedByte.Value);
            var zpVal = fetchedByte.Value;

            if ((zpVal & mask) == zpVal)
            {
                var amount = (sbyte)(operandAddress.Value);
                PC = (ushort)(PC + amount + currentInstruction.Length);
            }
            else
            {
                IncrementPC(currentInstruction.Length);
            }
        }

        // Branch on Bit Set
        private void BBS()
        {
            // fetchedByte contains operand1 (byte to test)
            // operandAddress contains operand2 (zp address to branch to)
            byte mask = 0x00;
            switch (currentInstruction.OpCode)
            {
                case 0x8F:
                    mask = 1 << 0;
                    break;
                case 0x9F:
                    mask = 1 << 1;
                    break;
                case 0xAF:
                    mask = 1 << 2;
                    break;
                case 0xBF:
                    mask = 1 << 3;
                    break;
                case 0xCF:
                    mask = 1 << 4;
                    break;
                case 0xDF:
                    mask = 1 << 5;
                    break;
                case 0xEF:
                    mask = 1 << 6;
                    break;
                case 0xFF:
                    mask = 1 << 7;
                    break;
                default:
                    break;
            }
            ReadValueFromAddress(fetchedByte.Value);
            var zpVal = fetchedByte.Value;
            if ((zpVal & mask) == mask)
            {
                var amount = (sbyte)(operandAddress.Value);
                PC = (ushort)(PC + amount + currentInstruction.Length);
            }
            else
            {
                IncrementPC(currentInstruction.Length);
            }
        }

        // Branch on Carry Clear (Pc=0)
        private void BCC()
        {
            if (!IsFlagSet(ProcessorFlags.C))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + currentInstruction.Length + amount);
            }
            else
            {
                IncrementPC(currentInstruction.Length);
            }
        }

        // Branch on Carry Set (Pc=1)
        private void BCS()
        {
            if (IsFlagSet(ProcessorFlags.C))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + currentInstruction.Length + amount);
            }
            else
            {
                IncrementPC(currentInstruction.Length);
            }
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
            if (operandAddress.HasValue)
            {
                ReadValueFromAddress(operandAddress.Value);
            }

            byte M = fetchedByte.Value;
            //bits 7 and 6 of operand are transfered to bit 7 and 6 of SR (N,V)
            SetFlag(ProcessorFlags.N, ((A & M) == 0x80));
            SetFlag(ProcessorFlags.V, ((A & M) == 0x40));

            //the zero-flag is set to the result of operand AND accumulator.
            SetFlag(ProcessorFlags.Z, (A & M) == 0);
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
            if (!IsFlagSet(ProcessorFlags.I))
            {
                // save return address to stack, hi byte first then lo byte
                var retAddr = (PC + currentInstruction.Length);
                WriteValueToAddress(SP, (byte)(retAddr >> 8)); // hi byte
                DecreaseSP();

                WriteValueToAddress(SP, (byte)(retAddr)); // lo byte
                DecreaseSP();

                // set the software interupt flag to indicate a break 
                SetFlag(ProcessorFlags.B, true);
                
                // save the Processor Flags to stack
                WriteValueToAddress(SP, (byte)ST);
                DecreaseSP();

                // disable further interupts
                SetFlag(ProcessorFlags.I, true);

                // CMOS version also clears the decimal flag
                SetFlag(ProcessorFlags.D, false);

                // set the PC to the IRQ vector
                PC = IRQ_Vect;
                ReadValueFromAddress(PC);
                var lo = fetchedByte;
                clockTicks++;

                PC++;
                ReadValueFromAddress(PC);
                var hi = fetchedByte;
                clockTicks++;

                PC = (ushort)((hi << 8) | lo);

                var arg = new OnInstructionExecutedEventArg
                {
                    CurrentInstruction = currentInstruction,
                    DecodedInstruction = "Software Interupt (BRK)",
                    A = A,
                    X = X,
                    Y = Y,
                    PC = PC,
                    SP = SP,
                    ST = ST,
                    RawData = $"{currentInstruction.OpCode:X2} {currentInstruction.Operand1:X2} {currentInstruction.Operand2:X2}".TrimEnd(),
                    ClockTicks = clockTicks
                };
                bus?.Publish(arg);
            }

        }

        // Branch on oVerflow Clear (Pv=0)
        private void BVC()
        {
            if (!IsFlagSet(ProcessorFlags.V))
            {
                var amount = (sbyte)(fetchedByte.Value);
                PC = (ushort)(PC + amount);
            }
            else
            {
                IncrementPC(currentInstruction.Length);
            }
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
            {
                IncrementPC(currentInstruction.Length);
            }
        }

        // CLear Cary flag
        private void CLC()
        {
            SetFlag(ProcessorFlags.C, false);
            IncrementPC(currentInstruction.Length);
        }

        // CLear Decimal mode
        private void CLD()
        {
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
            {
                ReadValueFromAddress(operandAddress.Value);
            }

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
            {
                ReadValueFromAddress(operandAddress.Value);
            }

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
            {
                ReadValueFromAddress(operandAddress.Value);
            }

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

            var c = ((operand & 0x01) == 0x01);
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
            SetFlag(ProcessorFlags.B, true); // B flag is always set on PHP
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
            SetFlag(ProcessorFlags.Z, (A == 0));
            SetFlag(ProcessorFlags.N, (A & 0x80) > 0);

            IncrementPC(currentInstruction.Length);
        }

        // PuLl Processor status from stack
        private void PLP()
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            // bits 4 & 5 (U & B) are ignored
            byte temp = fetchedByte.Value;
            temp = (byte)(temp & 0x30);
            var newST = (byte)(fetchedByte.Value ^ temp);
            ST = (ProcessorFlags)(newST ^ 0x30);
            IncrementPC(currentInstruction.Length);
        }

        // PuLl X register from stack
        private void PLX()
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            X = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, (X == 0));
            SetFlag(ProcessorFlags.N, (X & 0x80) > 0);
            IncrementPC(currentInstruction.Length);
        }

        // PuLl Y register from stack
        private void PLY()
        {
            IncreaseSP();
            ReadValueFromAddress(SP);
            Y = fetchedByte.Value;
            SetFlag(ProcessorFlags.Z, (Y == 0));
            SetFlag(ProcessorFlags.N, (Y & 0x80) > 0);
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
            fetchedByte = ((byte)(fetchedByte.Value & ~mask));
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
            // pull the processor status from stack
            ReadValueFromAddress(SP);
            // bits 4 & 5 (U & B) are ignored
            byte temp = fetchedByte.Value;
            temp = (byte)(temp & 0x30);
            var newST = (byte)(fetchedByte.Value ^ temp);
            ST = (ProcessorFlags)(newST ^ 0x30);
            IncreaseSP();

            //restore the PC to the next instruction
            ReadValueFromAddress(SP);
            IncreaseSP();
            var lo = fetchedByte.Value;

            ReadValueFromAddress(SP);
            IncreaseSP();
            var hi = fetchedByte.Value;

            // re-enable interupts
            SetFlag(ProcessorFlags.I, false);
            // clear the break flag
            SetFlag(ProcessorFlags.B, false);

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
        private void SBC() 
        {
            // same as ADC, but the operand is negated
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

            operand = (byte)~operand;

            byte Cin = (byte)(IsFlagSet(ProcessorFlags.C) ? 1 : 0);
            bool SameSignBits = ((A ^ operand) & NegativeFlagBit) == 0;
            ushort sum = (ushort)(A + operand + Cin);
            byte result = (byte)(sum & 0xFF);

            A = result;

            bool Cout = sum > 0xFF;
            bool n = (result & NegativeFlagBit) == NegativeFlagBit;
            bool z = result == 0;
            bool v = SameSignBits && (((A ^ operand) & NegativeFlagBit) > 0);

            SetFlag(ProcessorFlags.C, Cout);
            SetFlag(ProcessorFlags.Z, z);
            SetFlag(ProcessorFlags.N, n);
            SetFlag(ProcessorFlags.V, v);

            IncrementPC(currentInstruction.Length);
        }

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
            fetchedByte = (byte)(fetchedByte | mask);
            WriteValueToAddress(operandAddress.Value, fetchedByte.Value);

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
        private void STP()
        {
            //SetFlag(ProcessorFlags.B, true);
            stopCmdAsserted = true;
        }

        // STore Zero in memory
        private void STZ()
        {

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
            val = ((byte)(val & ~A));
            WriteValueToAddress(operandAddress.Value, val);

            SetFlag(ProcessorFlags.Z, ((val & A) == 0));
            IncrementPC(currentInstruction.Length);
        }

        // Test and Set memory Bit
        private void TSB()
        {
            ReadValueFromAddress(operandAddress.Value);
            var val = fetchedByte.Value;
            val = ((byte)(val | A));
            WriteValueToAddress(operandAddress.Value, val);

            SetFlag(ProcessorFlags.Z, ((val & A) == 0));
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
        private void WAI()
        {
            stopCmdAsserted = true;
            SetFlag(ProcessorFlags.B, true);
            IncrementPC(currentInstruction.Length);
        }
    }
}
