using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace W65C02S.Console
{
    public enum ScreenAlignment
    {
        Centre = 0,
        Left = 1,
        Right = 2
    }

    public class MenuItem
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public bool Hidden { get; set; }
        public ScreenAlignment TextAlign { get; set; }
        public ConsoleColor TextColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public ConsoleKey ShortcutKey { get; set; }
        public Action<int, int> MenuAction { get; set; }
        public MenuCollection ChildMenuItems { get; set; }

    }
}
