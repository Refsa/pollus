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

    public Span<T> AsSpan() => new(items, 0, count);

    public void Clear()
    {
        // Array.Clear(items, 0, count);
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

    public T this[int index]
    {
        get => items[index];
        set => items[index] = value;
    }
}
