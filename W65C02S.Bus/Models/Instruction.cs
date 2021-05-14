using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02S.CPU.Models
{
    public class Instruction
    {
        public byte OpCode { get; set; }
        public string Mnemonic { get; set; }
        public byte Length { get; set; }
        public string AddressCode { get; set; }
        public string FlagsAffected { get; set; }
        public string OperationDescription { get; set; }
        public Action<byte> Action { get; set; }
        public Func<byte> AddressMode { get; set; }
        public byte? Operand1 { get; private set; }
        public byte? Operand2 { get; private set; }
        public void Set(params byte[] operands)
        {
            if (operands != null)
            {
                if (operands.Length == 1)
                    Operand1 = operands[0];
                if (operands.Length == 2)
                {
                    Operand1 = operands[0];
                    Operand2 = operands[1];
                }
            }
        }
    }
}
