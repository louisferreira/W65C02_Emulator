using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpCodeViewer
{
    class Program
    {
        private const int screenWidth = 180;
        private static List<Record> Records = new List<Record>();
        private static List<Record> Filtered;
        private static Dictionary<string, int> columnWidths = new Dictionary<string, int>
        {
            { "OpCode",  8},
            { "Mnemonic", 9},
            { "Length", 7},
            { "Flags", 9},
            { "Address", 10},
            { "Mode", 35},
            { "Summary", 29},
            { "Description",40},
        };

        static void Main(string[] args)
        {
            //Console.SetWindowSize(screenWidth, 40);
            
            Console.BufferWidth = screenWidth;

            Console.Title = "W65C02 OpCode Viewer";
            CreateData();


        WaitForInput:
            Console.ForegroundColor = ConsoleColor.Green;
            ShowMenu();

            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.X)
            {
                goto Exit;
            }

            if (key.Key == ConsoleKey.F1)
            {
                F1Routine();
            }
            if (key.Key == ConsoleKey.F2)
            {
                F2Routine();
            }
            if (key.Key == ConsoleKey.F3)
            {
                F3Routine();
            }
            if (key.Key == ConsoleKey.F4)
            {
                F4Routine();
            }



            goto WaitForInput;


            
        Exit:
            Console.WriteLine("Bye...");

        }

        private static void DisplayResults(string subMenuTxt)
        {
            Console.Clear();
            DisplayResultsHeading();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            if(!Filtered.Any())
                Console.WriteLine(" (no records found.)");
            
            foreach (var record in Filtered)
            {
                Console.Write($"{record.OpCode}".PadRight(columnWidths["OpCode"], ' '));
                Console.Write($"{record.Mnemonic}".PadRight(columnWidths["Mnemonic"], ' '));
                Console.Write($"{record.Length}".PadRight(columnWidths["Length"], ' '));
                Console.Write($"{record.FlagsAffected}".PadRight(columnWidths["Flags"], ' '));
                Console.Write($"{record.AddressCode}".PadRight(columnWidths["Address"], ' '));
                Console.Write($"{record.AddressModeDescription}".PadRight(columnWidths["Mode"], ' '));
                Console.Write($"{record.OperationSummary}".PadRight(columnWidths["Summary"], ' '));
                Console.WriteLine($"{record.MnemonicDescription}".PadRight(columnWidths["Description"], ' '));
            }
            Console.WriteLine("".PadRight(screenWidth-2, '-'));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Press <Enter> to go back.");
            Console.Write(subMenuTxt);
        }
        private static void DisplayResultsHeading()
        {
            Console.WriteLine("".PadRight(screenWidth-2, '-'));

            foreach (var item in columnWidths)
            {
                Console.Write( $"{item.Key}".PadRight(item.Value, ' '));
            }
            Console.WriteLine();

            Console.WriteLine("".PadRight(screenWidth-2, '-'));

        }
        private static void ShowMenu()
        {
            Console.Clear();

            Console.WriteLine("".PadRight(screenWidth-2, '-'));
            Console.WriteLine("                 W65C02 OpCode Viewer");
            Console.WriteLine("           Written by Louis Ferreira ©2021");
            Console.WriteLine("".PadRight(screenWidth-2, '-'));
            Console.WriteLine(" F1 - Search for OpCode          e.g. 'A9',  '4C',  'B*'");
            Console.WriteLine(" F2 - Search for Mnemonic        e.g. 'LDA', 'TAX', 'BN*'");
            Console.WriteLine(" F3 - Search for Address Code    e.g. '(x,y)'");
            Console.WriteLine(" F4 - Search for Flags Affected  e.g. 'Z'");
            Console.WriteLine(" X  - Exit");
            Console.WriteLine("".PadRight(screenWidth-2, '-'));
            //Console.WriteLine($"{Records.Count} records loaded.");
            Console.WriteLine("Press Menu choice key.");
            Console.Write(">");
        }



        private static void F1Routine()
        {
            Console.Clear();
            var subMenuTxt = "**Search for OpCode: >";
            Console.Write(subMenuTxt);
        start:
            var opcode = Console.ReadLine();
            if (string.IsNullOrEmpty(opcode))
                return;

            Filtered = Records.Where(x => x.OpCode.StartsWith(opcode.ToUpper())).ToList();

            DisplayResults(subMenuTxt);

            goto start;

        }
        private static void F2Routine()
        {
            Console.Clear();
            var subMenuTxt = "**Search for Mnemonic: >";
            Console.Write(subMenuTxt);
            start:
            var mnemonic = Console.ReadLine();
            if (string.IsNullOrEmpty(mnemonic))
                return;

            Filtered = Records.Where(x => x.Mnemonic.StartsWith(mnemonic.ToUpper())).ToList();

            DisplayResults(subMenuTxt);
            goto start;
        }
        private static void F3Routine()
        {
            Console.Clear();
            var subMenuTxt = "Search for Address Code: >";
            Console.Write(subMenuTxt);
            start:
            var addrCode = Console.ReadLine();
            if (string.IsNullOrEmpty(addrCode))
                return;
            Filtered = Records.Where(x => x.AddressCode == addrCode).OrderBy(x => x.Mnemonic).ToList();
            DisplayResults(subMenuTxt);
            goto start;
        }
        private static void F4Routine()
        {
            Console.Clear();
            var subMenuTxt = "Search for Flags Affected: >";
            Console.Write(subMenuTxt);
            start:
            var flags = Console.ReadLine();
            if (string.IsNullOrEmpty(flags))
                return;
            Filtered = Records.Where(x => x.FlagsAffected.Contains(flags.ToUpper())).OrderBy(x => x.Mnemonic).ToList();
            DisplayResults(subMenuTxt);
            goto start;
        }

        private static void CreateData()
        {
            Records = new List<Record>() {
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "6D", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "7D", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "79", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "69", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "65", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "61", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "75", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "72", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "ADC", OperationSummary = "A+M+C->A", FlagsAffected = "NVZC", OpCode = "71", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "2D", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "3D", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "39", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "29", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "25", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "21", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "35", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "32", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "AND", OperationSummary = "A^M->A", FlagsAffected = "NZ", OpCode = "31", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "ASL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", OpCode = "0E", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "ASL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", OpCode = "1E", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "ASL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", OpCode = "0A", AddressCode = "A", Length = 1} },
            { new Record{Mnemonic = "ASL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", OpCode = "06", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "ASL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <-0", FlagsAffected = "NZC", OpCode = "16", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "BBR0", OperationSummary = "Branch on bit 0 reset", FlagsAffected = "", OpCode = "0F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR1", OperationSummary = "Branch on bit 1 reset", FlagsAffected = "", OpCode = "1F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR2", OperationSummary = "Branch on bit 2 reset", FlagsAffected = "", OpCode = "2F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR3", OperationSummary = "Branch on bit 3 reset", FlagsAffected = "", OpCode = "3F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR4", OperationSummary = "Branch on bit 4 reset", FlagsAffected = "", OpCode = "4F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR5", OperationSummary = "Branch on bit 5 reset", FlagsAffected = "", OpCode = "5F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR6", OperationSummary = "Branch on bit 6 reset", FlagsAffected = "", OpCode = "6F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBR7", OperationSummary = "Branch on bit 7 reset", FlagsAffected = "", OpCode = "7F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS0", OperationSummary = "Branch on bit 0 set", FlagsAffected = "", OpCode = "8F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS1", OperationSummary = "Branch on bit 1 set", FlagsAffected = "", OpCode = "9F", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS2", OperationSummary = "Branch on bit 2 set", FlagsAffected = "", OpCode = "AF", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS3", OperationSummary = "Branch on bit 3 set", FlagsAffected = "", OpCode = "BF", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS4", OperationSummary = "Branch on bit 4 set", FlagsAffected = "", OpCode = "CF", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS5", OperationSummary = "Branch on bit 5 set", FlagsAffected = "", OpCode = "DF", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS6", OperationSummary = "Branch on bit 6 set", FlagsAffected = "", OpCode = "EF", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BBS7", OperationSummary = "Branch on bit 7 set", FlagsAffected = "", OpCode = "FF", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BCC", OperationSummary = "Branch C = 0", FlagsAffected = "", OpCode = "90", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BCS", OperationSummary = "Branch if C=1", FlagsAffected = "", OpCode = "B0", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BEQ", OperationSummary = "Branch if Z=1", FlagsAffected = "", OpCode = "F0", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BIT", OperationSummary = "A^M", FlagsAffected = "NVZ", OpCode = "2C", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "BIT", OperationSummary = "A^M", FlagsAffected = "NVZ", OpCode = "3C", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "BIT", OperationSummary = "A^M", FlagsAffected = "NVZ", OpCode = "89", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "BIT", OperationSummary = "A^M", FlagsAffected = "NVZ", OpCode = "24", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "BIT", OperationSummary = "A^M", FlagsAffected = "NVZ", OpCode = "34", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "BMI", OperationSummary = "Branch if N=1", FlagsAffected = "", OpCode = "30", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BNE", OperationSummary = "Branch if Z=0", FlagsAffected = "", OpCode = "D0", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BPL", OperationSummary = "Branch if N=0", FlagsAffected = "", OpCode = "10", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BRA", OperationSummary = "Branch Always", FlagsAffected = "", OpCode = "80", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BRK", OperationSummary = "Break", FlagsAffected = "BZ", OpCode = "00", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "BVC", OperationSummary = "Branch if V=0", FlagsAffected = "", OpCode = "50", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "BVS", OperationSummary = "Branch if V=1", FlagsAffected = "", OpCode = "70", AddressCode = "r", Length = 2} },
            { new Record{Mnemonic = "CLC", OperationSummary = "0 -> C", FlagsAffected = "C", OpCode = "18", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "CLD", OperationSummary = "0 -> D", FlagsAffected = "D", OpCode = "D8", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "CLI", OperationSummary = "0 -> I", FlagsAffected = "I", OpCode = "58", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "CLV", OperationSummary = "0 -> V", FlagsAffected = "V", OpCode = "B8", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "CD", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "DD", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "D9", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "C9", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "C5", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "C1", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "D5", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "D2", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "CMP", OperationSummary = "A-M", FlagsAffected = "NZC", OpCode = "D1", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "CPX", OperationSummary = "X-M", FlagsAffected = "NZC", OpCode = "EC", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "CPX", OperationSummary = "X-M", FlagsAffected = "NZC", OpCode = "E0", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "CPX", OperationSummary = "X-M", FlagsAffected = "NZC", OpCode = "E4", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "CPY", OperationSummary = "Y-M", FlagsAffected = "NZC", OpCode = "CC", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "CPY", OperationSummary = "Y-M", FlagsAffected = "NZC", OpCode = "C0", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "CPY", OperationSummary = "Y-M", FlagsAffected = "NZC", OpCode = "C4", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "DEC", OperationSummary = "Decrement addressed location", FlagsAffected = "NZ", OpCode = "CE", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "DEC", OperationSummary = "Decrement addressed location", FlagsAffected = "NZ", OpCode = "DE", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "DEC", OperationSummary = "Decrement addressed location", FlagsAffected = "NZ", OpCode = "3A", AddressCode = "A", Length = 1} },
            { new Record{Mnemonic = "DEC", OperationSummary = "Decrement addressed location", FlagsAffected = "NZ", OpCode = "C6", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "DEC", OperationSummary = "Decrement addressed location", FlagsAffected = "NZ", OpCode = "D6", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "DEX", OperationSummary = "X-1 -> X", FlagsAffected = "NZ", OpCode = "CA", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "DEY", OperationSummary = "Y-1 -> Y", FlagsAffected = "NZ", OpCode = "88", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "4D", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "5D", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "59", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "49", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "45", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "41", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "55", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "52", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "EOR", OperationSummary = "A^M -> A", FlagsAffected = "NZ", OpCode = "51", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "INC", OperationSummary = "Increment addressed location", FlagsAffected = "NZ", OpCode = "EE", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "INC", OperationSummary = "Increment addressed location", FlagsAffected = "NZ", OpCode = "FE", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "INC", OperationSummary = "Increment addressed location", FlagsAffected = "NZ", OpCode = "1A", AddressCode = "A", Length = 1} },
            { new Record{Mnemonic = "INC", OperationSummary = "Increment addressed location", FlagsAffected = "NZ", OpCode = "E6", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "INC", OperationSummary = "Increment addressed location", FlagsAffected = "NZ", OpCode = "F6", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "INX", OperationSummary = "X+1 -> X", FlagsAffected = "NZ", OpCode = "E8", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "INY", OperationSummary = "Y+1 -> Y", FlagsAffected = "NZ", OpCode = "C8", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "JMP", OperationSummary = "Jump to new location", FlagsAffected = "", OpCode = "4C", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "JMP", OperationSummary = "Jump to new location", FlagsAffected = "", OpCode = "7C", AddressCode = "(a,x)", Length = 3} },
            { new Record{Mnemonic = "JMP", OperationSummary = "Jump to new location", FlagsAffected = "", OpCode = "6C", AddressCode = "(a)", Length = 3} },
            { new Record{Mnemonic = "JSR", OperationSummary = "Jump to Subroutine", FlagsAffected = "NZ", OpCode = "20", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "AD", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "BD", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "B9", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "A9", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "A5", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "A1", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "B5", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "B2", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "LDA", OperationSummary = "M -> A", FlagsAffected = "NZ", OpCode = "B1", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "LDX", OperationSummary = "M -> X", FlagsAffected = "NZ", OpCode = "AE", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "LDX", OperationSummary = "M -> X", FlagsAffected = "NZ", OpCode = "BE", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "LDX", OperationSummary = "M -> X", FlagsAffected = "NZ", OpCode = "A2", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "LDX", OperationSummary = "M -> X", FlagsAffected = "NZ", OpCode = "A6", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "LDX", OperationSummary = "M -> X", FlagsAffected = "NZ", OpCode = "B6", AddressCode = "zp,y", Length = 2} },
            { new Record{Mnemonic = "LDY", OperationSummary = "M -> Y", FlagsAffected = "NZ", OpCode = "AC", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "LDY", OperationSummary = "M -> Y", FlagsAffected = "NZ", OpCode = "BC", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "LDY", OperationSummary = "M -> Y", FlagsAffected = "NZ", OpCode = "A0", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "LDY", OperationSummary = "M -> Y", FlagsAffected = "NZ", OpCode = "A4", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "LDY", OperationSummary = "M -> Y", FlagsAffected = "NZ", OpCode = "B4", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "LSR", OperationSummary = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", OpCode = "4E", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "LSR", OperationSummary = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", OpCode = "5E", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "LSR", OperationSummary = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", OpCode = "4A", AddressCode = "A", Length = 1} },
            { new Record{Mnemonic = "LSR", OperationSummary = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", OpCode = "46", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "LSR", OperationSummary = "0 -> 7 6 5 4 3 2 1 0->C", FlagsAffected = "NZC", OpCode = "56", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "NOP", OperationSummary = "No Operation", FlagsAffected = "", OpCode = "EA", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "0D", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "1D", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "19", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "09", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "05", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "01", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "15", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "12", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "ORA", OperationSummary = "A V M -> A", FlagsAffected = "NZ", OpCode = "11", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "PHA", OperationSummary = "A -> Ms, S-1 -> S", FlagsAffected = "", OpCode = "48", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PHP", OperationSummary = "P -> Ms, S-1 -> S", FlagsAffected = "", OpCode = "08", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PHX", OperationSummary = "X -> Ms, S-1 -> S", FlagsAffected = "", OpCode = "DA", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PHY", OperationSummary = "Y -> Ms, S-1 -> S", FlagsAffected = "", OpCode = "5A", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PLA", OperationSummary = "S + 1->S, Ms -> A", FlagsAffected = "NZ", OpCode = "68", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PLP", OperationSummary = "S + 1->S, Ms -> P", FlagsAffected = "NVDIZC", OpCode = "28", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PLX", OperationSummary = "S + 1->S, Ms -> X", FlagsAffected = "NZ", OpCode = "FA", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "PLY", OperationSummary = "S + 1->S, Ms -> Y", FlagsAffected = "NZ", OpCode = "7A", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "RMB0", OperationSummary = "Reset Memory Bit 0", FlagsAffected = "", OpCode = "07", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB1", OperationSummary = "Reset Memory Bit 1", FlagsAffected = "", OpCode = "17", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB2", OperationSummary = "Reset Memory Bit 2", FlagsAffected = "", OpCode = "27", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB3", OperationSummary = "Reset Memory Bit 3", FlagsAffected = "", OpCode = "37", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB4", OperationSummary = "Reset Memory Bit 4", FlagsAffected = "", OpCode = "47", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB5", OperationSummary = "Reset Memory Bit 5", FlagsAffected = "", OpCode = "57", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB6", OperationSummary = "Reset Memory Bit 6", FlagsAffected = "", OpCode = "67", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "RMB7", OperationSummary = "Reset Memory Bit 7", FlagsAffected = "", OpCode = "77", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "ROL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", OpCode = "2E", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "ROL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", OpCode = "3E", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "ROL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", OpCode = "2A", AddressCode = "A", Length = 1} },
            { new Record{Mnemonic = "ROL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", OpCode = "26", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "ROL", OperationSummary = "C<-7 6 5 4 3 2 1 0 <- C", FlagsAffected = "NZC", OpCode = "36", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "ROR", OperationSummary = "C->7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", OpCode = "6E", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "ROR", OperationSummary = "C->7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", OpCode = "7E", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "ROR", OperationSummary = "C->7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", OpCode = "6A", AddressCode = "A", Length = 1} },
            { new Record{Mnemonic = "ROR", OperationSummary = "C->7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", OpCode = "66", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "ROR", OperationSummary = "C->7 6 5 4 3 2 1 0 -> C", FlagsAffected = "NZC", OpCode = "76", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "RTI", OperationSummary = "Return from Interrupt", FlagsAffected = "NVDIZC", OpCode = "40", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "RTS", OperationSummary = "Return from Subroutine", FlagsAffected = "", OpCode = "60", AddressCode = "s", Length = 1} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "ED", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "FD", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "F9", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "E9", AddressCode = "#", Length = 2} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "E5", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "E1", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "F5", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "F2", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "SBC", OperationSummary = "A - M - (~C) -> A", FlagsAffected = "NVZC", OpCode = "F1", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "SEC", OperationSummary = "1 -> C", FlagsAffected = "C", OpCode = "38", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "SED", OperationSummary = "1 -> D", FlagsAffected = "D", OpCode = "F8", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "SEI", OperationSummary = "1 -> I", FlagsAffected = "I", OpCode = "78", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "SMB0", OperationSummary = "Set Memory Bit 0", FlagsAffected = "", OpCode = "87", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB1", OperationSummary = "Set Memory Bit 1", FlagsAffected = "", OpCode = "97", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB2", OperationSummary = "Set Memory Bit 2", FlagsAffected = "", OpCode = "A7", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB3", OperationSummary = "Set Memory Bit 3", FlagsAffected = "", OpCode = "B7", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB4", OperationSummary = "Set Memory Bit 4", FlagsAffected = "", OpCode = "C7", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB5", OperationSummary = "Set Memory Bit 5", FlagsAffected = "", OpCode = "D7", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB6", OperationSummary = "Set Memory Bit 6", FlagsAffected = "", OpCode = "E7", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "SMB7", OperationSummary = "Set Memory Bit 7", FlagsAffected = "", OpCode = "F7", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "8D", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "9D", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "99", AddressCode = "a,y", Length = 3} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "85", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "81", AddressCode = "(zp,x)", Length = 2} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "95", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "92", AddressCode = "(zp)", Length = 2} },
            { new Record{Mnemonic = "STA", OperationSummary = "A -> M", FlagsAffected = "", OpCode = "91", AddressCode = "(zp),y", Length = 2} },
            { new Record{Mnemonic = "STP", OperationSummary = "STOP (1-> PHI2)", FlagsAffected = "", OpCode = "DB", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "STX", OperationSummary = "X -> M", FlagsAffected = "", OpCode = "8E", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "STX", OperationSummary = "X -> M", FlagsAffected = "", OpCode = "86", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "STX", OperationSummary = "X -> M", FlagsAffected = "", OpCode = "96", AddressCode = "zp,y", Length = 2} },
            { new Record{Mnemonic = "STY", OperationSummary = "Y -> M", FlagsAffected = "", OpCode = "8C", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "STY", OperationSummary = "Y -> M", FlagsAffected = "", OpCode = "84", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "STY", OperationSummary = "Y -> M", FlagsAffected = "", OpCode = "94", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "STZ", OperationSummary = "00 -> M", FlagsAffected = "", OpCode = "9C", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "STZ", OperationSummary = "00 -> M", FlagsAffected = "", OpCode = "9E", AddressCode = "a,x", Length = 3} },
            { new Record{Mnemonic = "STZ", OperationSummary = "00 -> M", FlagsAffected = "", OpCode = "64", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "STZ", OperationSummary = "00 -> M", FlagsAffected = "", OpCode = "74", AddressCode = "zp,x", Length = 2} },
            { new Record{Mnemonic = "TAX", OperationSummary = "A -> X", FlagsAffected = "NZ", OpCode = "AA", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "TAY", OperationSummary = "M -> X", FlagsAffected = "NZ", OpCode = "A8", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "TRB", OperationSummary = "~A^M -> M", FlagsAffected = "Z", OpCode = "1C", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "TRB", OperationSummary = "~A^M -> M", FlagsAffected = "Z", OpCode = "14", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "TSB", OperationSummary = "A^M -> M", FlagsAffected = "Z", OpCode = "0C", AddressCode = "a", Length = 3} },
            { new Record{Mnemonic = "TSB", OperationSummary = "A^M -> M", FlagsAffected = "Z", OpCode = "04", AddressCode = "zp", Length = 2} },
            { new Record{Mnemonic = "TSX", OperationSummary = "S -> X", FlagsAffected = "NZ", OpCode = "BA", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "TXA", OperationSummary = "X -> A", FlagsAffected = "NZ", OpCode = "8A", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "TXS", OperationSummary = "X -> S", FlagsAffected = "", OpCode = "9A", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "TYA", OperationSummary = "Y -> A", FlagsAffected = "NZ", OpCode = "98", AddressCode = "i", Length = 1} },
            { new Record{Mnemonic = "WAI", OperationSummary = "0 -> RDY", FlagsAffected = "", OpCode = "CB", AddressCode = "i", Length = 1} }

            };

            Records.ForEach(r => FillMetaData(r));
        }

        private static void FillMetaData(Record record)
        {
            record.AddressModeDescription = GetAddressModeDescription(record.AddressCode);
            record.MnemonicDescription = GetMnumonicDescription(record.Mnemonic);
        }
        private static string GetAddressModeDescription(string addMode)
        {
            switch (addMode)
            {
                case "a":
                    return "Absolute";
                case "(a,x)":
                    return "Absolute Indexed Indirect";
                case "a,x":
                    return "Absolute Indexed with X";
                case "a,y":
                    return "Absolute Indexed with Y";
                case "(a)":
                    return "Absolute Indirect";
                case "A":
                    return "Accumulator";
                case "#":
                    return "Immediate";
                case "i":
                    return "Implied";
                case "r":
                    return "Program Counter Relative";
                case "s":
                    return "Stack Pointer";
                case "zp":
                    return "Zero Page";
                case "(zp,x)":
                    return "Zero Page Indexed Indirect";
                case "zp,x":
                    return "Zero Page Indexed with X";
                case "zp,y":
                    return "Zero Page Indexed with Y";
                case "(zp)":
                    return "Zero Page Indirect";
                case "(zp),y":
                    return "Zero Page Indirect Indexed with Y";
                default:
                    return "";
            }
        }

        private static string GetMnumonicDescription(string mnu)
        {
            switch (mnu)
            {
                case "ADC": return "ADd memory to accumulator with Carry";
                case "AND": return "\"AND\" memory with accumulator";
                case "ASL": return "Arithmetic Shift one bit Left, memory or accumulator";
                case "BBR": return "Branch on Bit Reset";
                case "BBS": return "Branch of Bit Set";
                case "BCC": return "Branch on Carry Clear (Pc=0)";
                case "BCS": return "Branch on Carry Set (Pc=1)";
                case "BEQ": return "Branch if EQual (Pz=1)";
                case "BIT": return "BIt Test";
                case "BMI": return "Branch if result MInus (Pn=1)";
                case "BNE": return "Branch if Not Equal (Pz=0)";
                case "BPL": return "Branch if result PLus (Pn=0)";
                case "BRA": return "BRanch Always";
                case "BRK": return "BReaK instruction";
                case "BVC": return "Branch on oVerflow Clear (Pv=0)";
                case "BVS": return "Branch on oVerflow Set (Pv=1)";
                case "CLC": return "CLear Cary flag";
                case "CLD": return "CLear Decimal mode";
                case "CLI": return "CLear Interrupt disable bit";
                case "CLV": return "CLear oVerflow flag";
                case "CMP": return "CoMPare memory and accumulator";
                case "CPX": return "ComPare memory and X register";
                case "CPY": return "ComPare memory and Y register";
                case "DEC": return "DECrement memory or accumulate by one";
                case "DEX": return "DEcrement X by one";
                case "DEY": return "DEcrement Y by one";
                case "EOR": return "\"Exclusive OR\" memory with accumulate";
                case "INC": return "INCrement memory or accumulate by one";
                case "INX": return "INcrement X register by one";
                case "INY": return "INcrement Y register by one";
                case "JMP": return "JuMP to new location";
                case "JSR": return "Jump to new location Saving Return (Jump to SubRoutine)";
                case "LDA": return "LoaD Accumulator with memory";
                case "LDX": return "LoaD the X register with memory";
                case "LDY": return "LoaD the Y register with memory";
                case "LSR": return "Logical Shift one bit Right memory or accumulator";
                case "NOP": return "No OPeration";
                case "ORA": return "\"OR\" memory with Accumulator";
                case "PHA": return "PusH Accumulator on stack";
                case "PHP": return "PusH Processor status on stack";
                case "PHX": return "PusH X register on stack";
                case "PHY": return "PusH Y register on stack";
                case "PLA": return "PuLl Accumulator from stack";
                case "PLP": return "PuLl Processor status from stack";
                case "PLX": return "PuLl X register from stack";
                case "PLY": return "PuLl Y register from stack";
                case "RMB": return "Reset Memory Bit";
                case "ROL": return "ROtate one bit Left memory or accumulator";
                case "ROR": return "ROtate one bit Right memory or accumulator";
                case "RTI": return "ReTurn from Interrupt";
                case "RTS": return "ReTurn from Subroutine";
                case "SBC": return "SuBtract memory from accumulator with borrow (Crry bit)";
                case "SEC": return "SEt Carry";
                case "SED": return "SEt Decimal mode";
                case "SEI": return "SEt Interrupt disable status";
                case "SMB": return "Set Memory Bit";
                case "STA": return "STore Accumulator in memory";
                case "STP": return "SToP mode";
                case "STX": return "STore the X register in memory";
                case "STY": return "STore the Y register in memory";
                case "STZ": return "STore Zero in memory";
                case "TAX": return "Transfer the Accumulator to the X register";
                case "TAY": return "Transfer the Accumulator to the Y register";
                case "TRB": return "Test and Reset memory Bit";
                case "TSB": return "Test and Set memory Bit";
                case "TSX": return "Transfer the Stack pointer to the X register";
                case "TXA": return "Transfer the X register to the Accumulator";
                case "TXS": return "Transfer the X register to the Stack pointer register";
                case "TYA": return "Transfer Y register to the Accumulator";
                case "WAI": return "WAit for Interrupt";
                default : return "";
            }
        }
    }
}
