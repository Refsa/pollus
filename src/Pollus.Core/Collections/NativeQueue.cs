namespace Pollus.Collections;

public struct NativeQueue<T> : IDisposable
    where T : unmanaged
{
    NativeArray<T> data;
    int head;
    int tail;

    public readonly int Count => (tail - head + data.Length) % data.Length;

    public ref T First => ref data[head];

    public NativeQueue(int capacity)
    {
        data = new(capacity);
    }

    public void Dispose()
    {
        data.Dispose();
    }

    public void Enqueue(T value)
    {
        data[tail] = value;
        tail = (tail + 1) % data.Length;
    }

    public T Dequeue()
    {
        var value = data[head];
        head = (head + 1) % data.Length;
        return value;
    }

    public void Clear()
    {
        head = tail;
    }
}
