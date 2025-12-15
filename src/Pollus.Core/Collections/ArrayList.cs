namespace Pollus.Collections;

using System;
using System.Collections;
using System.Collections.Generic;

public class ArrayList<T> : IEnumerable<T>
{
    private T[] items;
    private int count;

    public int Count => count;

    public T this[int index]
    {
        get => items[index];
        set => items[index] = value;
    }

    public ArrayList(int capacity = 10)
    {
        items = new T[capacity];
    }

    public Span<T> AsSpan(int start, int count) => items.AsSpan(start, count);

    public Span<T> AsSpan(int count) => items.AsSpan(0, count);

    public Span<T> AsSpan() => items.AsSpan(0, count);

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

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        var span = items.AsSpan(index + 1, count - index - 1);
        span.CopyTo(items.AsSpan()[index..]);
        count--;
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        EnsureCapacity(count + items.Length);
        items.CopyTo(AsSpan(count, items.Length));
        count += items.Length;
    }

    public void EnsureCapacity(int capacity)
    {
        if (items.Length < capacity) Array.Resize(ref items, capacity);
    }

    public void CopyTo(ArrayList<T> other)
    {
        other.EnsureCapacity(count);
        AsSpan().CopyTo(other.items.AsSpan(0, count));
        other.count = count;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return items.GetEnumerator();
    }
}
