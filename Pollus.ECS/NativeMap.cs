namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe public struct NativeMap<TKey, TValue> : IDisposable
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    static readonly TKey sentinel;
    static NativeMap()
    {
        var temp = stackalloc byte[Unsafe.SizeOf<TKey>()];
        Unsafe.InitBlock(temp, byte.MaxValue, (uint)Unsafe.SizeOf<TKey>());
        sentinel = *(TKey*)temp;
    }

    NativeArray<TKey> keys;
    NativeArray<TValue> values;
    int count = 0;
    int capacity;

    public readonly NativeArray<TKey> Keys => keys;
    public readonly NativeArray<TValue> Values => values;
    public readonly int Count => count;

    public NativeMap(int initialCapacity)
    {
        count = 0;
        capacity = int.Max(initialCapacity, 1);
        keys = new(capacity);
        values = new(capacity);

        for (int i = 0; i < capacity; i++)
        {
            keys.Set(i, sentinel);
        }
    }

    public void Dispose()
    {
        keys.Dispose();
        values.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int Hash(TKey key)
    {
        return key.GetHashCode() % capacity;
    }

    void Resize(int newCapacity)
    {
        NativeArray<TKey> newKeys = new NativeArray<TKey>(newCapacity);
        NativeArray<TValue> newValues = new NativeArray<TValue>(newCapacity);

        for (int i = 0; i < newCapacity; i++)
        {
            newKeys[i] = sentinel;
        }

        for (int i = 0; i < capacity; i++)
        {
            if (keys[i].Equals(sentinel)) continue;

            TKey key = keys[i];
            TValue value = values[i];
            int index = key.GetHashCode() % newCapacity;
            int probeCount = 0;

            for (int j = 0; j < newCapacity; j++)
            {
                if (newKeys[index].Equals(sentinel))
                {
                    break;
                }

                index = (index + 1) % newCapacity;
                probeCount++;
            }

            newKeys[index] = key;
            newValues[index] = value;
        }

        keys.Dispose();
        values.Dispose();

        keys = newKeys;
        values = newValues;
        capacity = newCapacity;
    }

    public void Add(TKey key, TValue value)
    {
        if (count == capacity)
        {
            Resize(capacity * 2);
        }

        int index = Hash(key);
        int probeCount = 0;

        for (int i = 0; i < capacity; i++, index = (index + 1) % capacity, probeCount++)
        {
            if (keys[index].Equals(sentinel))
            {
                keys.Set(index, key);
                values.Set(index, value);
                count++;
                break;
            }

            int existingProbeCount = (index - Hash(keys[index]) + capacity) % capacity;

            if (existingProbeCount >= probeCount) continue;

            // Swap elements
            TKey tempKey = keys[index];
            TValue tempValue = values[index];

            keys[index] = key;
            values[index] = value;

            key = tempKey;
            value = tempValue;
            probeCount = existingProbeCount;
        }
    }

    public bool Has(TKey key)
    {
        return GetIndex(key) != -1;
    }

    public ref TValue Get(TKey key)
    {
        int index = GetIndex(key);
        if (index != -1)
        {
            return ref values.Get(index);
        }
        return ref Unsafe.NullRef<TValue>();
    }

    int GetIndex(TKey key)
    {
        int index = Hash(key);
        var keySpan = keys.AsSpan();
        for (int i = 0; i < keySpan.Length; i++, index = (index + 1) % capacity)
        {
            if (keySpan[index].Equals(key))
            {
                return index;
            }

            if (keySpan[index].Equals(sentinel))
            {
                break;
            }
        }
        return -1;
    }
}