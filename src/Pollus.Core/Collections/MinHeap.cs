namespace Pollus.Collections;

using System.Numerics;
using System.Runtime.CompilerServices;

public class MinHeap<T>
    where T : INumber<T>
{
    SpinLock spin = new(enableThreadOwnerTracking: false);
    T[] heap;
    int size;

    public int Count { get { bool taken = false; spin.Enter(ref taken); var c = size; spin.Exit(); return c; } }
    public bool HasFree { get { bool taken = false; spin.Enter(ref taken); var r = size > 0; spin.Exit(); return r; } }
    public bool IsEmpty { get { bool taken = false; spin.Enter(ref taken); var r = size == 0; spin.Exit(); return r; } }

    public MinHeap(int initialCapacity = 16)
    {
        heap = new T[initialCapacity];
        size = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPop(out T value)
    {
        bool taken = false;
        spin.Enter(ref taken);

        if (size == 0)
        {
            spin.Exit();
            Unsafe.SkipInit(out value);
            return false;
        }

        value = PopCore();
        spin.Exit();
        return true;
    }

    public T Pop()
    {
        bool taken = false;
        spin.Enter(ref taken);

        if (size == 0)
        {
            spin.Exit();
            throw new InvalidOperationException("Heap is empty");
        }

        var value = PopCore();
        spin.Exit();
        return value;
    }

    public void Push(T value)
    {
        bool taken = false;
        spin.Enter(ref taken);

        if (size >= heap.Length)
            Array.Resize(ref heap, heap.Length * 2);

        heap[size] = value;
        SiftUp(size);
        size++;

        spin.Exit();
    }

    public void Clear()
    {
        bool taken = false;
        spin.Enter(ref taken);
        size = 0;
        spin.Exit();
    }

    T PopCore()
    {
        var min = heap[0];
        size--;

        if (size > 0)
        {
            heap[0] = heap[size];
            SiftDown(0);
        }

        return min;
    }

    void SiftUp(int index)
    {
        T item = heap[index];
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (heap[parent] <= item) break;
            heap[index] = heap[parent];
            index = parent;
        }
        heap[index] = item;
    }

    void SiftDown(int index)
    {
        T item = heap[index];
        while (true)
        {
            int left = 2 * index + 1;
            if (left >= size) break;
            int right = left + 1;
            int min = (right < size && heap[right] < heap[left]) ? right : left;
            if (item <= heap[min]) break;
            heap[index] = heap[min];
            index = min;
        }
        heap[index] = item;
    }
}
