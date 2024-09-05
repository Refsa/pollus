using System.Text;

namespace Pollus.Collections;

unsafe public struct NativeUtf8 : IDisposable
{
    NativeArray<byte> data;

    public byte* Pointer => (byte*)data.Data;

    public NativeUtf8(string str)
    {
        data = new NativeArray<byte>(Encoding.UTF8.GetByteCount(str) + 1);
        int actualBytes = Encoding.UTF8.GetBytes(str, data.AsSpan());
        data[actualBytes] = 0;
    }

    public NativeUtf8(ReadOnlySpan<char> str)
    {
        data = new NativeArray<byte>(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, data.AsSpan());
    }

    public void Dispose()
    {
        data.Dispose();
    }
}