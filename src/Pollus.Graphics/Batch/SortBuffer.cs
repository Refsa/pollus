namespace Pollus.Graphics;

public class SortBuffer
{
    public struct Entry : IComparable<Entry>
    {
        public uint SortKey;
        public int InstanceIndex;

        public int CompareTo(Entry other) => SortKey.CompareTo(other.SortKey);
    }

    const int RadixSortThreshold = 256;

    Entry[] scratch = [];
    Entry[] entries;
    int count;

    public int Count => count;
    public Span<Entry> Entries => entries.AsSpan(0, count);

    public SortBuffer(int initialCapacity = 16)
    {
        entries = new Entry[initialCapacity];
    }

    public void Add(uint sortKey, int instanceIndex)
    {
        if (count == entries.Length) Resize(count * 2);
        entries[count++] = new Entry { SortKey = sortKey, InstanceIndex = instanceIndex };
    }

    public bool Sort()
    {
        if (count < 2) return false;

        var entries = Entries;

        var isMonotonic = true;
        for (int i = 1; i < entries.Length; i++)
        {
            if (entries[i - 1].SortKey <= entries[i].SortKey) continue;
            isMonotonic = false;
            break;
        }

        if (isMonotonic) return false;

        if (count < RadixSortThreshold) entries.Sort();
        else RadixSort();

        return true;
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

    void RadixSort()
    {
        if (scratch.Length < Count) scratch = new Entry[entries.Length];

        var source = entries.AsSpan(0, count);
        var destination = scratch.AsSpan(0, count);

        Span<int> offsets = stackalloc int[256];

        for (int shift = 0; shift < 32; shift += 8)
        {
            offsets.Clear();

            foreach (ref readonly var entry in source)
            {
                var bucket = (int)((entry.SortKey >> shift) & 0xFF);
                offsets[bucket]++;
            }

            var total = 0;

            for (int bucket = 0; bucket < offsets.Length; bucket++)
            {
                var bucketCount = offsets[bucket];
                offsets[bucket] = total;
                total += bucketCount;
            }

            foreach (ref readonly var entry in source)
            {
                var bucket = (int)((entry.SortKey >> shift) & 0xFF);
                destination[offsets[bucket]++] = entry;
            }

            var temporary = source;
            source = destination;
            destination = temporary;
        }
    }
}
