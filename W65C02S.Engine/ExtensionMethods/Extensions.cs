using System;
using System;
//using System.Windows.Forms;

//public static class ControlExtensions
//{
//    /// <summary>
//    /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
//    /// </summary>
//    /// <param name="control"></param>
//    /// <param name="code"></param>
//    public static void UIThread(this Control @this, Action code)
//    {
//        if (@this.InvokeRequired)
//        {
//            @this.BeginInvoke(code);
//        }
//        else
//        {
//            code.Invoke();
//        }
//    }
//}

public static class BinaryExt
{
    public static string ToBinary(this byte number)
    {
        return NumberToBinary(number, 8);
    }

    public static string ToBinary(this ushort number)
    {
        return NumberToBinary(number, 16);
    }

    public static string NumberToBinary(int number, int bitsLength = 32)
    {
        string result = Convert.ToString(number, 2).PadLeft(bitsLength, '0');

        return result;
    }


}
