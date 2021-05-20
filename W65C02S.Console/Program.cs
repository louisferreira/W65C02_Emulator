using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using W65C02S.Bus;
using W65C02S.CPU;
using W65C02S.Engine;
using W65C02S.InputOutput.Devices;
using System.Collections.Generic;
using System.Linq;

namespace W65C02S.Console
{
    class Program
    {
        private static Process p;
        private const int maxColumns = 120;
        private const int maxRows = 40;

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
        private static MenuCollection mainMenu;
        private static MenuCollection subMenu;

        static void Main(string[] args)
        {
            SetupMenuStructure();

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
                bus.Subscribe<ExceptionEventArg>(OnError);

                System.Console.CursorVisible = false;
                try
                {
                    DisplayMainMenu();
                }
                catch (Exception ex)
                {
                    DisplayError(ex.Message, ExceptionType.Error);
                    System.Console.ReadKey();
                }
                finally
                {
                    System.Console.Clear();

                    bus.UnSubscribe<AddressBusEventArgs>(OnAddressChanged);
                    bus.UnSubscribe<OnInstructionExecutingEventArg>(OnInstructionExecuting);
                    bus.UnSubscribe<OnInstructionExecutedEventArg>(OnInstructionExecuted);
                    bus.UnSubscribe<ExceptionEventArg>(OnError);

                    emulator.Dispose();
                    bus.Dispose();
                }
                
            }

            if (p != null)
                p.Kill(true);

            System.Console.WriteLine("Bye!");
        }

        #region Main Menu
        private static void DisplayMainMenu()
        {
        Reset:
            System.Console.Clear();
            var row = 1;
            var right = System.Console.BufferWidth / 2;
            CreateSubMenuHeading("Main Menu");
            CreateAsciiArt();
            foreach (var menu in mainMenu.Items)
            {
                if (menu.Hidden)
                    continue;

                if (row > mainMenu.NumberOfLines - 1)
                {
                    System.Console.CursorTop = row - mainMenu.NumberOfLines + 1;
                    System.Console.CursorLeft = right;
                }
                else
                {
                    System.Console.CursorTop = row;
                    System.Console.CursorLeft = 0;
                }
                row++;

                System.Console.Write($" {menu.ShortcutKey.ToString().PadLeft(6, ' ')} - {menu.Text}");
            }

            System.Console.CursorTop = mainMenu.NumberOfLines + 1;
            System.Console.CursorLeft = 0;
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));

        WaitForSelection:

            var input = System.Console.ReadKey();

            var selected = mainMenu.Items.FirstOrDefault(x => x.ShortcutKey == input.Key);
            if (selected != null)
            {
                if (selected.ShortcutKey == ConsoleKey.Escape)
                    return;
                subMenu = selected.ChildMenuItems;
                selected.MenuAction(0, 0);
                goto Reset;
            }
            goto WaitForSelection;
        }

        private static void CreateAsciiArt()
        {
            var art = @"
                                                                       &@@/                  
                                                               #%@@@@@@@@@@@&                
                                                       ./@@@@@@@@@@@@@@@@@@@@@@/             
                                                ,&@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@           
                                         .@@@@@@@@&* ,/&&@@@@@@@@@@@@@@@@@@@@@@@@@@@.        
                                  @@@@@@@@@@@@@.           @@@@@@@@@@@@@@@@@@@@@@@@@@@       
                           #@@@@@@@@@@@@@@@@@@              %@@@@@@@@@@@@@@@@@@@@@@@@(./     
                   /#@@@@@@@@@@@@@@@@@@@@@@@@@@             &@@@@@@@@@@@@@@@@@/, ,#@@@@@     
                @%@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@,        ,@@@@@@@@@@@&#* ,(%@@@@@@@* /&     
                @,,@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@&  .,@@@@@@@@&@@  %&* /      
                #*% &@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@(  ,&@@@@@@@@, #@( ,&@  % * /      
                ,,*(,#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@/  .#@@@@@@@. @@  @,, # ( ,  &.@  /(      
                @%*/., @@@@@@@@@@@@@@@@@@@@@*. *%@@@@@@&/@@  @@  #,  @ , & #(&   .,  /(      
                   %*%%.,@@@@@@@@@@@%(, *#&@@@@@@@* (@# .@@  *(  #.@ (  **  #%   .,  *,      
                    ,@(.@ @@@@%  .*@@@@@@@&%@@  &%* / # . @*%* (/  .    **  #%               
                      .@ &&(@@@@@@&@@. #@( *#@  & * ( ,#&  &%  (/  .                         
                        @@ &@  @@  @.. # ( * .@ @  /(  %&  &%  *#                            
                          #@@  #*  @ . @ #(%    .  /(  %&                                    
                            @  #.@./  ,*  ##    .  ,,                                        
                             (/  ,.   ,*  ##                                                 
                             (/  ,.                                                          
                             *#              Developed by L. Ferreira
";
            
            System.Console.CursorLeft = 0;
            System.Console.CursorTop = mainMenu.NumberOfLines + 4;
            System.Console.Write(art);
        } 

        #endregion

        #region Load ROM
        private static void DisplayLoadROMFile(int left = 0, int topOffset = 0)
        {
            System.Console.Clear();
            CreateSubMenuHeading("Load Binary File");

            System.Console.WriteLine($"     ROM Address: ${rom.StartAddress:X4} - ${rom.EndAddress:X4}");
            System.Console.WriteLine($"     ROM Size:     {rom.EndAddress - rom.StartAddress} bytes");
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine("    Press Esc to return to  main menu.");
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));
            System.Console.Write("Enter full path to file:> ");

        WaitForInput:
            binaryFileLoaded = false;
            System.Console.CursorVisible = true;
            var input = System.Console.ReadKey();
            if (input.Key == ConsoleKey.Escape)
            {
                System.Console.CursorVisible = false;
                return;
            }
            if (input.Key == ConsoleKey.Enter && string.IsNullOrEmpty(lastROMFileLoaded))
            {
                System.Console.CursorVisible = false;
                return;
            }

            string filePath = "";
            char? capturedChar = default;
            if (!string.IsNullOrEmpty(lastROMFileLoaded))
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
                filePath = "C:\\Assemblers\\as65\\65c02_extended_opcodes_test.bin";
            //// debug
            if (!System.IO.File.Exists(filePath))
            {
                System.Console.WriteLine($"Cannot find the file '{filePath}'");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }


            System.Console.WriteLine();
            System.Console.WriteLine($"Reading in data from file {filePath}");
            var data = System.IO.File.ReadAllBytes(filePath);


            if (data.Length > (rom.EndAddress - rom.StartAddress))
            {
                System.Console.WriteLine($"Binary file is too big. ROM size is 32768 bytes, and this file is {data.Length} bytes");
                System.Console.Write("Enter full path to file:> ");
                goto WaitForInput;
            }

            var offset = false;
            if (data.Length < (rom.EndAddress - rom.StartAddress))
            {
                System.Console.WriteLine();
                System.Console.WriteLine($"Binary file is smaller than ROM size. Do you want to off set this to the end of the ROM?");
                System.Console.Write("[Y] to offset, [any other key] to load at start of ROM:> ");
                offset = (System.Console.ReadKey().Key == ConsoleKey.Y);
            }

            emulator.LoadROM(data, offset);
            binaryFileLoaded = true;
            var disassm = mainMenu.Items.FirstOrDefault(x => x.MenuAction == DisplayDisassembler);
            if (disassm != null)
                disassm.Hidden = !binaryFileLoaded;
            var loadbin = mainMenu.Items.FirstOrDefault(x => x.MenuAction == DisplayLoadROMFile);
            if (loadbin != null)
                loadbin.Text += " \u221A";

            var startAddr = offset ? (rom.EndAddress - data.Length + 1) : (rom.StartAddress);
            lastROMFileLoaded = filePath;
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine($"Loaded {data.Length} bytes into ROM starting at location ${startAddr:X4}");
            System.Console.Write("Press Enter to return to main menu...");
            System.Console.ReadLine();
        }

        #endregion

        #region Memory Monitor
        private static void DisplayMonitor(int left = 0, int topOffset = 0)
        {
        Reset:
            System.Console.Clear();
            CreateSubMenuHeading("Memory Monitor");
            var row = 1;
            var right = System.Console.BufferWidth / 2;
            foreach (var menu in subMenu.Items)
            {
                if (row > mainMenu.NumberOfLines - 1)
                {
                    System.Console.CursorTop = row - mainMenu.NumberOfLines + 1;
                    System.Console.CursorLeft = right;
                }
                else
                {
                    System.Console.CursorTop = row;
                    System.Console.CursorLeft = 0;
                }
                row++;

                System.Console.Write($" {menu.ShortcutKey.ToString().PadLeft(6, ' ')} - {menu.Text}");
            }

            System.Console.CursorTop = mainMenu.NumberOfLines + 1;
            System.Console.CursorLeft = 0;
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));

        WaitForInput:
            var input = System.Console.ReadKey();

            var selected = subMenu.Items.FirstOrDefault(x => x.ShortcutKey == input.Key);
            if (selected != null)
            {
                if (selected.ShortcutKey == ConsoleKey.Escape)
                    return;

                ClearClientArea(true, subMenu.NumberOfLines + 1);
                
                selected.MenuAction(0, subMenu.NumberOfLines + 2);
                
                if(selected.ChildMenuItems != null)
                    subMenu = selected.ChildMenuItems;
            }

            goto WaitForInput;

        }
        private static void Monitor_DisplayHeader(int left = 0, int topOffset = 0)
        {
            System.Console.SetCursorPosition(left, topOffset);
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
            ClearClientArea(false, topOffSet);
        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, topOffSet);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, topOffSet);
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
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write($"${location:X4}   : ");
            for (int index = location; index < location + 16; index++)
            {
                if (index == requestedLoc)
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
            ClearClientArea(false, topOffset);
        Start:
            System.Console.SetCursorPosition(left, topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, topOffset);

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
            for (int index = location; index < location + 256; index += 16)
            {
                System.Console.Write($"${index:X4}   : ");

                for (int col = index; col < (index + 16); col++)
                {
                    if (left != 0 && col == currentStackPointer)
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
            ClearClientArea((left == 0), topOffSet);
        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, topOffSet);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, topOffSet);
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

        #endregion

        #region Emulator
        private static void DisplayEmulator(int left = 0, int topOffset = 0)
        {
            System.Console.Clear();
            CreateSubMenuHeading("Emulator");
            var row = 1;
            var right = System.Console.BufferWidth / 2;
            foreach (var menu in subMenu.Items)
            {
                if (row > mainMenu.NumberOfLines - 1)
                {
                    System.Console.CursorTop = row - mainMenu.NumberOfLines + 1;
                    System.Console.CursorLeft = right;
                }
                else
                {
                    System.Console.CursorTop = row;
                    System.Console.CursorLeft = 0;
                }
                row++;

                System.Console.Write($" {menu.ShortcutKey.ToString().PadLeft(6, ' ')} - {menu.Text}");
            }

            System.Console.CursorTop = mainMenu.NumberOfLines + 1;
            System.Console.CursorLeft = 0;
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));
            System.Console.SetCursorPosition(0, topOffset + subMenu.NumberOfLines + 1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(0, topOffset + subMenu.NumberOfLines + 2);
            System.Console.Write("Address:$---- | A:$-- X:$-- Y:$-- | SP:$---- | ST:-------- | PC: $---- |                        Clock Ticks:------------");
            System.Console.SetCursorPosition(0, topOffset + subMenu.NumberOfLines + 3);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(0, topOffset + subMenu.NumberOfLines + 4);
            lastInstructionPos = topOffset + subMenu.NumberOfLines + 4;
            emulatorStarted = true;

            if(!binaryFileLoaded)
            {
                DisplayError("NO BINARY FILE LOADED. Please go to main menu, and select option to load binary file.", ExceptionType.Warning);
            }

        WaitForInput:
            showDeviceActivity = true;
            var input = System.Console.ReadKey(true);

            var selected = subMenu.Items.FirstOrDefault(x => x.ShortcutKey == input.Key);
            if (selected != null)
            {
                if (selected.ShortcutKey == ConsoleKey.Escape)
                    return;

                var sideDisplayIDs = new int[] { 1, 2, 3, 7 };
                var clearAfterIDs = new int[] { 7, 8 };
                var isSideDisplay = sideDisplayIDs.Any(x => x == selected.Index);
                if (isSideDisplay)
                {
                    System.Console.CursorVisible = true;
                    left = (sideDisplayIDs.Any(x => x == selected.Index)) ? maxColumns / 2 : 0;
                    showDeviceActivity = false;
                    ClearClientArea(false, topOffset + subMenu.NumberOfLines + 4);
                }


                selected.MenuAction(left, topOffset + subMenu.NumberOfLines + 4);

                if (selected.ChildMenuItems != null)
                    subMenu = selected.ChildMenuItems;

                System.Console.CursorVisible = false;

                if(clearAfterIDs.Any(x => x == selected.Index))
                {
                    ClearClientArea(false, topOffset + subMenu.NumberOfLines + 3);
                }
            }
            goto WaitForInput;

        }

        private static void ResetEmulator(int left = 0, int topOffset = 0)
        {
            showDeviceActivity = false;
            lastInstructionPos = subMenu.NumberOfLines + 4;
            emulator.Reset();
            ClearClientArea(true, subMenu.NumberOfLines + 3);
            ClearClientArea(false, subMenu.NumberOfLines + 3);
        }

        private static void StepNextInstruction(int left = 0, int topOffset = 0)
        {
            ClearActivityArea();
            emulator.Step();
        }

        private static void SendNMISignal(int left = 0, int topOffset = 02)
        {
            emulator.SendNMI();
        }

        private static void SendIRQSignal(int left = 0, int topOffset = 0)
        {
            emulator.SendIRQ();
        }

        private static void RunEmulator(int left = 0, int topOffset = 02)
        {
            showDeviceActivity = false;
            ClearActivityArea();
            emulator.Run();
        }

        private static void DisplayBreakPoints(int left = 0, int topOffset = 2)
        {
            if (left == 0)
                left = maxColumns / 2;
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "Add/Remove PC Address (blank to quit) [AFB9]: >$";

            ClearClientArea(false, topOffset);

            // display list
            System.Console.SetCursorPosition(left, topOffset + 1);
            System.Console.WriteLine("Current Breakpoints:".PadRight(25, ' '));

            foreach (var bp in emulator.GetBreakPoints())
            {
                System.Console.CursorLeft = left;
                System.Console.WriteLine($"[${bp:X4}]");
            }


        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, topOffset);
            var input = System.Console.ReadLine();
            System.Console.CursorVisible = false;
            if (String.IsNullOrEmpty(input) || input == Environment.NewLine)
            {
                ClearError();
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
            System.Console.SetCursorPosition(left, topOffset + 1);
            System.Console.WriteLine("Current Breakpoints:".PadRight(25, ' '));
            System.Console.ForegroundColor = ConsoleColor.DarkCyan;
            foreach (var bp in emulator.GetBreakPoints())
            {
                System.Console.CursorLeft = left;
                System.Console.WriteLine($"[${bp:X4}]");
            }
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.CursorLeft = left;
            System.Console.WriteLine($"".PadRight((maxColumns / 2) - 2, ' ')); // clear the last entry off the screen
            goto Start;
        }

        private static void EditPCValue(int left = 0, int topOffset = 0)
        {
            if (left == 0)
                left = maxColumns / 2;
            var inputRegex = "^[0-9A-Fa-f]{4}$";
            var inputDisplay = "Set PC Address [AFB9]: >$";

            ClearClientArea(false, topOffset);
        Start:
            System.Console.CursorVisible = true;
            System.Console.SetCursorPosition(left, topOffset);
            System.Console.WriteLine(inputDisplay.PadRight(60, ' '));
            System.Console.SetCursorPosition(inputDisplay.Length + left, topOffset);
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
            UpdateProgramCounterDisplay(inputValue);
        }

        private static void UpdateAddressDisplay(AddressBusEventArgs sender)
        {
            var topOffset = subMenu.NumberOfLines + 2;

            if (!emulatorStarted)
                return;
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            var curreColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.SetCursorPosition(9, topOffset);
            System.Console.Write($"{sender.Address:X4}");
            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = curreColor;
        }

        private static void UpdateProgramCounterDisplay(ushort newValue)
        {
            var topOffset = subMenu.NumberOfLines + 2;

            if (!emulatorStarted)
                return;
            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            var curreColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.SetCursorPosition(66, topOffset);
            System.Console.Write($"{newValue:X4}");
            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = curreColor;
        }

        private static void DisplayRegisters(byte A, byte X, byte Y, byte ST, ushort SP, ushort PC, double clockTicks)
        {
            var topOffset = subMenu.NumberOfLines + 2;
            currentStackPointer = SP;

            var curLeft = System.Console.CursorLeft;
            var curTop = System.Console.CursorTop;
            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.SetCursorPosition(0, topOffset);

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
            System.Console.CursorLeft = 108; System.Console.Write($"{clockTicks}".PadLeft(12, ' '));

            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.SetCursorPosition(0, topOffset + 1);
            System.Console.Write("".PadRight(maxColumns, '-'));
            System.Console.SetCursorPosition(curLeft, curTop);
        }
        private static void ShowLastInstruction(string instruction, string rawData, ushort PC)
        {
            var topOffset = subMenu.NumberOfLines + 1;
            if (lastInstructionPos > clientAreaHeight)
            {
                var sourceLeft = 0;
                var sourceTop = topOffset + 3;
                var sourceWidth = (maxColumns / 2) - 1;
                var sourceHeight = clientAreaHeight - sourceTop + 1;
                var targetLeft = 0;
                var targetTop = topOffset + 2;

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

        private static void OnInstructionExecuting(OnInstructionExecutingEventArg e)
        {
            ShowLastInstruction(e.DecodedInstruction, e.RawData, e.PC);
        }
        private static void OnInstructionExecuted(OnInstructionExecutedEventArg e)
        {
            DisplayRegisters(e.A, e.X, e.Y, (byte)e.ST, e.SP, e.PC, e.ClockTicks);
        }

        #endregion

        #region OpCode Viewer
        private static void ShowOpCodeApplication(int arg1, int arg2)
        {
            try
            {
                ProcessStartInfo startinfo = new ProcessStartInfo(".\\OpCodeViewer.exe");
                startinfo.CreateNoWindow = true;
                startinfo.UseShellExecute = true;
                p = Process.Start(startinfo);
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message, ExceptionType.Error);
            }
        }
        #endregion

        #region Disassembler
        private static void DisplayDisassembler(int arg1, int arg2)
        {
            //Reset:
            System.Console.Clear();
            CreateSubMenuHeading("Disassembler");
            var row = 1;
            var right = System.Console.BufferWidth / 2;
            if(subMenu != null)
            {
                foreach (var menu in subMenu.Items)
                {
                    if (row > subMenu.NumberOfLines - 1)
                    {
                        System.Console.CursorTop = row - subMenu.NumberOfLines + 1;
                        System.Console.CursorLeft = right;
                    }
                    else
                    {
                        System.Console.CursorTop = row;
                        System.Console.CursorLeft = 0;
                    }
                    row++;

                    System.Console.Write($" {menu.ShortcutKey.ToString().PadLeft(6, ' ')} - {menu.Text}");
                }

                System.Console.CursorTop = subMenu.NumberOfLines + 1;
            }
            System.Console.CursorLeft = 0;
            System.Console.WriteLine("".PadRight(maxColumns - 1, '-'));

        WaitForSelection:
            var input = System.Console.ReadKey();

            var selected = mainMenu.Items.FirstOrDefault(x => x.ShortcutKey == input.Key);
            if (selected != null)
            {
                if (selected.ShortcutKey == ConsoleKey.Escape)
                    return;
                subMenu = selected.ChildMenuItems;
                selected.MenuAction(0, 0);
                //goto Reset;
            }
            goto WaitForSelection;

        }
        #endregion

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            emulator.Mode = RunMode.Debug;
            showDeviceActivity = true;
            e.Cancel = true;
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
            else if (exceptionType == ExceptionType.Warning)
            {
                System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                System.Console.Write($"WARNING: {errorMsg}".PadRight(100, ' '));
            }
            else if (exceptionType == ExceptionType.Debug)
            {
                System.Console.ForegroundColor = ConsoleColor.Magenta;
                System.Console.Write($"DEBUG: {errorMsg}".PadRight(100, ' '));
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
            UpdateAddressDisplay(arg);
            if (showDeviceActivity)
                UpdateDeviceActivity(arg);
        }
        private static void CreateSubMenuHeading(string headingText)
        {
            System.Console.Write("".PadRight(((System.Console.BufferWidth) / 2) - (headingText.Length / 2), '-'));
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write(headingText);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine("".PadRight(((maxColumns - 1) / 2) - (headingText.Length / 2), '-'));
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
            var sourceWidth = maxColumns - 2;
            var sourceHeight = 4;
            var targetLeft = 2;
            var targetTop = 35;

            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);

            System.Console.SetCursorPosition(0, maxRows - 1);
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            if (sender.Mode == DataBusMode.Read)
                System.Console.Write($" >Read value ${sender.Data:X2} from address ${sender.Address:X4} on device '{sender.DeviceName}'".PadRight(maxColumns - 1, ' '));
            else
                System.Console.Write($" >Wrote value ${sender.Data:X2} to address ${sender.Address:X4} on device '{sender.DeviceName}'".PadRight(maxColumns - 1, ' '));

            System.Console.SetCursorPosition(curLeft, curTop);
            System.Console.ForegroundColor = curreColor;


        }
        private static void ClearClientArea(bool left, int topOffset = 0)
        {
            var curLeft = left == true ? 0 : (maxColumns / 2);
            var currTop = System.Console.CursorTop;
            for (int index = topOffset+1; index <= clientAreaHeight; index++)
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

 

        private static void SetupMenuStructure()
        {
            mainMenu = new MenuCollection();
            mainMenu.NumberOfLines = 7;
            mainMenu.Items = new List<MenuItem> {
                new MenuItem { Index = 1, Text = "Load Binary Image into ROM", ShortcutKey = ConsoleKey.F1, MenuAction = DisplayLoadROMFile },
                new MenuItem { Index = 2, Text = "Memory Monitor", ShortcutKey = ConsoleKey.F2, MenuAction = DisplayMonitor, ChildMenuItems = new MenuCollection { NumberOfLines = 7,
                    Items = new List<MenuItem> {
                        { new MenuItem { Index = 1, Text = "View Memory Location", ShortcutKey = ConsoleKey.F1, MenuAction = Monitor_DisplayLocation} },
                        { new MenuItem { Index = 2, Text = "View Memory Page", ShortcutKey = ConsoleKey.F2, MenuAction = Monitor_DisplayPage} },
                        { new MenuItem { Index = 3, Text = "Edit Memory Location", ShortcutKey = ConsoleKey.F3, MenuAction = Monitor_EditLocation} },
                        { new MenuItem {Index = 4,Text = "Back to main menu",ShortcutKey = ConsoleKey.Escape} }
                    }
                } },
                new MenuItem {Index = 3, Text = "Start Emulator", ShortcutKey = ConsoleKey.F3, MenuAction = DisplayEmulator, ChildMenuItems = new MenuCollection { NumberOfLines = 7, 
                    Items = new List<MenuItem>
                    {
                        { new MenuItem { Index = 1, Text = "View Memory Location", ShortcutKey = ConsoleKey.F1, MenuAction = Monitor_DisplayLocation} },
                        { new MenuItem { Index = 2, Text = "View Memory Page", ShortcutKey = ConsoleKey.F2, MenuAction = Monitor_DisplayPage} },
                        { new MenuItem { Index = 3, Text = "Edit Memory Location", ShortcutKey = ConsoleKey.F3, MenuAction = Monitor_EditLocation} },
                        { new MenuItem { Index = 4, Text = "Run (Ctrl+Break to break into Debug)", ShortcutKey = ConsoleKey.F5, MenuAction = RunEmulator } },
                        { new MenuItem { Index = 5, Text = "Send IRQ Signal", ShortcutKey = ConsoleKey.F6, MenuAction = SendIRQSignal} },
                        { new MenuItem { Index = 6, Text = "Send NMI Signal", ShortcutKey = ConsoleKey.F7, MenuAction = SendNMISignal} },
                        { new MenuItem { Index = 7, Text = "Set Program Counter Value", ShortcutKey = ConsoleKey.F8, MenuAction = EditPCValue} },
                        { new MenuItem { Index = 8, Text = "Add/Remove Breakpoint", ShortcutKey = ConsoleKey.F9, MenuAction = DisplayBreakPoints} },
                        { new MenuItem { Index = 9, Text = "Step next Instruction", ShortcutKey = ConsoleKey.F10, MenuAction = StepNextInstruction} },
                        { new MenuItem { Index = 10, Text = "Reset CPU", ShortcutKey = ConsoleKey.F12, MenuAction = ResetEmulator} },
                        { new MenuItem {Index = 4,Text = "Back to main menu",ShortcutKey = ConsoleKey.Escape} }
                    }
                } },
                new MenuItem {Index = 4, Text = "OpCode Viewer Application", ShortcutKey = ConsoleKey.F8, MenuAction = ShowOpCodeApplication},
                new MenuItem {Index = 5, Text = "Disassembler", ShortcutKey = ConsoleKey.F4, MenuAction = DisplayDisassembler, Hidden = true, ChildMenuItems = new MenuCollection { NumberOfLines = 7,
                    Items = new List<MenuItem>
                    {
                        { new MenuItem { Index = 1, Text = "View Memory Location", ShortcutKey = ConsoleKey.F1, MenuAction = Monitor_DisplayLocation} },
                        { new MenuItem {Index = 4,Text = "Back to main menu",ShortcutKey = ConsoleKey.Escape} }
                    }
                } },
                new MenuItem {Index = 6,Text = "Exit",ShortcutKey = ConsoleKey.Escape}
            };

        }


    }
}
