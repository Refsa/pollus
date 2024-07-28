namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe public struct NativeMap<TKey, TValue> : IDisposable
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    NativeArray<TKey> keys;
    NativeArray<TValue> values;
    int count = 0;

    public readonly NativeArray<TKey> Keys => keys;
    public readonly NativeArray<TValue> Values => values;
    public readonly int Count => count;

    public NativeMap(int capacity)
    {
        keys = new(capacity);
        values = new(capacity);
    }

    public void Dispose()
    {
        keys.Dispose();
        values.Dispose();
    }

    public void Add(TKey key, TValue value)
    {
        keys.Resize(count + 1);
        values.Resize(count + 1);
        keys.Set(count, key);
        values.Set(count, value);
        count++;
    }

    public bool Has(TKey key)
    {
        for (int i = 0; i < count; i++)
        {
            if (keys[i].Equals(key))
                return true;
        }

        return false;
    }

    public ref TValue Get(TKey key)
    {
        for (int i = 0; i < count; i++)
        {
            if (keys[i].Equals(key))
                return ref values[i];
        }

        return ref Unsafe.NullRef<TValue>();
    }
}

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
