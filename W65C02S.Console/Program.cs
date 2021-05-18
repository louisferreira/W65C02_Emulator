using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using W65C02S.Bus;
using W65C02S.CPU;
using W65C02S.Engine;
using W65C02S.InputOutput.Devices;
using W65C02S.MemoryMappedDevice;
using W65C02S.RAM;
using W65C02S.ROM;

namespace W65C02S.Console
{
    class Program
    {
        private static Process p;
        private const int maxColumns = 120;
        private const int maxRows = 40;

        private const int clientAreaStart = 8;
        private const int clientAreaHeight = 34;
        
        private static int lastInstructionPos = 0;
        private static ushort currentStackPointer = 0;

        private static bool binaryFileLoaded = false;
        private static bool emulatorStarted = false;
        private static bool showDeviceActivity = false;

        private static Emulator emulator;
        private static Bus.Bus bus;
        private static ROM.ROM rom;
        private static RAM.RAM ram;
        private static W6522_Via ioDevice;
        private static string lastROMFileLoaded = string.Empty;

        static void Main(string[] args)
        {
            System.Console.SetWindowSize(maxColumns, maxRows);
            System.Console.SetBufferSize(maxColumns, maxRows);
            System.Console.Title = "W65C02 Emulator";
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.CancelKeyPress += OnCancelKeyPress;
            using (bus = new Bus.Bus())
            {
                emulator = new Emulator(bus);
                //ram = new RAM.RAM(bus, 0, 0x7FFF, DataBusMode.ReadWrite);
                //ioDevice = new W6522_Via(bus, 0x8000, 0x8FFF, DataBusMode.ReadWrite);
                //rom = new ROM.ROM(bus, 0x9000, 0xFFFF, DataBusMode.Read);
                rom = new ROM.ROM(bus, 0x0000, 0xFFFF, DataBusMode.ReadWrite);

                bus.Subscribe<AddressBusEventArgs>(OnAddressChanged);
                bus.Subscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
                bus.Subscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
                bus.Subscribe< ExceptionEventArg>(OnError);

                System.Console.CursorVisible = false;
                DisplayMainMenu();

                System.Console.Clear();

                bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
                bus.UnSubscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
                bus.UnSubscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
                bus.UnSubscribe<ExceptionEventArg>(OnError);

                emulator.Dispose();
                bus.Dispose();
            }

            if (p != null)
                p.Kill(true);

            System.Console.WriteLine("Bye!");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            emulator.Mode = RunMode.Debug;
            showDeviceActivity = true;
            e.Cancel = true;
        }


        private static void OnInstructionExecuting(OnInstructionExecutingEventArg e)
        {
            ShowLastInstruction(e.DecodedInstruction, e.RawData, e.PC);
            //DisplayRegisters(e.A, e.X, e.Y, (byte)e.ST, e.SP, e.PC, e.ClockTicks);
        }
        private static void OnInstructionExecuted(OnInstructionExecutedEventArg e)
        {
            //ShowLastInstruction(e.DecodedInstruction, e.RawData);
            DisplayRegisters(e.A, e.X, e.Y, (byte)e.ST, e.SP, e.PC, e.ClockTicks);
        }

        private static void OnError(ExceptionEventArg e)
        {
            DisplayError(e.ErrorMessage, e.ExceptionType);
        }
        private static void DisplayError(string errorMsg, ExceptionType exceptionType = ExceptionType.Error)
        {
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            var curreColor = System.Console.ForegroundColor;


            var sourceLeft = 0;
            var sourceTop = 36;
            var sourceWidth = maxColumns - 0;
            var sourceHeight = 4;
            var targetLeft = 2;
            var targetTop = 35;

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);

            System.Console.SetCursorPosition(0, maxRows - 1);

            if (exceptionType == ExceptionType.Error)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.Write($"ERROR: {errorMsg}".PadRight(100, ' '));
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                System.Console.Write($"WARNING: {errorMsg}".PadRight(100, ' '));
            }
            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = curreColor;
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
            UpdateAddress(arg);
            if (showDeviceActivity)
                UpdateDeviceActivity(arg);
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

            System.Console.WriteLine("    F2 = Memory Monitor".PadRight(colWidth));
            System.Console.WriteLine("    F5 = Start Emulator".PadRight(colWidth));
            System.Console.WriteLine("    F8 = OpCode Viewer Application".PadRight(colWidth));
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
                    emulatorStarted = false;
                    goto Reset;
                }
            }
            if (input.Key == ConsoleKey.F8)
            {
                ProcessStartInfo startinfo = new ProcessStartInfo(".\\OpCodeViewer.exe");
                startinfo.CreateNoWindow = true;
                startinfo.UseShellExecute = true;
                p = Process.Start(startinfo);
            }
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

            System.Console.WriteLine($"    ROM Address: ${rom.StartAddress:X4} - ${rom.EndAddress:X4}");
            System.Console.WriteLine($"    ROM Size:     {rom.EndAddress - rom.StartAddress} bytes");
            System.Console.WriteLine("    Ensure that the file size is same size in bytes.");
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("    Press Esc to return to  main menu.");
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));
            if(string.IsNullOrEmpty(lastROMFileLoaded))
                System.Console.Write("Enter full path to file:> ");
            else
                System.Console.Write($"Enter full path to file: [{lastROMFileLoaded}]> ");

            WaitForInput:
            binaryFileLoaded = false;
            System.Console.CursorVisible = true;
            var input = System.Console.ReadKey();
            if (input.Key == ConsoleKey.Escape)
            {
                System.Console.CursorVisible = false;
                return;
            }
            if(input.Key == ConsoleKey.Enter && string.IsNullOrEmpty(lastROMFileLoaded))
            {
                System.Console.CursorVisible = false;
                return;
            }

            string filePath = "";
            char? capturedChar = default;
            if ( !string.IsNullOrEmpty(lastROMFileLoaded))
            {
                capturedChar = null;
                filePath = lastROMFileLoaded;
            }
            else
            {
                capturedChar = input.KeyChar;
                filePath = System.Console.ReadLine();
            }
            
            System.Console.CursorVisible = false;
            if (string.IsNullOrEmpty(filePath))
                return;

            filePath = $"{capturedChar}{filePath}";
            //// debug
            if (filePath == "xxx")
                filePath = "C:\\temp\\65c02_extended_opcodes_test.bin";
            //// debug
            if (!System.IO.File.Exists(filePath))
            {
                System.Console.WriteLine($"Cannot find the file '{filePath}'");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }

            var data = System.IO.File.ReadAllBytes(filePath);


            if (data.Length > rom.EndAddress)
            {
                System.Console.WriteLine($"Binary file is too big. ROM size is 32768 bytes, and this file is {data.Length} bytes");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }

            emulator.LoadROM(data);
            binaryFileLoaded = true;
            lastROMFileLoaded = filePath;
            System.Console.WriteLine($"Loaded {data.Length} bytes into ROM....");
            System.Console.Write("Press Enter to return to main menu :>");
            System.Console.ReadLine();
        }

        private static void DisplayMenu_Monitor()
        {
            System.Console.Clear();
            CreateSubMenuHeading("Memory Monitor");

            System.Console.WriteLine("    F1 = View Memory Location");
            System.Console.WriteLine("    F2 = View Memory Page");
            System.Console.WriteLine("    F3 = Edit Memory Location");
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("   Esc = Return to main menu");
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
                Monitor_DisplayPage();
                goto ReStart;
            }
            if (input.Key == ConsoleKey.F3)
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
            var inputRegex = "^[0-9A-Fa-f]{2,4}$";
            var inputDisplay = "View address location [FA],[AFB9]: >$";
            ClearClientArea(false, 2);
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
                DisplayError("Invalid address. Requires 2 or 4 Hex digits.");
                goto Start;
            }
            var orgLocation = input;
            
            if(input.Length > 2)
                input = input.Substring(0, 3) + "0";
            else
                input = input.Substring(0, 1) + "0";

            ClearError();
            Monitor_DisplayHeader(left, topOffSet);
            var location = int.Parse(input, System.Globalization.NumberStyles.HexNumber);
            var requestedLoc = int.Parse(orgLocation, System.Globalization.NumberStyles.HexNumber);

            System.Console.CursorLeft = left;
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write($"${location:X4}   : ");
            for (int index = location; index < location + 16; index++)
            {
                if(index == requestedLoc)
                {
                    System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                    System.Console.Write($"{emulator.ReadMemoryLocation((ushort)index):X2} ");
                    System.Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write($"{emulator.ReadMemoryLocation((ushort)index):X2} ");
                }
            }
        }
        
        private static void Monitor_DisplayPage(int left = 0, int topOffset = 0)
        {
            var inputRegex = "^[0-9A-Fa-f]{2}$";
            var inputDisplay = "View page [AF]: >$";
            ClearClientArea(false, 2);
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
            for ( int index = location ; index < location + 256; index += 16)
            {
                System.Console.Write($"${index:X4}   : ");

                for (int col = index; col < (index + 16); col++)
                {
                    if (col == currentStackPointer)
                        System.Console.ForegroundColor = ConsoleColor.White;
                    else
                        System.Console.ForegroundColor = ConsoleColor.Green;

                    System.Console.Write($"{emulator.ReadMemoryLocation((ushort)col):X2} ");
                }
                
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.CursorTop++;
                System.Console.CursorLeft = left;
            }
        }
        private static void Monitor_EditLocation(int left = 0, int topOffSet = 0)
        {
            var inputRegex = "^[0-9A-Fa-f]{2,4}$";
            var inputDisplay = "Edit address location [EA],[AFB9]: >$";
            ClearClientArea(false, 2);
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
                DisplayError("Invalid address. Requires 2 or 4 Hex digits.");
                goto Start;
            }
            var orgLocation = input;
            if (input.Length > 2)
                input = input.Substring(0, 3) + "0";
            else
                input = input.Substring(0, 1) + "0";

            ClearError();
            Monitor_DisplayHeader(left, topOffSet);
            var location = int.Parse(input, System.Globalization.NumberStyles.HexNumber);
            var requestedLoc = int.Parse(orgLocation, System.Globalization.NumberStyles.HexNumber);

            System.Console.CursorLeft = left;
            System.Console.Write($"${location:X4}   : ");
            var editIndex = 0;
            for (int index = location; index < location + 16; index++)
            {
                if (index == requestedLoc)
                {
                    editIndex = System.Console.CursorLeft;
                    System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                    System.Console.Write($"{emulator.ReadMemoryLocation((ushort)index):X2} ");
                    System.Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.Write($"{emulator.ReadMemoryLocation((ushort)index):X2} ");
                }
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
            emulator.WriteMemoryLocation((ushort)requestedLoc, newVal);
        }

        private static void DisplayMenu_Emulator()
        {
            var leftCol = 0;
            var rightCol = maxColumns / 2;
            
            System.Console.Clear();
            CreateSubMenuHeading("Emulator");

            System.Console.SetCursorPosition(leftCol, 1); System.Console.Write("    F5 = Run (Ctrl+Break to break into Debug)");
            System.Console.SetCursorPosition(leftCol, 2); System.Console.Write("    F9 = Add/Remove Breakpoint");
            System.Console.SetCursorPosition(leftCol, 3); System.Console.Write("   F10 = Step next Instruction");
            System.Console.SetCursorPosition(leftCol, 4); System.Console.Write("    F8 = Set Program Counter Value");
            System.Console.SetCursorPosition(leftCol ,5); System.Console.Write("   F12 = Reset CPU");
            System.Console.SetCursorPosition(leftCol, 6); System.Console.Write("   Esc = Return to main menu");

            System.Console.SetCursorPosition(rightCol, 1); System.Console.Write("F1 = View Memory Location");
            System.Console.SetCursorPosition(rightCol, 2); System.Console.Write("F2 = View Memory Page");
            System.Console.SetCursorPosition(rightCol, 3); System.Console.Write("F3 = Edit Memory Location");
            System.Console.SetCursorPosition(rightCol, 4); System.Console.Write("F6 = Send IRQ Signal");
            System.Console.SetCursorPosition(rightCol, 5); System.Console.Write("F7 = Send NMI Signal");

        Reset:
            System.Console.SetCursorPosition(leftCol, clientAreaStart-1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(leftCol, clientAreaStart);
            System.Console.Write("Address:$---- | A:$-- X:$-- Y:$-- | SP:$---- | ST:-------- | PC: $---- |                        Clock Ticks:------------");
            System.Console.SetCursorPosition(leftCol, clientAreaStart + 1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(0, clientAreaStart+1);
            lastInstructionPos = clientAreaStart+2;
            emulatorStarted = true;

        WaitForInput:
            showDeviceActivity = true;
            var input = System.Console.ReadKey();
            if (input.Key == ConsoleKey.Escape || input.Key == ConsoleKey.Enter)
                return;
            
            if (input.Key == ConsoleKey.F1)
            {
                showDeviceActivity = false;
                //ClearClientArea(false, 2);
                System.Console.CursorVisible = true;
                Monitor_DisplayLocation(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F2)
            {
                showDeviceActivity = false;
                System.Console.CursorVisible = true;
                Monitor_DisplayPage(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F3)
            {
                showDeviceActivity = false;
                System.Console.CursorVisible = true;
                Monitor_EditLocation(maxColumns / 2, 2);
                System.Console.CursorVisible = false;
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F5)
            {
                showDeviceActivity = false;
                emulator.Run();

                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F6)
            {
                emulator.SendIRQ();
            }
            if (input.Key == ConsoleKey.F7)
            {
                emulator.SendNMI();
            }
            if (input.Key == ConsoleKey.F8)
            {
                EditPCValue();
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F9)
            {
                DisplayBreakPoints();
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F10)
            {
                ClearActivityArea();
                emulator.Step();
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F11)
            {
                
                goto WaitForInput;
            }
            if (input.Key == ConsoleKey.F12)
            {
                showDeviceActivity = false;
                emulator.Reset();
                ClearClientArea(true, -1);
                ClearClientArea(false, -1);
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
            currentStackPointer = SP;

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

        private static void UpdateAddress(AddressBusEventArgs sender)
        {
            if ( !emulatorStarted)
                return;
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            var curreColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.SetCursorPosition(9, clientAreaStart);
            System.Console.Write($"{sender.Address:X2}");
            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = curreColor;
        }


        private static void DisplayBreakPoints()
        {
            var left = maxColumns / 2;
            var topOffset = 2;
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "Add / Remove PC Address [AFB9]: >$";

            ClearClientArea(false, 2);

            // display list
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset + 1);
            System.Console.WriteLine("Current Breakpoints:".PadRight(25, ' '));

            foreach (var bp in emulator.GetBreakPoints())
            {
                System.Console.CursorLeft = left;
                System.Console.WriteLine($"[${bp:X4}]");
            }
            

        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, clientAreaStart + topOffset);
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

            var inputValue = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
            emulator.AddRemoveBreakPoint(inputValue);

            // display new list
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset + 1);
            System.Console.WriteLine("Current Breakpoints:".PadRight(25, ' '));

            foreach (var bp in emulator.GetBreakPoints())
            {
                System.Console.CursorLeft = left;
                System.Console.WriteLine($"[${bp:X4}]");
            }
            System.Console.CursorLeft = left;
            System.Console.WriteLine($"".PadRight((maxColumns / 2) - 2, ' ')); // clear the last entry off the screen

        }

        private static void EditPCValue()
        {
            var left = maxColumns / 2;
            var topOffset = 2;
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "Set PC Address [AFB9]: >$";

            ClearClientArea(false, 2);
        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, clientAreaStart + topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, clientAreaStart + topOffset);
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

            var inputValue = ushort.Parse(input, System.Globalization.NumberStyles.HexNumber);
            emulator.SetPCValue(inputValue);
        }

        private static void UpdateDeviceActivity(AddressBusEventArgs sender)
        {
            if (!emulatorStarted)
                return;
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            var curreColor = System.Console.ForegroundColor;


            var sourceLeft = 2;
            var sourceTop = 36;
            var sourceWidth = maxColumns  - 2;
            var sourceHeight = 4;
            var targetLeft = 2;
            var targetTop = 35;

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);

            System.Console.SetCursorPosition(0, maxRows - 1);
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            if(sender.Mode == DataBusMode.Read)
                System.Console.Write($" >Read value ${sender.Data:X2} from address ${sender.Address:X4} on device '{sender.DeviceName}'".PadRight(maxColumns - 1, ' '));
            else
                System.Console.Write($" >Wrote value ${sender.Data:X2} to address ${sender.Address:X4} on device '{sender.DeviceName}'".PadRight(maxColumns - 1, ' '));

            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = curreColor;

            
        }

        private static void ShowLastInstruction(string instruction, string rawData, ushort PC)
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
            var txt = $" ${PC:X4}: [{rawData.PadRight(8, ' ')}] {instruction}";
            if (txt.Length > (maxColumns / 2))
                txt = txt.Substring(0, (maxColumns / 2));

            System.Console.WriteLine(txt);
            lastInstructionPos++;

        }

        private static void ClearClientArea(bool left, int topOffset = 0)
        {
            var curLeft = left == true ? 0 : (maxColumns / 2);
            var currTop = System.Console.CursorTop;
            for (int index = (clientAreaStart + topOffset); index <= clientAreaHeight; index++)
            {
                System.Console.CursorLeft = curLeft;
                System.Console.CursorTop = index;
                System.Console.Write("".PadRight(maxColumns / 2, ' '));

            }
            System.Console.CursorTop = currTop;
        }
        private static void ClearActivityArea()
        {
            var currTop = System.Console.CursorTop;
            for (int index = (maxRows - 4); index < maxRows; index++)
            {
                System.Console.CursorLeft = 0;
                System.Console.CursorTop = index;
                System.Console.Write("".PadRight(maxColumns - 1, ' '));
            }
            System.Console.CursorTop = currTop;
        }
    }
}
