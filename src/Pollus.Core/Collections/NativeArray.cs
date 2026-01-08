namespace Pollus.Collections;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe public struct NativeArray<T> : IDisposable
    where T : unmanaged
{
    public static NativeArray<T> Empty => new()
    {
        data = null,
        size = 0,
        length = 0,
    };

    T* data;
    int size;
    int length;

    public readonly int Size => size;
    public readonly int Length => length;
    public readonly void* Data => data;

    public ref T this[int index] => ref data[index];

    public NativeArray(int length)
    {
        this.length = length;
        size = length * Unsafe.SizeOf<T>();
        data = (T*)NativeMemory.AllocZeroed((nuint)size);
    }

    public void Dispose()
    {
        if (data == null) return;
        NativeMemory.Free(data);
    }

    public void Resize(int newLength)
    {
        var oldData = data;
        var newSize = newLength * Unsafe.SizeOf<T>();

        data = (T*)NativeMemory.AllocZeroed((nuint)newSize);
        NativeMemory.Copy(oldData, data, nuint.Min((nuint)size, (nuint)newSize));
        NativeMemory.Free(oldData);

        length = newLength;
        size = newSize;
    }

    public void Set(int index, T value)
    {
        data[index] = value;
    }

    public ref T Get(int index)
    {
        return ref data[index];
    }

    public ref TCast Get<TCast>(int index)
        where TCast : unmanaged
    {
        return ref Unsafe.AsRef<TCast>(Unsafe.Add<T>(data, index));
    }

    public readonly Span<T> AsSpan() => new(data, length);
    public readonly Span<T> Slice(int start) => new(data + start, length - start);
    public readonly Span<T> Slice(int start, int length) => new(data + start, length);
    public readonly NativeSlice<T> AsSlice() => new(data, length);
    public readonly NativeSlice<T> AsSlice(int start, int length) => new((T*)Unsafe.Add<T>(data, start), length);
    public Enumerator GetEnumerator() => new(data, length);

    public NativeArray<T> SubArray(int start, int length)
    {
        return new()
        {
            data = (T*)Unsafe.Add<T>(data, start),
            length = length,
            size = length * Unsafe.SizeOf<T>(),
        };
    }

    public struct Enumerator
    {
        T* data;
        int index;
        int length;

        public ref T Current => ref data[index];

        public Enumerator(T* data, int length)
        {
            this.data = data;
            this.length = length;
            index = -1;
        }

        public bool MoveNext()
        {
            index++;
            return index < length;
        }
    }
}
