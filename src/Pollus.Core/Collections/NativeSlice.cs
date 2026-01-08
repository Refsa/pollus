namespace Pollus.Collections;

unsafe public struct NativeSlice<T>
    where T : unmanaged
{
    T* data;
    int length;

    public readonly int Length => length;
    public Span<T> AsSpan() => new(data, length);

    public ref T this[int index] => ref data[index];

    public NativeSlice(T* from, int length)
    {
        data = from;
        this.length = length;
    }

    public Enumerator GetEnumerator() => new(data, length);

    public struct Enumerator
    {
        T* data;
        int index;
        int length;

        public ref T Current => ref data[index];

        public Enumerator(T* data, int length)
        {
            this.data = data;
            this.length = length;
            index = -1;
        }

        public bool MoveNext()
        {
            index++;
            return index < length;
        }
    }
}