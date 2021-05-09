using System;
using System.Text.RegularExpressions;
using W65C02S.Bus;
using W65C02S.CPU;
using W65C02S.Engine;
using W65C02S.RAM;
using W65C02S.ROM;

namespace W65C02S.Console
{
    class Program
    {
        private const int maxColumns = 120;
        private const int maxRows = 40;

        private const int clientAreaStart = 8;
        private const int clientAreaHeight = 34;
        private const int clientAreaEnd = clientAreaStart + clientAreaHeight;

        private static int lastInstructionPos = 0;
        private static bool binaryFileLoaded = false;

        private static Emulator emulator;
        private static Bus.Bus bus;
        private static ROM32K rom;
        private static RAM32K ram;

        static void Main(string[] args)
        {
            System.Console.SetWindowSize(maxColumns, maxRows);
            System.Console.SetBufferSize(maxColumns, maxRows);
            System.Console.Title = "W65C02 Emulator";
            System.Console.ForegroundColor = ConsoleColor.Green;

            using (bus = new Bus.Bus())
            {
                emulator = new Emulator(bus);
                rom = new ROM32K(bus, 0x8000);
                ram = new RAM32K(bus, 0);

                bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
                bus.Subscribe<InstructionDisplayEventArg>(OnInstructionExecuted);
                bus.Subscribe< ExceptionEventArg>(OnError);

                System.Console.CursorVisible = false;
                DisplayMainMenu();

                System.Console.Clear();

                bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
                bus.UnSubscribe<InstructionDisplayEventArg>(OnInstructionExecuted);
                bus.UnSubscribe<ExceptionEventArg>(OnError);

                emulator.Dispose();
                bus.Dispose();
            }

            System.Console.WriteLine("Bye!");
        }

        private static void OnInstructionExecuted(InstructionDisplayEventArg e)
        {
            ShowLastInstruction(e.DecodedInstruction, e.RawData);
            DisplayRegisters(e.A, e.X, e.Y, (byte)e.ST, e.SP, e.PC, e.ClockTicks);
        }

        private static void OnError(ExceptionEventArg e)
        {
            DisplayError(e.ErrorMessage);
        }
        private static void DisplayError(string errorMsg)
        {
            var cc = System.Console.ForegroundColor;
            var currLeft = System.Console.CursorLeft;
            var currTop = System.Console.CursorTop;

            System.Console.SetCursorPosition(0, maxRows - 1);
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.Write($"ERROR: {errorMsg}");
            System.Console.ForegroundColor = cc;
            System.Console.SetCursorPosition(currLeft, currTop);
        }
        private static void ClearError()
        {
            var currLeft = System.Console.CursorLeft;
            var currTop = System.Console.CursorTop;
            System.Console.SetCursorPosition(0, maxRows - 1);
            System.Console.Write("".PadRight(100), ' ');
            System.Console.SetCursorPosition(currLeft, currTop);
        }
        private static void OnAddressChanged(AddressBusEventArgs arg)
        {
            UpdateAddress(arg.Address);
        }

        private static void DisplayMainMenu()
        {
            Reset:
            System.Console.Clear();
            var colWidth = 40;
            CreateSubMenuHeading("Main Menu");
            if (binaryFileLoaded)
                System.Console.WriteLine($"    F1 = Load Binary file into ROM " + "\u221A".PadRight(colWidth));
            else
                System.Console.WriteLine($"    F1 = Load Binary file into ROM".PadRight(colWidth));

            System.Console.WriteLine("    F2 = System Monitor".PadRight(colWidth));
            System.Console.WriteLine("    F5 = Start Emulator".PadRight(colWidth));
            //System.Console.WriteLine("   F12 = Reset System".PadRight(colWidth));
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("     X = Quit".PadRight(colWidth));
            System.Console.WriteLine("".PadRight(maxColumns-1, '-'));
            System.Console.SetCursorPosition(0, clientAreaStart);
            
            

            WaitForSelection:
            
            var input = System.Console.ReadKey();

            if(input.Key == ConsoleKey.F1)
            {
                DisplayMenu_LoadROMFile();
                goto Reset;
            }
            if (input.Key == ConsoleKey.F2)
            {
                DisplayMenu_Monitor();
                                goto Reset;
            }
            if (input.Key == ConsoleKey.F5)
            {
                if(!binaryFileLoaded)
                {
                    DisplayError("No Binary file loaded. Please load a binary file in to ROM (F1).");
                }
                else
                {
                    System.Console.CursorVisible = false;
                    emulator.Reset();
                    DisplayMenu_Emulator();
                    System.Console.CursorVisible = true;
                    goto Reset;
                }
            }
            //if (input.Key == ConsoleKey.F12)
            //{
            //    binaryFileLoaded = false;
            //    goto Reset;
            //}
            if (input.Key == ConsoleKey.X)
            {
                return;
            }

            goto WaitForSelection;
        }


        private static void DisplayMenu_LoadROMFile()
        {
            System.Console.Clear();
            CreateSubMenuHeading("Load Binary File");

            System.Console.WriteLine("ROM Address is located at $8000 - $FFFF");
            System.Console.WriteLine("The ROM will be loaded with the binary data from the file selected here.");
            System.Console.WriteLine("Ensure that the file size is exactly 32687 bytes.");
            System.Console.WriteLine("Press Esc to return to  main menu.");
            System.Console.Write("Enter full path to file:> ");
        WaitForInput:
            binaryFileLoaded = false;
            var input = System.Console.ReadKey();
            if (input.Key == ConsoleKey.Escape || input.Key == ConsoleKey.Enter)
                return;
            var capturedChar = input.KeyChar;
            
            var filePath = System.Console.ReadLine();
            if (string.IsNullOrEmpty(filePath))
                return;

            filePath = capturedChar + filePath;
            //// debug
            if (filePath == "xxx")
                filePath = "C:\\temp\\display.rom";
            //// debug
            if (!System.IO.File.Exists(filePath))
            {
                System.Console.WriteLine($"Cannot find the file '{filePath}'");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }

            var data = System.IO.File.ReadAllBytes(filePath);

            if (data.Length < 32768)
            {
                System.Console.WriteLine($"Binary file is too small. ROM size is 32768 bytes, and this file is only {data.Length} bytes");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }
            if (data.Length > 32768)
            {
                System.Console.WriteLine($"Binary file is too big. ROM size is 32768 bytes, and this file is {data.Length} bytes");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }

            if (data.Length == 32768)
            {
                emulator.LoadROM(data);
                binaryFileLoaded = true;
                System.Console.WriteLine($"Loaded {data.Length} bytes into ROM....");
                System.Console.Write("Press Enter to return to main menu :>");
                System.Console.ReadLine();
            }
        }

        private static void DisplayMenu_Monitor()
        {
            System.Console.Clear();
            CreateSubMenuHeading("System Monitor");

            System.Console.WriteLine("    F1 = View Memory Location");
            System.Console.WriteLine("    F2 = View Memory Row");
            System.Console.WriteLine("    F3 = View Memory Page");
            System.Console.WriteLine("    F4 = Edit Memory Location");
            System.Console.WriteLine("   Esc = Return to main menu");
            System.Console.WriteLine();
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));
            ReStart:

        WaitForInput:
            var input = System.Console.ReadKey();
            if (input.Key == ConsoleKey.Escape || input.Key == ConsoleKey.Enter)
                return;

            if (input.Key == ConsoleKey.F1)
            {
                ClearClientArea(true);
                Monitor_DisplayLocation();
                goto ReStart;
            }
            if (input.Key == ConsoleKey.F2)
            {
                ClearClientArea(true);
                Monitor_DisplayRow(0);
                goto ReStart;
            }
            if (input.Key == ConsoleKey.F3)
            {
                ClearClientArea(true);
                Monitor_DisplayPage();
                goto ReStart;
            }
            if (input.Key == ConsoleKey.F4)
            {
                ClearClientArea(true);
                Monitor_EditLocation();
                goto ReStart;
            }
            if (input.Key == ConsoleKey.Escape)
            {
                return;
            }

            goto WaitForInput;

        }
        private static void Monitor_DisplayHeader(int left = 0, int topOffset = 0)
        {
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset);
            System.Console.Write("Address | -0 -1 -2 -3 -4 -5 -6 -7 -8 -9 -A -B -C -D -E -F");
            System.Console.CursorLeft = left;
            System.Console.CursorTop++;
            System.Console.Write("--------|-------------------------------------------------");
            System.Console.CursorTop++;

        }
        private static void Monitor_DisplayLocation(int left = 0, int topOffSet = 0)
        {
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "View address location [AFB9]: >$";
        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, clientAreaStart + topOffSet);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, clientAreaStart + topOffSet);
            var input = System.Console.ReadLine();
            System.Console.CursorVisible = false;
            if (String.IsNullOrEmpty(input))
            {
                return;
            }
            
            if( !Regex.IsMatch(input, inputRegex))
            {
                DisplayError("Invalid address. Requires 4 Hex digits.");
                goto Start;
            }
            var orgLocation = input;
            input = input.Substring(0, 3) + "0";

            ClearError();
            Monitor_DisplayHeader(left, topOffSet);
            var location = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
            var requestedLoc = int.Parse(orgLocation, System.Globalization.NumberStyles.HexNumber);

            System.Console.CursorLeft = left;
            System.Console.Write($"${location:X4}   : ");
            for (ushort index = location; index < location + 16; index++)
            {
                if(index == requestedLoc)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.Write($"{emulator.ReadMemoryLocation(index):X2} ");
                    System.Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    System.Console.Write($"{emulator.ReadMemoryLocation(index):X2} ");
            }
        }
        private static void Monitor_DisplayRow(int left = 0, int topOffset = 0)
        {
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "View address row [AFB9]: >$";
        Start:
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, clientAreaStart + topOffset);
            var input = System.Console.ReadLine();
            if (String.IsNullOrEmpty(input))
            {
                return;
            }

            if (!Regex.IsMatch(input, inputRegex))
            {
                DisplayError("Invalid address. Requires 4 Hex digits.");
                goto Start;
            }
            input = input.Substring(0, 3) + "0";

            ClearError();
            Monitor_DisplayHeader(left, topOffset);
            var location = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
            System.Console.CursorLeft = left;
            System.Console.Write($"${location:X4}   : ");
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            for (ushort index = location; index < location + 16; index++)
            {
                System.Console.Write($"{emulator.ReadMemoryLocation(index):X2} ");
            }
            System.Console.ForegroundColor = ConsoleColor.Green;
        }
        private static void Monitor_DisplayPage(int left = 0, int topOffset = 0)
        {
            var inputRegex = "^[0-9A-Fa-f]{2}$";
            var inputDisplay = "View page [AF]: >$";
        Start:
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, clientAreaStart + topOffset);
            
            var input = System.Console.ReadLine();
            if (String.IsNullOrEmpty(input))
            {
                return;
            }

            if (!Regex.IsMatch(input, inputRegex))
            {
                DisplayError("Invalid page number. Requires 2 Hex digits (e.g. A2, FD).");
                goto Start;
            }

            ClearError();
            Monitor_DisplayHeader(left, topOffset);
            input = input + "00";

            var location = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
            System.Console.CursorLeft = left;
            for ( ushort index = location ; index < location + 256; index += 16)
            {
                System.Console.Write($"${index:X4}   : ");
                System.Console.ForegroundColor = ConsoleColor.Yellow;

                for (ushort col = index; col < (index + 16); col++)
                {
                    System.Console.Write($"{emulator.ReadMemoryLocation(col):X2} ");
                }

                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.CursorTop++;
                System.Console.CursorLeft = left;
            }
        }
        private static void Monitor_EditLocation(int left = 0, int topOffSet = 0)
        {
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "Edit address location [AFB9]: >$";
        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, clientAreaStart + topOffSet);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, clientAreaStart + topOffSet);
            var input = System.Console.ReadLine();
            System.Console.CursorVisible = false;
            if (String.IsNullOrEmpty(input))
            {
                return;
            }

            if (!Regex.IsMatch(input, inputRegex))
            {
                DisplayError("Invalid address. Requires 4 Hex digits.");
                goto Start;
            }
            var orgLocation = input;
            input = input.Substring(0, 3) + "0";

            ClearError();
            Monitor_DisplayHeader(left, topOffSet);
            var location = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
            var requestedLoc = ushort.Parse(orgLocation, System.Globalization.NumberStyles.HexNumber);

            System.Console.CursorLeft = left;
            System.Console.Write($"${location:X4}   : ");
            var editIndex = 0;
            for (ushort index = location; index < location + 16; index++)
            {
                if (index == requestedLoc)
                {
                    editIndex = System.Console.CursorLeft;
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.Write($"{emulator.ReadMemoryLocation(index):X2} ");
                    System.Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    System.Console.Write($"{emulator.ReadMemoryLocation(index):X2} ");
            }


        TryAgain:
            
            //System.Console.CursorLeft = editIndex;
            //System.Console.Write("  ");
            System.Console.CursorLeft = editIndex;

            System.Console.CursorVisible = true;
            var valRegex = "^[0-9A-Fa-f]{2}$";
            var val = System.Console.ReadLine();
            if (String.IsNullOrEmpty(val))
            {
                return;
            }

            if (!Regex.IsMatch(val, valRegex))
            {
                DisplayError("Invalid address. Requires 2 Hex digits.");
                System.Console.CursorTop--;
                goto TryAgain;
            }
            ClearError();


            System.Console.CursorVisible = false;
            var newVal = byte.Parse(val, System.Globalization.NumberStyles.HexNumber);
            emulator.WriteMemoryLocation(requestedLoc, newVal);
        }

        private static void DisplayMenu_Emulator()
        {
            var leftCol = 0;
            var rightCol = maxColumns / 2;

            System.Console.Clear();
            CreateSubMenuHeading("Emulator");

            System.Console.SetCursorPosition(leftCol, 1); System.Console.Write("    F5 = Run / Break");
            System.Console.SetCursorPosition(leftCol, 2); System.Console.Write("    F9 = Add Breakpoint");
            System.Console.SetCursorPosition(leftCol, 3); System.Console.Write("   F10 = Step next Instruction");
            System.Console.SetCursorPosition(leftCol ,4); System.Console.Write("   F12 = Reset CPU");
            System.Console.SetCursorPosition(leftCol, 5); System.Console.Write("   Esc = Return to main menu");

            System.Console.SetCursorPosition(rightCol, 1); System.Console.Write("F1 = View Memory Location");
            System.Console.SetCursorPosition(rightCol, 2); System.Console.Write("F2 = View Memory Row");
            System.Console.SetCursorPosition(rightCol, 3); System.Console.Write("F3 = View Memory Page");
            System.Console.SetCursorPosition(rightCol, 4); System.Console.Write("F4 = Edit Memory Location");

        Reset:
            System.Console.SetCursorPosition(leftCol, clientAreaStart-1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(leftCol, clientAreaStart);
            System.Console.Write("Address:$---- | A:$-- X:$-- Y:$-- | SP:$---- | ST:-------- | PC: $---- |                        Clock Ticks:------------");
            System.Console.SetCursorPosition(leftCol, clientAreaStart + 1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(0, clientAreaStart+1);
            lastInstructionPos = clientAreaStart+2;

        WaitForInput:
            var input = System.Console.ReadKey();
            if (input.Key == ConsoleKey.Escape || input.Key == ConsoleKey.Enter)
                return;
            
            if (input.Key == ConsoleKey.F1)
            {
                ClearClientArea(false, 2);
                System.Console.CursorVisible = true;
                Monitor_DisplayLocation(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F2)
            {
                ClearClientArea(false, 2);
                System.Console.CursorVisible = true;
                Monitor_DisplayRow(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F3)
            {
                System.Console.CursorVisible = true;
                ClearClientArea(false, 2);
                Monitor_DisplayPage(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F4)
            {
                ClearClientArea(false, 2);
                System.Console.CursorVisible = true;
                Monitor_EditLocation(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F5)
            {
                if (emulator.Mode == RunMode.Debug)
                    emulator.Run();
                else
                    emulator.Mode = RunMode.Debug;

                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F9)
            {
                
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F10)
            {
                emulator.Step();
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F11)
            {
                
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F12)
            {
                emulator.Reset();
                ClearClientArea(true);
                goto Reset;
            }

            if (input.Key == ConsoleKey.Escape)
            {
                return;
            }

            goto WaitForInput;
        }

        private static void CreateSubMenuHeading(string headingText)
        {
            System.Console.Write("".PadRight(((maxColumns-1) / 2) - (headingText.Length / 2), '-'));
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write(headingText);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("".PadRight(((maxColumns - 1) / 2) - (headingText.Length / 2), '-'));
        }

        private static void DisplayRegisters(byte A, byte X, byte Y, byte ST, ushort SP, ushort PC, double clockTicks)
        {
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.SetCursorPosition(0, clientAreaStart);

            System.Console.CursorLeft = 19; System.Console.Write($"{A:X2}");
            System.Console.CursorLeft = 25; System.Console.Write($"{X:X2}");
            System.Console.CursorLeft = 31; System.Console.Write($"{Y:X2}");
            
            System.Console.CursorLeft = 40; System.Console.Write($"{SP:X4}");
            
            System.Console.CursorLeft = 50;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.N) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"N");

            System.Console.CursorLeft = 51;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.V) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"V");

            System.Console.CursorLeft = 52;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write($"1");

            System.Console.CursorLeft = 53;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.B) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"B");

            System.Console.CursorLeft = 54;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.D) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"D");

            System.Console.CursorLeft = 55;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.I) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"I");

            System.Console.CursorLeft = 56;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.Z) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"Z");

            System.Console.CursorLeft = 57;
            System.Console.ForegroundColor = emulator.IsFlagSet(ProcessorFlags.C) ? ConsoleColor.Yellow : ConsoleColor.DarkYellow;
            System.Console.Write($"C");

            System.Console.CursorLeft = 66; System.Console.Write($"{PC:X4}");
            System.Console.CursorLeft = 108;System.Console.Write($"{clockTicks}".PadLeft(12, ' '));

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.SetCursorPosition(0, clientAreaStart + 1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(curLeft, curTop);
        }

        private static void UpdateAddress(ushort address)
        {
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.SetCursorPosition(9, clientAreaStart);
            System.Console.Write($"{address:X2}");
            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = ConsoleColor.Green;
        }

        private static void ShowLastInstruction(string instruction, string rawData)
        {
            if (lastInstructionPos > clientAreaHeight)
            {
                var sourceLeft = 0;
                var sourceTop = clientAreaStart + 3;
                var sourceWidth = (maxColumns / 2) - 1;
                var sourceHeight = clientAreaHeight - sourceTop + 1;
                var targetLeft = 0;
                var targetTop = clientAreaStart + 2;

                System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
                lastInstructionPos = clientAreaHeight;
            }

            System.Console.SetCursorPosition(0, lastInstructionPos);
            var txt = $" [{rawData.PadRight(8, ' ')}] {instruction}";
            if (txt.Length > (maxColumns / 2))
                txt = txt.Substring(0, (maxColumns / 2));
            System.Console.WriteLine(txt);
            lastInstructionPos++;
        }

        private static void ClearClientArea(bool left, int topOffset = 0)
        {
            var curLeft = left == true ? 0 : (maxColumns / 2);
            
            for (int index = (clientAreaStart + topOffset); index < clientAreaHeight; index++)
            {
                System.Console.CursorLeft = curLeft;
                System.Console.CursorTop = index;
                System.Console.WriteLine("".PadRight(maxColumns / 2, ' '));

            }

        }

    }
}
