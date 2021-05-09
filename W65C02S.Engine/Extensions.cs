using System;
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
