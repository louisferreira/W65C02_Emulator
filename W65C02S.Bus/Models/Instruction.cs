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
        public Action Action { get; set; }
        public Action AddressMode { get; set; }
        public byte? Operand1 { get; set; }
        public byte? Operand2 { get; set; }

    }
}
