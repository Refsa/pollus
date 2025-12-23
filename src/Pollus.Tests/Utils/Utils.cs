namespace Pollus.Tests.Utils;

using System.Text;

public static class Utils
{
    public static byte[] ToBytes(this string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }
}