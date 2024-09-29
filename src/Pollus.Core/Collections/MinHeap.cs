namespace Pollus.Collections;

using System.Runtime.CompilerServices;

public class MinHeap
{
    int[] heap;
    int size;
    SpinWait spin = new();

    public MinHeap(int initialCapacity = 1024)
    {
        heap = new int[initialCapacity];
        size = 0;
    }

    public bool HasFree => Volatile.Read(ref size) > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public int Pop()
    {
        while (true)
        {
            int currentSize = Volatile.Read(ref size);
            if (currentSize == 0) return -1;

            int minEntity = heap[0];
            int lastEntity = heap[currentSize - 1];

            if (Interlocked.CompareExchange(ref size, currentSize - 1, currentSize) != currentSize)
            {
                spin.SpinOnce();
                continue;
            }

            if (currentSize > 1)
            {
                heap[0] = lastEntity;
                HeapifyDown(0, currentSize - 1);
            }

            return minEntity;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Push(int id)
    {
        while (true)
        {
            int currentSize = Volatile.Read(ref size);
            if (currentSize >= heap.Length)
            {
                GrowHeap();
                continue;
            }

            heap[currentSize] = id;
            if (Interlocked.CompareExchange(ref size, currentSize + 1, currentSize) == currentSize)
            {
                HeapifyUp(currentSize);
                break;
            }

            spin.SpinOnce();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void GrowHeap()
    {
        var newHeap = new int[heap.Length * 2];
        Array.Copy(heap, newHeap, heap.Length);
        Interlocked.CompareExchange(ref heap, newHeap, heap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void HeapifyUp(int index)
    {
        int newEntity = heap[index];
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            int parent = heap[parentIndex];
            if (parent <= newEntity)
                break;
            heap[index] = parent;
            index = parentIndex;
        }
        heap[index] = newEntity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void HeapifyDown(int index, int heapSize)
    {
        int topEntity = heap[index];
        while (true)
        {
            int leftChild = 2 * index + 1;
            if (leftChild >= heapSize)
                break;

            int rightChild = leftChild + 1;
            int minChild = (rightChild < heapSize && heap[rightChild] < heap[leftChild]) ? rightChild : leftChild;

            if (topEntity <= heap[minChild])
                break;

            heap[index] = heap[minChild];
            index = minChild;
        }
        heap[index] = topEntity;
    }
}