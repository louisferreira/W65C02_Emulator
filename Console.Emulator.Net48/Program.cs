using System;
using W6502C.CPU;


namespace Console.Emulator
{
    class Program
    {
        static W65C02S cpu;

        [STAThread]
        static void Main(string[] args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                string path = dialog.FileName;
            }



            var bus = new Bus();
            var ram = new RAM();
            bus.Connect(ram);

            cpu = new W65C02S(bus);
            cpu.OnOutput += Cpu_OnOutput;
            cpu.OnError += Cpu_OnError;

        Reset:
            System.Console.Clear();
            System.Console.WriteLine("CPU Started.");
            System.Console.WriteLine("F10 = Step, F5 = Run, F12 = Reset, X = Quit");
            System.Console.WriteLine();

            cpu.Reset();

        WaitForInput:
            var key = System.Console.ReadKey(true);
            if (key.Key == ConsoleKey.X)
            {
                goto Exit;
            }

            if (key.Key == ConsoleKey.F5)
            {
                cpu.RunMode = true;
                cpu.Run();
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
            cpu.OnOutput -= Cpu_OnOutput;
            cpu.OnError -= Cpu_OnError;

            System.Console.WriteLine("Bye");
        }
        private static void Cpu_OnOutput(object sender, OutputEventArg e)
        {
            System.Console.WriteLine($"{e.Address:X4} ({e.Type}) {e.Data:X2}  A:[{e.A:X2}] X:[{e.X:X2}] Y:[{e.Y:X2}]  ST:[{((byte)(e.ST)).ToBinary()}] SP:[{e.SP:X4}] ");
        }

        private static void Cpu_OnError(object sender, ExceptionEventArg e)
        {
            var cc = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"ERROR: {e.ErrorMessage}");
            System.Console.ForegroundColor = cc;
        }

    }
}
