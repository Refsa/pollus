namespace Pollus.Tests.Collections;

using Pollus.Collections;

public class MinHeapTests
{
    [Fact]
    public void Pop_ReturnsMinimum()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(5);
        heap.Push(1);
        heap.Push(3);

        Assert.Equal(1, heap.Pop());
    }

    [Fact]
    public void Pop_ReturnsSortedOrder()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(5);
        heap.Push(1);
        heap.Push(4);
        heap.Push(2);
        heap.Push(3);

        Assert.Equal(1, heap.Pop());
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(4, heap.Pop());
        Assert.Equal(5, heap.Pop());
    }

    [Fact]
    public void Pop_Empty_Throws()
    {
        var heap = new MinHeap<int>(4);
        Assert.Throws<InvalidOperationException>(() => heap.Pop());
    }

    [Fact]
    public void TryPop_Empty_ReturnsFalse()
    {
        var heap = new MinHeap<int>(4);
        Assert.False(heap.TryPop(out _));
    }

    [Fact]
    public void TryPop_NonEmpty_ReturnsTrueWithMin()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(3);
        heap.Push(1);

        Assert.True(heap.TryPop(out var value));
        Assert.Equal(1, value);
    }

    [Fact]
    public void Count_TracksSize()
    {
        var heap = new MinHeap<int>(4);
        Assert.Equal(0, heap.Count);

        heap.Push(10);
        heap.Push(20);
        Assert.Equal(2, heap.Count);

        heap.Pop();
        Assert.Equal(1, heap.Count);
    }

    [Fact]
    public void IsEmpty_HasFree_Reflect_State()
    {
        var heap = new MinHeap<int>(4);
        Assert.True(heap.IsEmpty);
        Assert.False(heap.HasFree);

        heap.Push(1);
        Assert.False(heap.IsEmpty);
        Assert.True(heap.HasFree);

        heap.Pop();
        Assert.True(heap.IsEmpty);
        Assert.False(heap.HasFree);
    }

    [Fact]
    public void Clear_ResetsHeap()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(1);
        heap.Push(2);
        heap.Push(3);

        heap.Clear();
        Assert.True(heap.IsEmpty);
        Assert.Equal(0, heap.Count);
        Assert.False(heap.TryPop(out _));
    }

    [Fact]
    public void Push_AfterClear_Works()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(10);
        heap.Push(20);
        heap.Clear();

        heap.Push(5);
        heap.Push(3);
        Assert.Equal(3, heap.Pop());
        Assert.Equal(5, heap.Pop());
    }

    [Fact]
    public void Push_BeyondInitialCapacity_Grows()
    {
        var heap = new MinHeap<int>(2);
        for (int i = 100; i > 0; i--)
            heap.Push(i);

        Assert.Equal(100, heap.Count);
        for (int i = 1; i <= 100; i++)
            Assert.Equal(i, heap.Pop());
    }

    [Fact]
    public void Push_Duplicates_AllReturned()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(3);
        heap.Push(3);
        heap.Push(1);
        heap.Push(3);

        Assert.Equal(1, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(3, heap.Pop());
    }

    [Fact]
    public void Push_SingleElement_PopReturnsIt()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(42);
        Assert.Equal(42, heap.Pop());
        Assert.True(heap.IsEmpty);
    }

    [Fact]
    public void Push_AlreadySorted_PopReturnsSorted()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(1);
        heap.Push(2);
        heap.Push(3);
        heap.Push(4);

        Assert.Equal(1, heap.Pop());
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(4, heap.Pop());
    }

    [Fact]
    public void Push_ReverseSorted_PopReturnsSorted()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(4);
        heap.Push(3);
        heap.Push(2);
        heap.Push(1);

        Assert.Equal(1, heap.Pop());
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(4, heap.Pop());
    }

    [Fact]
    public void InterleavedPushPop_MaintainsHeapProperty()
    {
        var heap = new MinHeap<int>(4);
        heap.Push(5);
        heap.Push(3);
        Assert.Equal(3, heap.Pop());

        heap.Push(1);
        heap.Push(4);
        Assert.Equal(1, heap.Pop());
        Assert.Equal(4, heap.Pop());
        Assert.Equal(5, heap.Pop());
        Assert.True(heap.IsEmpty);
    }

    [Fact]
    public void ConcurrentPush_AllValuesPresent()
    {
        var heap = new MinHeap<int>(4);
        const int threadsCount = 4;
        const int itemsPerThread = 250;
        var barrier = new Barrier(threadsCount);

        var threads = new Thread[threadsCount];
        for (int t = 0; t < threadsCount; t++)
        {
            int threadId = t;
            threads[t] = new Thread(() =>
            {
                barrier.SignalAndWait();
                int start = threadId * itemsPerThread;
                for (int i = start; i < start + itemsPerThread; i++)
                    heap.Push(i);
            });
            threads[t].Start();
        }

        foreach (var thread in threads) thread.Join();

        Assert.Equal(threadsCount * itemsPerThread, heap.Count);

        var results = new List<int>();
        while (heap.TryPop(out var val))
            results.Add(val);

        Assert.Equal(threadsCount * itemsPerThread, results.Count);
        for (int i = 1; i < results.Count; i++)
            Assert.True(results[i - 1] <= results[i], $"Out of order: {results[i - 1]} > {results[i]}");

        var expected = Enumerable.Range(0, threadsCount * itemsPerThread).ToHashSet();
        Assert.Equal(expected, results.ToHashSet());
    }

    [Fact]
    public void ConcurrentPop_AllValuesConsumed()
    {
        var heap = new MinHeap<int>(4);
        const int totalItems = 1000;
        for (int i = 0; i < totalItems; i++)
            heap.Push(i);

        const int threadsCount = 4;
        var barrier = new Barrier(threadsCount);
        var consumed = new System.Collections.Concurrent.ConcurrentBag<int>();

        var threads = new Thread[threadsCount];
        for (int t = 0; t < threadsCount; t++)
        {
            threads[t] = new Thread(() =>
            {
                barrier.SignalAndWait();
                while (heap.TryPop(out var val))
                    consumed.Add(val);
            });
            threads[t].Start();
        }

        foreach (var thread in threads) thread.Join();

        Assert.True(heap.IsEmpty);
        Assert.Equal(totalItems, consumed.Count);

        var expected = Enumerable.Range(0, totalItems).ToHashSet();
        Assert.Equal(expected, consumed.ToHashSet());
    }

    [Fact]
    public void ConcurrentPushAndPop_NoCorruption()
    {
        var heap = new MinHeap<int>(4);
        const int itemsPerThread = 500;
        const int producerCount = 2;
        const int consumerCount = 2;
        var barrier = new Barrier(producerCount + consumerCount);
        var consumed = new System.Collections.Concurrent.ConcurrentBag<int>();
        var done = new ManualResetEventSlim(false);

        var producers = new Thread[producerCount];
        for (int t = 0; t < producerCount; t++)
        {
            int threadId = t;
            producers[t] = new Thread(() =>
            {
                barrier.SignalAndWait();
                int start = threadId * itemsPerThread;
                for (int i = start; i < start + itemsPerThread; i++)
                    heap.Push(i);
            });
            producers[t].Start();
        }

        var consumers = new Thread[consumerCount];
        for (int t = 0; t < consumerCount; t++)
        {
            consumers[t] = new Thread(() =>
            {
                barrier.SignalAndWait();
                while (!done.IsSet || heap.HasFree)
                {
                    if (heap.TryPop(out var val))
                        consumed.Add(val);
                }
            });
            consumers[t].Start();
        }

        foreach (var p in producers) p.Join();
        done.Set();
        foreach (var c in consumers) c.Join();

        while (heap.TryPop(out var remaining))
            consumed.Add(remaining);

        int totalItems = producerCount * itemsPerThread;
        Assert.Equal(totalItems, consumed.Count);

        var expected = Enumerable.Range(0, totalItems).ToHashSet();
        Assert.Equal(expected, consumed.ToHashSet());
    }
}
