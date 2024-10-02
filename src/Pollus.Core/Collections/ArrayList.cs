namespace Pollus.Collections;

public class ArrayList<T>
{
    private T[] items;
    private int count;

    public int Count => count;

    public ArrayList(int capacity = 10)
    {
        items = new T[capacity];
    }

    public Span<T> AsSpan(int count) => items.AsSpan(0, count);

    public void SetCount(int count)
    {
        this.count = count;
    }

    public void Clear(bool zero = false)
    {
        if (zero) Array.Clear(items, 0, count);
        count = 0;
    }

    public void Add(T item)
    {
        if (count == items.Length) Array.Resize(ref items, count * 2);
        items[count++] = item;
    }

    public ref T Get(int index)
    {
        return ref items[index];
    }

    public void EnsureCapacity(int capacity)
    {
        if (items.Length < capacity) Array.Resize(ref items, capacity);
    }

    public T this[int index]
    {
        get => items[index];
        set => items[index] = value;
    }
}
