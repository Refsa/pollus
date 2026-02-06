namespace Pollus.ECS;

using System.Numerics;
using System.Runtime.CompilerServices;

public class SparseSet<TIndex, TValue>
    where TIndex : unmanaged, INumber<TIndex>, IMinMaxValue<TIndex>
{
    TValue[] values;
    SparseSet<TIndex> sparseSet;

    public int Length => sparseSet.Length;
    public bool IsEmpty => sparseSet.IsEmpty;

    public SparseSet(int capacity)
    {
        sparseSet = new SparseSet<TIndex>(capacity);
        values = new TValue[capacity];
    }

    public void Add(TIndex item, TValue value)
    {
        var idx = sparseSet.Add(item);
        if (idx >= values.Length) Array.Resize(ref values, idx * 2);
        values[idx] = value;
    }

    public void Remove(TIndex item)
    {
        var idx = sparseSet.Remove(item);
        if (idx != -1)
        {
            values[idx] = values[Length];
            values[Length] = default!;
        }
    }

    public bool Contains(TIndex item)
    {
        return sparseSet.Contains(item);
    }

    public ref TValue Get(TIndex idx)
    {
        var i = sparseSet.Get(idx);
        if (i == -1) return ref Unsafe.NullRef<TValue>();
        return ref values[i];
    }

    public void Clear()
    {
        sparseSet.Clear();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(values, sparseSet.Length);
    }

    public ref struct Enumerator
    {
        int length;
        int index;
        TValue[] values;

        public Enumerator(TValue[] values, int length)
        {
            this.length = length;
            this.values = values;
            index = -1;
        }

        public bool MoveNext()
        {
            index++;
            return index < length;
        }

        public ref TValue Current => ref values[index];
    }
}

public class SparseSet<TIndex>
    where TIndex : unmanaged, INumber<TIndex>, IMinMaxValue<TIndex>
{
    TIndex[] dense;
    int[] sparse;
    int n;

    public int Length => n;
    public bool IsEmpty => n == 0;

    public SparseSet(int capacity)
    {
        dense = new TIndex[capacity];
        sparse = new int[capacity];
        Array.Fill(sparse, -1);
        n = 0;
    }

    public void Clear()
    {
        n = 0;
    }

    public bool Contains(TIndex idx)
    {
        int i = int.CreateTruncating(idx);
        if (i >= sparse.Length) return false;
        return sparse[i] != -1;
    }

    public int Get(TIndex idx)
    {
        int i = int.CreateTruncating(idx);
        if (i >= sparse.Length) return -1;
        return sparse[i];
    }

    public int Add(TIndex idx)
    {
        if (Contains(idx))
        {
            int i = int.CreateTruncating(idx);
            return sparse[i];
        }

        int sparseIdx = int.CreateTruncating(idx);
        if (sparseIdx >= sparse.Length) ResizeSparse(sparseIdx);
        if (n >= dense.Length) ResizeDense();

        sparse[sparseIdx] = n;
        dense[n] = idx;
        n++;

        return n - 1;
    }

    public int Remove(TIndex idx)
    {
        if (!Contains(idx)) return -1;

        int sparseIdx = int.CreateTruncating(idx);
        n--;
        int d = sparse[sparseIdx];
        TIndex last = dense[n];
        dense[d] = last;
        int lastSparseIdx = int.CreateTruncating(last);
        sparse[lastSparseIdx] = d;
        sparse[sparseIdx] = -1;

        return d;
    }

    void ResizeSparse(int size)
    {
        int oldLength = sparse.Length;
        Array.Resize(ref sparse, size * 2);
        Array.Fill(sparse, -1, oldLength, sparse.Length - oldLength);
    }

    void ResizeDense()
    {
        Array.Resize(ref dense, n * 2);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(dense, n);
    }

    public ref struct Enumerator
    {
        readonly TIndex[] dense;
        int index;
        int length;

        public Enumerator(TIndex[] dense, int length)
        {
            this.dense = dense;
            this.length = length;
            index = -1;
        }

        public bool MoveNext()
        {
            index++;
            return index < length;
        }

        public TIndex Current => dense[index];
    }
}