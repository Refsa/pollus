namespace Pollus.Graphics;

public class SortBuffer
{
    public struct Entry : IComparable<Entry>
    {
        public ulong SortKey;
        public int InstanceIndex;

        public int CompareTo(Entry other) => SortKey.CompareTo(other.SortKey);
    }

    Entry[] entries;
    int count;

    public int Count => count;
    public Span<Entry> Entries => entries.AsSpan(0, count);

    public SortBuffer(int initialCapacity = 16)
    {
        entries = new Entry[initialCapacity];
    }

    public void Add(ulong sortKey, int instanceIndex)
    {
        if (count == entries.Length) Resize(count * 2);
        entries[count++] = new Entry { SortKey = sortKey, InstanceIndex = instanceIndex };
    }

    public void Sort()
    {
        Entries.Sort();
    }

    public void Clear()
    {
        count = 0;
    }

    void Resize(int capacity)
    {
        var next = new Entry[capacity];
        entries.AsSpan(0, count).CopyTo(next);
        entries = next;
    }
}
