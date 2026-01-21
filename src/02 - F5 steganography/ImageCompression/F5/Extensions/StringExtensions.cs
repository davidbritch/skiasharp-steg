using System.Text;

namespace ImageCompression.F5.Extensions;

public static class StringExtensions
{
    public static int GetBitLength(this string str)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(str);
        return messageBytes.Length * 8;
    }
}