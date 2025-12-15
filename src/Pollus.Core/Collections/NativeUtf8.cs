namespace Pollus.Collections;

using System.Buffers;
using System.Diagnostics;
using System.Text;

[DebuggerDisplay("{ToString()}")]
unsafe public record struct NativeUtf8 : IDisposable
{
    public static NativeUtf8 Null => new()
    {
        data = NativeArray<byte>.Empty,
    };

    NativeArray<byte> data;
    int count = 0;

    public byte* Pointer => (byte*)data.Data;

    public NativeUtf8(ReadOnlySpan<char> str)
    {
        count = Encoding.UTF8.GetByteCount(str) + 1;
        data = new NativeArray<byte>(count);
        Encoding.UTF8.GetBytes(str, data.AsSpan());
        data[^1] = 0;
    }

    public NativeUtf8(ReadOnlySpan<byte> data)
    {
        count = data.Length;
        this.data = new NativeArray<byte>(count);
        data.CopyTo(this.data.AsSpan());
        this.data[^1] = 0;
    }

    public void Dispose()
    {
        data.Dispose();
    }

    public static implicit operator NativeUtf8(ReadOnlySpan<char> str) => new(str);

    public static implicit operator NativeUtf8(ReadOnlySpan<byte> utf8) => new(utf8);

    public Enumerator GetEnumerator() => new(Pointer, count);

    public ReadOnlySpan<byte> AsSpan() => new(Pointer, count);

    public override string ToString()
    {
        return Encoding.UTF8.GetString(data.Slice(0, count));
    }

    public ref struct Enumerator
    {
        readonly byte* data;
        readonly int length;
        int index;

        public char Current { get; private set; }

        public Enumerator(byte* data, int length)
        {
            this.data = data;
            this.length = length;
            index = 0;
            Current = '\0';
        }

        public bool MoveNext()
        {
            if (index >= length) return false;

            var b = data[index];
            if (b == 0) return false;

            if ((b & 0x80) == 0)
            {
                Current = (char)b;
                index++;
                return true;
            }

            if ((b & 0xE0) == 0xC0)
            {
                Current = (char)(((b & 0x1F) << 6) | (data[index + 1] & 0x3F));
                index += 2;
                return true;
            }

            if ((b & 0xF0) == 0xE0)
            {
                Current = (char)(((b & 0x0F) << 12) | ((data[index + 1] & 0x3F) << 6) | (data[index + 2] & 0x3F));
                index += 3;
                return true;
            }

            if ((b & 0xF8) == 0xF0)
            {
                Current = '?';
                index += 4;
                return true;
            }

            index++;
            return false;
        }
    }
}