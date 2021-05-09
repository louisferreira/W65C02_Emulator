using W6502C.CPU;
using System;

namespace Console.Emulator
{
    class Program
    {
        static W65C02S cpu;
        static RAM ram;
        private static bool breakMode = false;
        static void Main(string[] args)
        {
            var bus = new Bus();
            ram = new RAM(64);
            bus.Connect(ram);

            cpu = new W65C02S(bus);
            cpu.OnError += Cpu_OnError;
            cpu.OnInstructionExecuted += Cpu_OnInstructionExecuted;
            cpu.OnDataRead += Cpu_OnDataRead;
            cpu.OnDataWrite += Cpu_OnDataWrite;
            cpu.OnBreakPoint += Cpu_OnBreakPoint;

        Reset:
            System.Console.Clear();
            DisplayMenu();

            cpu.Reset();

        WaitForInput:
            var key = System.Console.ReadKey(true);
            if (key.Key == ConsoleKey.X)
            {
                goto Exit;
            }
            if (key.Key == ConsoleKey.F1)
            {
                DisplayMemoryRange();
            }
            if (key.Key == ConsoleKey.F2)
            {
                LoadBinaryFile();
            }
            if (key.Key == ConsoleKey.F3)
            {
                DisplayRegisters();
            }
            if (key.Key == ConsoleKey.F4)
            {
                SetPCValue();
            }
            if (key.Key == ConsoleKey.F5)
            {
                cpu.Run();
            }
            if (key.Key == ConsoleKey.F6)
            {
                SetAddressValue();
            }
            if (key.Key == ConsoleKey.F7)
            {
                
            }
            if (key.Key == ConsoleKey.F8)
            {
                RemoveBreakPoint();
            }
            if (key.Key == ConsoleKey.F9)
            {
                AddBreakPoint();
            }
            if (key.Key == ConsoleKey.F10)
            {
                cpu.Step();
            }
            if (key.Key == ConsoleKey.F12)
            {
                goto Reset;
            }

            goto WaitForInput;

        Exit:
            cpu.OnError -= Cpu_OnError;
            cpu.OnDataRead -= Cpu_OnDataRead;
            cpu.OnDataWrite -= Cpu_OnDataWrite;
            cpu.OnBreakPoint -= Cpu_OnBreakPoint;

            System.Console.WriteLine("Bye");
        }

        private static void AddBreakPoint()
        {
            System.Console.WriteLine("Current Break Points:");
            foreach (var item in cpu.BreakPoints)
            {
                System.Console.WriteLine($" >{item:X4}");
            }
            System.Console.Write("Enter PC value to break at: >");
            var brkPnt = System.Console.ReadLine();
            if (!string.IsNullOrEmpty(brkPnt))
            {
                var val = int.Parse(brkPnt, System.Globalization.NumberStyles.HexNumber);
                cpu.AddBreakPoint((ushort)val);
            }
        }

        private static void RemoveBreakPoint()
        {
            System.Console.WriteLine("Current Break Points:");
            foreach (var item in cpu.BreakPoints)
            {
                System.Console.WriteLine($" >{item:X4}");
            }
            System.Console.Write("Enter break point value to remove: >");
            var brkPnt = System.Console.ReadLine();

            if (!string.IsNullOrEmpty(brkPnt))
            {
                var bp = UInt16.Parse(brkPnt, System.Globalization.NumberStyles.HexNumber);
                if(cpu.BreakPoints.Contains(bp))
                    cpu.RemoveBreakPoint(bp);
            }
        }

        private static void Cpu_OnBreakPoint(object sender, OutputEventArg e)
        {
            var orgColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine($" >Break on address ${e.Address:X4}");
            System.Console.ForegroundColor = orgColor;
        }

        private static void Cpu_OnDataWrite(object sender, OutputEventArg e)
        {
            var orgColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkMagenta;
            System.Console.WriteLine($" >Writing value {e.Data:X2} to address ${e.Address:X4}");
            System.Console.ForegroundColor = orgColor;
        }

        private static void Cpu_OnDataRead(object sender, OutputEventArg e)
        {
            var orgColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkCyan;
            System.Console.WriteLine($" >Read value {e.Data:X2} from address ${e.Address:X4}");
            System.Console.ForegroundColor = orgColor;
        }

        private static void Cpu_OnInstructionExecuted(object sender, InstructionEventArg e)
        {
            var orgColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"[{cpu.PC:X4}] {e.DisAssembledInstruction}");
            System.Console.ForegroundColor = orgColor;
            if (breakMode)
                e.RunMode = false;
        }



        private static void Cpu_OnError(object sender, ExceptionEventArg e)
        {
            var cc = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"ERROR: {e.ErrorMessage}");
            System.Console.ForegroundColor = cc;
        }

        private static void DisplayMenu()
        {
            var colWidth = 40;
            
            System.Console.Write("F1  = Memory Dump".PadRight(colWidth));
            System.Console.WriteLine("F7  = ");

            System.Console.Write("F2  = Load binary file into RAM".PadRight(colWidth));
            System.Console.WriteLine("F8  = Remove Break Point");

            System.Console.Write("F3  = Display Registers and Status".PadRight(colWidth));
            System.Console.WriteLine("F9  = Add break point");

            System.Console.Write("F4  = Set Program Counter to value".PadRight(colWidth));
            System.Console.WriteLine("F10 = Step");

            System.Console.Write("F5  = Run".PadRight(colWidth));
            System.Console.WriteLine("F12 = Reset");

            System.Console.Write("F6  = Set Memory Address Value".PadRight(colWidth));
            System.Console.WriteLine("X   = Quit".PadRight(colWidth));


            System.Console.WriteLine();
        }

        private static void DisplayMemoryRange()
        {
            System.Console.WriteLine(" Enter range (e.g FFFC, 0000:7FFF, :AEDE)");
            System.Console.Write(" > ");

            var rangeTxt = System.Console.ReadLine();
            if (String.IsNullOrEmpty(rangeTxt))
            {
                return;
            }
            string rangeLow, rangeHi;

            var range = rangeTxt.Split(":");
            if (range.Length == 1)
            {
                rangeLow = range[0];
                rangeHi = rangeLow;
            }
            else if (range.Length == 2)
            {
                rangeLow = range[0];
                rangeHi = range[1];
                if (String.IsNullOrEmpty(rangeLow))
                {
                    rangeLow = "0000";
                }
                if (String.IsNullOrEmpty(rangeHi))
                {
                    rangeHi = rangeLow;
                }
            }
            else
            {
                rangeLow = "0000";
                rangeHi = "FFFF";
            }

            var startByte = int.Parse(rangeLow, System.Globalization.NumberStyles.HexNumber);
            var endByte = int.Parse(rangeHi, System.Globalization.NumberStyles.HexNumber);

            if (endByte < startByte)
            {
                endByte = startByte;
            }
            if (endByte == startByte)
            {
                endByte = startByte + 16;
            }

            System.Console.WriteLine($"Memory Dump of {startByte:X4} - {(endByte - 1):X4}");
            System.Console.WriteLine(" Address  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F");

            for (int index = startByte; index < endByte; index += 16)
            {
                if (index > 0xFFFF)
                    continue;
                System.Console.Write($" {index:X4}:    ");
                for (int x = 0; x < 16; x++)
                {
                    if (index + x > 0xFFFF)
                        continue;
                    System.Console.Write($"{ram.Data[index + x]:X2} ");
                }
                System.Console.WriteLine();
            }

        }

        private static void DisplayRegisters()
        {
            System.Console.WriteLine($" A:[{cpu.A:X2}] X:[{cpu.X:X2}] Y:[{cpu.Y:X2}]  ST:[{((byte)(cpu.ST)).ToBinary()}] SP:[{cpu.SP:X4}] PC:[{cpu.PC:X4}]");
        }

        private static void LoadBinaryFile()
        {
            var path = "C:\\temp\\display.rom";
            System.Console.WriteLine(" Enter full path to binary file: ");
            System.Console.Write($" [{path}] > ");
            var newpath = System.Console.ReadLine();

            if (!string.IsNullOrEmpty(newpath))
                path = newpath;

            var data = System.IO.File.ReadAllBytes(path);
            if (data.Length == 32768)
            {
                for (int index = 0; index < data.Length; index++)
                {
                    ram.Data[0x8000 + index] = data[index];
                }
            }

            cpu.Reset();
        }

        private static void SetAddressValue()
        {
            System.Console.Write("Select Address (FFFF): >");
            var addr = System.Console.ReadLine();

            System.Console.Write("Set Value (FF): >");
            var val = System.Console.ReadLine();

            var memAddr = Int32.Parse(addr, System.Globalization.NumberStyles.HexNumber);
            var memVal = byte.Parse(val, System.Globalization.NumberStyles.HexNumber);

            ram.Data[memAddr] = memVal;

        }

        private static void SetPCValue()
        {
            System.Console.Write("Set PC Value (FF): >");
            var val = System.Console.ReadLine();

            var memAddr = UInt16.Parse(val, System.Globalization.NumberStyles.HexNumber);

            cpu.PC = memAddr;

        }
    }
}
