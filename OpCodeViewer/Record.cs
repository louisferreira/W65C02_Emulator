using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpCodeViewer
{
    public class Record
    {
        public string OpCode { get; set; }
        public string Mnemonic { get; set; }
        public string AddressCode { get; set; }
        public string FlagsAffected { get; set; }
        public string OperationSummary { get; set; }
        public string AddressModeDescription { get; set; }
        public string MnemonicDescription { get; set; }
    }
}
