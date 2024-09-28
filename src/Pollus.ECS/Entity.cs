using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public partial record struct Entity(int ID, int Version = 0)
{
    public static readonly Entity NULL = new Entity(0, -1);

    public bool IsNull => ID <= 0 && Version < 0;
    public override int GetHashCode() => ID;
    public override string ToString() => $"Entity({ID}, {Version})";
}

public class Entities
{
    volatile int counter = -1;
    EntityFreeList freeList = new();

    public Entity Create()
    {
        var entity = freeList.Pop();
        if (entity.IsNull) entity = new Entity(Interlocked.Increment(ref counter), 0);

        return entity;
    }

    public void Create(in Span<Entity> entities)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = freeList.Pop();
            if (entity.IsNull) entity = new Entity(Interlocked.Increment(ref counter), 0);
            entities[i] = entity;
        }
    }

    public void Free(in Entity entity)
    {
        freeList.Push(entity);
    }
}

public class EntityFreeList
{
    Entity[] heap;
    int size;
    SpinWait spin = new SpinWait();

    public EntityFreeList(int initialCapacity = 1024)
    {
        heap = new Entity[initialCapacity];
        size = 0;
    }

    public bool HasFree => Volatile.Read(ref size) > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Pop()
    {
        while (true)
        {
            int currentSize = Volatile.Read(ref size);
            if (currentSize == 0)
                return Entity.NULL;

            Entity minEntity = heap[0];
            Entity lastEntity = heap[currentSize - 1];

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

            minEntity.Version++;
            return minEntity;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Push(in Entity entity)
    {
        while (true)
        {
            int currentSize = Volatile.Read(ref size);
            if (currentSize >= heap.Length)
            {
                GrowHeap();
                continue;
            }

            heap[currentSize] = entity;
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
        var newHeap = new Entity[heap.Length * 2];
        Array.Copy(heap, newHeap, heap.Length);
        Interlocked.CompareExchange(ref heap, newHeap, heap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void HeapifyUp(int index)
    {
        Entity newEntity = heap[index];
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            Entity parent = heap[parentIndex];
            if (parent.ID <= newEntity.ID)
                break;
            heap[index] = parent;
            index = parentIndex;
        }
        heap[index] = newEntity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    void HeapifyDown(int index, int heapSize)
    {
        Entity topEntity = heap[index];
        while (true)
        {
            int leftChild = 2 * index + 1;
            if (leftChild >= heapSize)
                break;

            int rightChild = leftChild + 1;
            int minChild = (rightChild < heapSize && heap[rightChild].ID < heap[leftChild].ID) ? rightChild : leftChild;

            if (topEntity.ID <= heap[minChild].ID)
                break;

            heap[index] = heap[minChild];
            index = minChild;
        }
        heap[index] = topEntity;
    }
}