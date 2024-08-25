namespace Pollus.Collections;

using System.Diagnostics;
using System.Runtime.CompilerServices;

unsafe public struct NativeMap<TKey, TValue> : IDisposable
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    public static TKey Sentinel;
    static NativeMap()
    {
        var temp = stackalloc byte[Unsafe.SizeOf<TKey>()];
        Unsafe.InitBlock(temp, byte.MaxValue - 1, (uint)Unsafe.SizeOf<TKey>());
        Sentinel = *(TKey*)temp;
    }

    NativeArray<TKey> keys;
    NativeArray<TValue> values;
    int count = 0;
    int capacity;

    public readonly NativeArray<TKey> Keys => keys;
    public readonly NativeArray<TValue> Values => values;
    public readonly int Count => count;

    public ref TValue this[in TKey key]
    {
        get => ref Get(key);
    }

    public NativeMap() : this(0) { }

    public NativeMap(int initialCapacity)
    {
        count = 0;
        capacity = int.Max(initialCapacity, 1);
        keys = new(capacity);
        values = new(capacity);

        for (int i = 0; i < capacity; i++)
        {
            keys.Set(i, Sentinel);
        }
    }

    public void Dispose()
    {
        keys.Dispose();
        values.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int Hash(scoped in TKey key)
    {
        var hash = key.GetHashCode();
        return hash * int.Sign(hash) % capacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    void Resize(int newCapacity)
    {
        NativeArray<TKey> newKeys = new(newCapacity);
        NativeArray<TValue> newValues = new(newCapacity);

        for (int i = 0; i < newCapacity; i++)
        {
            newKeys[i] = Sentinel;
        }

        for (int i = 0; i < capacity; i++)
        {
            if (keys[i].Equals(Sentinel)) continue;

            TKey key = keys[i];
            TValue value = values[i];
            int index = key.GetHashCode() % newCapacity;
            int probeCount = 0;

            for (int j = 0; j < newCapacity; j++)
            {
                if (newKeys[index].Equals(Sentinel))
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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
            if (keys[index].Equals(Sentinel))
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool Has(scoped in TKey key)
    {
        return GetIndex(key) != -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public ref TValue Get(scoped in TKey key)
    {
        int index = GetIndex(key);
        if (index != -1)
        {
            return ref values[index];
        }
        return ref Unsafe.NullRef<TValue>();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Set(scoped in TKey key, scoped in TValue value)
    {
        int index = GetIndex(key);
        if (index != -1)
        {
            values.Set(index, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool TryGetValue(scoped in TKey key, out TValue value)
    {
        int index = GetIndex(key);
        if (index != -1)
        {
            value = values[index];
            return true;
        }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Remove(scoped in TKey key)
    {
        int index = GetIndex(key);
        if (index != -1)
        {
            keys.Set(index, Sentinel);
            count--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    int GetIndex(scoped in TKey key)
    {
        int index = Hash(key);
        var keySpan = keys.AsSpan();
        for (int i = 0; i < keySpan.Length; i++, index = (index + 1) % capacity)
        {
            if (keySpan[index].Equals(key))
            {
                return index;
            }
        }
        return -1;
    }
}