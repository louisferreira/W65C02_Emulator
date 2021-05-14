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
        private void Absolute()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)((hi << 8) | lo);
            fetchedByte = null;
        }

        // (a,x)
        private void AbsoluteIndexedIndirect()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;
            var baseAdd = (ushort)(((hi << 8) | lo) + X);
            
            ReadValueFromAddress(baseAdd);
            lo = fetchedByte.Value;
            
            ReadValueFromAddress((ushort)(baseAdd + 1));
            hi = fetchedByte.Value;

            operandAddress = (ushort?)((hi << 8) + lo);
            fetchedByte = null;
        }

        // a,x
        private void AbsoluteIndexedWithX()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)(((hi << 8) | lo) + X);
            fetchedByte = null;
        }

        // a,y
        private void AbsoluteIndexedWithY()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)(((hi << 8) | lo) + Y);
            fetchedByte = null;
        }

        // (a)
        private void AbsoluteIndirect()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;
            var baseAdd = (ushort)((hi << 8) | lo);

            ReadValueFromAddress(baseAdd);
            lo = fetchedByte.Value;

            ReadValueFromAddress((ushort)(baseAdd + 1));
            hi = fetchedByte.Value;

            operandAddress = (ushort?)((hi << 8) + lo);
            fetchedByte = null;
        }

        // A
        private void Accumulator()
        {
            operandAddress = null;
            fetchedByte = null;
        }

        // #
        private void Immediate()
        {
            operandAddress = null;
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
        }

        // i
        private void Implied()
        {
            operandAddress = PC;
            fetchedByte = null;
        }

        // r
        private void ProgramCounterRelative()
        {
            operandAddress = null;
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
        }

        // s
        private void Stack()
        {
            operandAddress = SP;
            fetchedByte = null;
        }

        // zp
        private void ZeroPage()
        {
            fetchedByte = null;
            operandAddress = currentInstruction.Operand1 ?? 0x00;
        }

        // (zp,x)
        private void ZeroPageIndexedIndirect()
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
            fetchedByte = null;
        }

        // zp,x
        private void ZeroPageIndexedWithX()
        {
            fetchedByte = null;
            operandAddress = (ushort)((currentInstruction.Operand1 ?? 0x00) + X);
        }

        // zp,y
        private void ZeroPageIndexedWithY()
        {
            fetchedByte = null;
            operandAddress = (ushort)((currentInstruction.Operand1 ?? 0x00) + Y);
        }

        // (zp)
        private void ZeroPageIndirect()
        {
            var baseAddr = currentInstruction.Operand1 ?? 0x00;
            ReadValueFromAddress(baseAddr);
            var lo = fetchedByte.Value;

            ReadValueFromAddress((ushort)(baseAddr + 1));
            var hi = fetchedByte.Value;
            
            operandAddress = ((ushort?)((hi << 8) + lo));
            fetchedByte = null;
        }

        // (zp),y
        private void ZeroPageIndirectIndexedWithY()
        {
            var baseAdd = currentInstruction.Operand1 ?? 0x00;
            ReadValueFromAddress(baseAdd);
            var lo = fetchedByte.Value;

            ReadValueFromAddress((ushort)(baseAdd + 1));
            var hi = fetchedByte.Value;

            operandAddress = (ushort)(((hi << 8) + lo) + Y);
            fetchedByte = null;
        }
    }
}
