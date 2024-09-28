namespace Pollus.ECS;

using System.Runtime.CompilerServices;


public partial record struct Entity(int ID, int Version = 0)
{
    public static readonly Entity NULL = new Entity(0, -1);

    public bool IsNull => ID <= 0 && Version < 0;
    public override int GetHashCode() => ID;
    public override string ToString() => $"Entity({ID}, {Version})";
}

public class Entities
{
    public struct EntityInfo
    {
        public bool IsAlive = false;
        public Entity Entity = Entity.NULL;
        public int ArchetypeIndex = -1;
        public int ChunkIndex = -1;
        public int RowIndex = -1;

        public EntityInfo() { }
    }

    volatile int counter = -1;
    int aliveCount = 0;
    EntityInfo[] entities = new EntityInfo[64];
    MinHeap freeList = new();

    public int AliveCount => Volatile.Read(ref aliveCount);

    public Entity Create()
    {
        var entityId = freeList.Pop();
        ref var entityInfo = ref Unsafe.NullRef<EntityInfo>();
        
        if (entityId == -1) entityInfo = ref NewEntity();
        else entityInfo = ref entities[entityId];

        entityInfo.Entity.Version++;
        entityInfo.IsAlive = true;
        Interlocked.Increment(ref aliveCount);

        return entityInfo.Entity;
    }

    public void Create(in Span<Entity> target)
    {
        for (int i = 0; i < target.Length; i++)
        {
            target[i] = Create();
        }
        Interlocked.Add(ref aliveCount, target.Length);
    }

    public void Free(in Entity entity)
    {
        ref var entityInfo = ref GetEntityInfo(entity);
        entityInfo.IsAlive = false;
        freeList.Push(entity.ID);
        Interlocked.Decrement(ref aliveCount);
    }

    public bool IsAlive(in Entity entity)
    {
        return Volatile.Read(ref aliveCount) > 0 && entities[entity.ID].IsAlive;
    }

    public ref EntityInfo GetEntityInfo(in Entity entity)
    {
        return ref entities[entity.ID];
    }

    ref EntityInfo NewEntity()
    {
        var id = Interlocked.Increment(ref counter);
        if (id >= entities.Length) Array.Resize(ref entities, id * 2);
        ref var entityInfo = ref entities[id];

        entityInfo.Entity.ID = id;
        entityInfo.Entity.Version = -1;

        return ref entityInfo;
    }
}

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