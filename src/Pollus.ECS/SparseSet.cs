namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public class SparseSet<T>
{
    T[] values;
    SparseSet sparseSet;

    public int Length => sparseSet.Length;
    public bool IsEmpty => sparseSet.IsEmpty;

    public SparseSet(int capacity)
    {
        sparseSet = new SparseSet(capacity);
        values = new T[capacity];
    }

    public void Add(int item, T value)
    {
        var idx = sparseSet.Add(item);
        if (idx >= values.Length) Array.Resize(ref values, idx * 2);
        values[idx] = value;
    }

    public void Remove(int item)
    {
        var idx = sparseSet.Remove(item);
        if (idx != -1) values[idx] = default!;
    }

    public bool Contains(int item)
    {
        return sparseSet.Contains(item);
    }

    public ref T Get(int idx)
    {
        var i = sparseSet.Get(idx);
        if (i == -1) return ref Unsafe.NullRef<T>();
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
        T[] values;

        public Enumerator(T[] values, int length)
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

        public ref T Current => ref values[index];
    }
}

public class SparseSet
{
    int[] dense;
    int[] sparse;
    int n;

    public int Length => n;
    public bool IsEmpty => n == 0;

    public SparseSet(int capacity)
    {
        dense = new int[capacity];
        sparse = new int[capacity];
        Array.Fill(sparse, -1);
        n = 0;
    }

    public void Clear()
    {
        n = 0;
    }

    public bool Contains(int idx)
    {
        if (idx >= sparse.Length) return false;
        return sparse[idx] != -1;
    }

    public int Get(int idx)
    {
        return sparse[idx];
    }

    /// <summary>
    /// adds item, returns the index of the item in the dense array
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public int Add(int idx)
    {
        if (Contains(idx))
            return sparse[idx];
        if (idx >= sparse.Length) ResizeSparse(idx);
        if (n >= dense.Length) ResizeDense();

        sparse[idx] = n;
        dense[n] = idx;
        n++;

        return n - 1;
    }

    /// <summary>
    /// removes item, returns the index of the item in the dense array
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public int Remove(int idx)
    {
        if (!Contains(idx)) return -1;

        n--;
        int d = sparse[idx];
        int last = dense[n];
        dense[d] = last;
        sparse[last] = d;
        sparse[idx] = -1;

        return d;
    }

    void ResizeSparse(int size)
    {
        Array.Resize(ref sparse, size * 2);
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
        readonly int[] dense;
        int index;
        int length;

        public Enumerator(int[] dense, int length)
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

        public int Current => dense[index];
    }
}