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

        }

        // a,x
        private void AbsoluteIndexedWithX()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)(((hi << 8) | lo) + X);
        }

        // a,y
        private void AbsoluteIndexedWithY()
        {
            var lo = currentInstruction.Operand1 ?? 0x00;
            var hi = currentInstruction.Operand2 ?? 0x00;

            operandAddress = (ushort)(((hi << 8) | lo) + Y);
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

        }

        // A
        private void Accumulator()
        {

        }

        // #
        private void Immediate()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
        }

        // i
        private void Implied()
        {
            operandAddress = PC;
        }

        // r
        private void ProgramCounterRelative()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
        }

        // s
        private void Stack()
        {
            operandAddress = SP;
        }

        // zp
        private void ZeroPage()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            operandAddress = fetchedByte.Value;
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
        }

        // zp,x
        private void ZeroPageIndexedWithX()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            operandAddress = (ushort)(fetchedByte.Value + X);
        }

        // zp,y
        private void ZeroPageIndexedWithY()
        {
            fetchedByte = currentInstruction.Operand1 ?? 0x00;
            operandAddress = (ushort)(fetchedByte.Value + Y);
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
        }
    }
}
