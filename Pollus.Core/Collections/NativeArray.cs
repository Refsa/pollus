namespace Pollus.Collections;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe public struct NativeArray<T> : IDisposable
    where T : unmanaged
{
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

    public void SwapRemove(int index)
    {
        data[index] = data[length - 1];
        data[length - 1] = default;
        length--;
    }

    public Span<T> AsSpan() => new(data, length);
    public Enumerator GetEnumerator() => new(data, length);

    public struct Enumerator
    {
        T* data;
        int index;
        int length;

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

        public ref T Current => ref data[index];
    }
}
