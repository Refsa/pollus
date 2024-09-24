namespace Pollus.Collections;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
unsafe public struct NativeMap<TKey, TValue> : IDisposable
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    public enum EntryState : byte
    {
        Empty = 0,
        Occupied = 1,
        Tombstone = 2,
    }

    public struct KeyEntry
    {
        public TKey Key;
        internal EntryState state;
    }

    NativeArray<KeyEntry> keys;
    NativeArray<TValue> values;
    int count = 0;
    int capacity;

    public readonly KeyEnumerator Keys => new(keys);
    public readonly ValueEnumerator Values => new(Keys, values);
    public readonly int Count => count;

    public ref TValue this[in TKey key]
    {
        get => ref Get(key);
    }

    public NativeMap() : this(0) { }

    public NativeMap(int initialCapacity)
    {
        count = 0;
        capacity = 1;
        while (capacity < initialCapacity)
        {
            capacity <<= 1;
        }
        keys = new(capacity);
        values = new(capacity);
    }

    public void Dispose()
    {
        keys.Dispose();
        values.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    readonly int Hash(scoped in TKey key, int capacity)
    {
        var hash = key.GetHashCode();
        return hash & 0x7FFFFFFF & (capacity - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void Resize(int newCapacity)
    {
        NativeArray<KeyEntry> newKeys = new(newCapacity);
        NativeArray<TValue> newValues = new(newCapacity);

        for (int i = 0; i < capacity; i++)
        {
            ref var key = ref keys[i];
            if (key.state != EntryState.Occupied) continue;

            int hash = Hash(key.Key, newCapacity);
            int quadProbe = hash;
            int j = 0;

            while (newKeys[quadProbe].state != EntryState.Empty)
            {
                j++;
                quadProbe = (hash + j * j) & (newCapacity - 1);
            }

            newKeys[quadProbe] = key;
            newValues[quadProbe] = values[i];
        }

        keys.Dispose();
        values.Dispose();

        keys = newKeys;
        values = newValues;
        capacity = newCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Add(TKey key, TValue value)
    {
        if (count >= capacity * 0.75f)
        {
            Resize(capacity * 2);
        }

        int hash = Hash(key, capacity);
        int i = 0;
        int quadProbe;

        while (true)
        {
            quadProbe = (hash + i * i) & (capacity - 1);
            ref var entry = ref keys[quadProbe];

            if (entry.state != EntryState.Occupied)
            {
                entry.Key = key;
                entry.state = EntryState.Occupied;
                values[quadProbe] = value;
                count++;
                return;
            }

            i++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Has(scoped in TKey key)
    {
        return !Unsafe.IsNullRef(ref GetEntry(key, Hash(key, capacity)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref TValue Get(scoped in TKey key)
    {
        ref var val = ref GetValue(key, Hash(key, capacity));
        return ref val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Set(scoped in TKey key, scoped in TValue value)
    {
        ref var val = ref GetValue(key, Hash(key, capacity));
        val = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool TryGetValue(scoped in TKey key, out TValue value)
    {
        ref var val = ref GetValue(key, Hash(key, capacity));
        if (Unsafe.IsNullRef(ref val))
        {
            Unsafe.SkipInit(out value);
            return false;
        }
        value = val;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Remove(scoped in TKey key)
    {
        ref var entry = ref GetEntry(key, Hash(key, capacity));
        if (!Unsafe.IsNullRef(ref entry))
        {
            entry.state = EntryState.Tombstone;
            count--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    ref TValue GetValue(scoped in TKey key, int hash)
    {
        var keySpan = keys.AsSpan();
        for (int i = 0; i < keySpan.Length; i++)
        {
            int quadProbe = (hash + i * i) & (capacity - 1);
            ref var entry = ref keySpan[quadProbe];

            if (entry.state == EntryState.Empty) break;
            if (entry.state == EntryState.Occupied && entry.Key.Equals(key)) return ref values[quadProbe];
        }

        return ref Unsafe.NullRef<TValue>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    ref KeyEntry GetEntry(scoped in TKey key, int hash)
    {
        var keySpan = keys.AsSpan();
        for (int i = 0; i < keySpan.Length; i++)
        {
            int quadProbe = (hash + i * i) & (capacity - 1);
            ref var entry = ref keySpan[quadProbe];

            if (entry.state == EntryState.Empty) break;
            if (entry.state == EntryState.Occupied && entry.Key.Equals(key)) return ref entry;
        }

        return ref Unsafe.NullRef<KeyEntry>();
    }

    public ref struct KeyEnumerator
    {
        NativeArray<KeyEntry> keys;
        int index;

        public KeyEnumerator(NativeArray<KeyEntry> keys)
        {
            this.keys = keys;
            index = -1;
        }

        public TKey Current => keys[index].Key;
        public int Index => index;

        public bool MoveNext()
        {
            while (++index < keys.Length)
            {
                if (keys[index].state == EntryState.Occupied)
                {
                    return true;
                }
            }
            return false;
        }

        public KeyEnumerator GetEnumerator() => this;
    }

    public ref struct ValueEnumerator
    {
        KeyEnumerator keys;
        NativeArray<TValue> values;
        int index;

        public ValueEnumerator(KeyEnumerator keys, NativeArray<TValue> values)
        {
            this.values = values;
            this.keys = keys;
        }

        public ref TValue Current => ref values[index];

        public bool MoveNext()
        {
            while (keys.MoveNext())
            {
                index = keys.Index;
                return true;
            }
            return false;
        }

        public ValueEnumerator GetEnumerator() => this;
    }
}