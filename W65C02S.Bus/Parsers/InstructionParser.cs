using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using W65C02S.CPU.Models;

namespace W65C02S.Engine.Parsers
{
    public static class InstructionParser
    {
        public static string Parse(Instruction currentInstruction)
        {
            var operandFormatString = GetAddressModeFormat(currentInstruction.AddressCode);
            var operands = currentInstruction.Operand2.HasValue
                ? String.Format(operandFormatString, currentInstruction.Operand2, currentInstruction.Operand1)
                : String.Format(operandFormatString, currentInstruction.Operand1, "");
            return $"{currentInstruction.Mnemonic} {operands}";
        }

        private static string GetAddressModeFormat(string addMode)
        {
            switch (addMode)
            {
                case "a":
                    return "${0:X2}{1:X2} (Absolute)";
                case "(a,x)":
                    return "(${0:X2},${1:X2}) (Absolute Indexed Indirect)";
                case "a,x":
                    return "${0:X2},${1:X2} (Absolute Indexed with X)";
                case "a,y":
                    return "${0:X2},${1:X2} (Absolute Indexed with Y)";
                case "(a)":
                    return "(${0:X2}{1:X2}) (Absolute Indirect)";
                case "A":
                    return "${0:X2} (Accumulator)";
                case "#":
                    return "#${0:X2} (Immediate)";
                case "i":
                    return "(Implied)";
                case "r":
                    return "${0:X2} (Program Counter Relative)";
                case "s":
                    return "(Stack Pointer)";
                case "zp":
                    return "${0:X2} (Zero Page)";
                case "(zp,x)":
                    return "(${0:X2},X) (Zero Page Indexed Indirect)";
                case "zp,x":
                    return "${0:X2},X (Zero Page Indexed with X)";
                case "zp,y":
                    return "${0:X2},Y (Zero Page Indexed with Y)";
                case "(zp)":
                    return "(${0:X2}) (Zero Page Indirect)";
                case "(zp),y":
                    return "(${0:X2}),Y (Zero Page Indirect Indexed with Y)";
                default:
                    return "";
            }
        }
    }
}
