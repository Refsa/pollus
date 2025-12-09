namespace Pollus.Collections;

public struct NativeList<T> : IDisposable
    where T : unmanaged
{
    NativeArray<T> data;
    int count;

    public readonly int Count => count;

    public ref T this[int index] => ref data[index];

    public NativeList(int capacity)
    {
        data = new(capacity);
        count = 0;
    }

    public void Dispose()
    {
        data.Dispose();
    }

    public void Add(T value)
    {
        if (count >= data.Length) data.Resize(count * 2);
        data[count] = value;
        count++;
    }

    public ref T Get(int index)
    {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        return ref data[index];
    }

    public void Set(int index, T value)
    {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        data[index] = value;
    }

    public void Clear()
    {
        count = 0;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        var span = data.Slice(index, count - index - 1);
        span.CopyTo(data.AsSpan()[index..]);
        count--;
    }

    public Span<T> AsSpan() => data.Slice(0, count);
}