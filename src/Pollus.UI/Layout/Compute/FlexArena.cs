namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// LIFO bump allocator for temporary arrays during flex layout.
/// </summary>
internal sealed class FlexArena
{
    byte[] buffer;
    int offset;

    internal FlexArena(int initialCapacity = 64 * 1024)
    {
        buffer = new byte[initialCapacity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<T> Rent<T>(int count) where T : struct
    {
        int byteCount = count * Unsafe.SizeOf<T>();
        // Align to 8 bytes
        byteCount = (byteCount + 7) & ~7;

        if (offset + byteCount > buffer.Length)
            Grow(offset + byteCount);

        var span = MemoryMarshal.Cast<byte, T>(buffer.AsSpan(offset, count * Unsafe.SizeOf<T>()));
        offset += byteCount;
        span.Clear();
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Return<T>(int count) where T : struct
    {
        int byteCount = count * Unsafe.SizeOf<T>();
        byteCount = (byteCount + 7) & ~7;
        offset -= byteCount;
    }

    internal void Reset()
    {
        offset = 0;
    }

    void Grow(int minCapacity)
    {
        int newCap = System.Math.Max(buffer.Length * 2, minCapacity);
        var newBuffer = new byte[newCap];
        Buffer.BlockCopy(buffer, 0, newBuffer, 0, offset);
        buffer = newBuffer;
    }
}
