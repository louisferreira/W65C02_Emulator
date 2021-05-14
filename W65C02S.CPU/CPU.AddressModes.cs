using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02S.CPU
{
    public partial class CPUCore
    {
        // a
        private byte Absolute()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)((hi << 8) | lo);
            return 3;
        }

        // (a,x)
        private byte AbsoluteIndexedIndirect()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;
            var baseAdd = (ushort)(((hi << 8) | lo) + X);
            
            ReadValueFromAddress(baseAdd);
            lo = fetchedByte.Value;
            
            ReadValueFromAddress((ushort)(baseAdd + 1));
            hi = fetchedByte.Value;

            operandAddress = (ushort?)((hi << 8) + lo);

            return 3;
        }

        // a,x
        private byte AbsoluteIndexedWithX()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)(((hi << 8) | lo) + X);
            return 3;
        }

        // a,y
        private byte AbsoluteIndexedWithY()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)(((hi << 8) | lo) + Y);
            return 3;
        }

        // (a)
        private byte AbsoluteIndirect()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;
            var baseAdd = (ushort)((hi << 8) | lo);

            ReadValueFromAddress(baseAdd);
            lo = fetchedByte.Value;

            ReadValueFromAddress((ushort)(baseAdd + 1));
            hi = fetchedByte.Value;

            operandAddress = (ushort?)((hi << 8) + lo);

            return 3;
        }

        // A
        private byte Accumulator()
        {
            return 1;
        }

        // #
        private byte Immediate()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            return 2;
        }

        // i
        private byte Implied()
        {
            operandAddress = PC;
            return 1;
        }

        // r
        private byte ProgramCounterRelative()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            return 2;
        }

        // s
        private byte Stack()
        {
            operandAddress = SP;
            return 1;
        }

        // zp
        private byte ZeroPage()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            operandAddress = fetchedByte.Value;
            return 2;
        }

        // (zp,x)
        private byte ZeroPageIndexedIndirect()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            var baseAdd = fetchedByte.Value;
            operandAddress = (ushort)(baseAdd + X);
            ReadValueFromAddress(operandAddress.Value);
            var lo = fetchedByte.Value;

            operandAddress = (ushort)(lo + X + 1);
            ReadValueFromAddress(operandAddress.Value);
            var hi = fetchedByte.Value;
            
            operandAddress = (ushort?)((hi << 8) + lo);
            return 2;
        }

        // zp,x
        private byte ZeroPageIndexedWithX()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            operandAddress = (ushort)(fetchedByte.Value + X);
            return 2;
        }

        // zp,y
        private byte ZeroPageIndexedWithY()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            operandAddress = (ushort)(fetchedByte.Value + Y);
            return 2;
        }

        // (zp)
        private byte ZeroPageIndirect()
        {
            var baseAddr = currentInstruction.Operand1 ?? 0x00;
            ReadValueFromAddress(baseAddr);
            var lo = fetchedByte.Value;

            ReadValueFromAddress((ushort)(baseAddr + 1));
            var hi = fetchedByte.Value;
            
            operandAddress = ((ushort?)((hi << 8) + lo));
            return 2;
        }

        // (zp),y
        private byte ZeroPageIndirectIndexedWithY()
        {
            var baseAdd = currentInstruction.Operand1 ?? 0x00;
            ReadValueFromAddress(baseAdd);
            var lo = fetchedByte.Value;

            ReadValueFromAddress((ushort)(baseAdd + 1));
            var hi = fetchedByte.Value;

            operandAddress = (ushort)(((hi << 8) + lo) + Y);
            return 2;
        }
    }
}
