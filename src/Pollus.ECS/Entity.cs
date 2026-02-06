namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;

public partial record struct Entity(int ID, int Version = 0)
{
    public static readonly Entity Null = new Entity(0, -1);

    public bool IsNull => ID <= 0 && Version < 0;
    public override int GetHashCode() => ID;
    public override string ToString() => $"Entity({ID}, {Version})";
}

public class Entities
{
    public struct EntityInfo
    {
        public bool IsAlive = false;
        public Entity Entity = Entity.Null;
        public int ArchetypeIndex = -1;
        public int ChunkIndex = -1;
        public int RowIndex = -1;

        public EntityInfo() { }
    }

    volatile int counter = -1;
    int aliveCount = 0;
    EntityInfo[] entities = new EntityInfo[64];
    MinHeap<int> freeList = new();

    public int AliveCount => Volatile.Read(ref aliveCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Create()
    {
        ref var entityInfo = ref Unsafe.NullRef<EntityInfo>();

        if (freeList.TryPop(out var entityId)) entityInfo = ref entities[entityId];
        else entityInfo = ref NewEntity();

        entityInfo.Entity.Version++;
        entityInfo.IsAlive = true;
        Interlocked.Increment(ref aliveCount);

        return entityInfo.Entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Create(in Span<Entity> target)
    {
        for (int i = 0; i < target.Length; i++)
        {
            target[i] = Create();
        }
    }

    public void Free(in Entity entity)
    {
        ref var entityInfo = ref GetEntityInfo(entity);
        entityInfo.IsAlive = false;
        freeList.Push(entity.ID);
        Interlocked.Decrement(ref aliveCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool IsAlive(in Entity entity)
    {
        ref readonly var otherEntity = ref entities[entity.ID];
        return Volatile.Read(ref aliveCount) > 0 && otherEntity.IsAlive && entity.Version == otherEntity.Entity.Version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

    public void Append(ReadOnlySpan<EntityInfo> insert)
    {
        if (insert.Length == 0) return;

        for (int i = 0; i < insert.Length; i++)
        {
            var entity = insert[i];
            if (entity.Entity.ID >= entities.Length) Array.Resize(ref entities, entity.Entity.ID * 2);
            entities[entity.Entity.ID] = entity;
        }
    }

    public void Append(scoped in EntityInfo insert)
    {
        if (insert.Entity.ID >= entities.Length) Array.Resize(ref entities, insert.Entity.ID * 2);
        entities[insert.Entity.ID] = insert;
    }

    /// <summary>
    /// Repopulate free list and ensure ID is set on all entities
    /// </summary>
    public void Recalcuate()
    {
        freeList.Clear();
        for (int i = 0; i < entities.Length; i++)
        {
            entities[i].Entity.ID = i;
            if (!entities[i].IsAlive)
            {
                freeList.Push(i);
            }
        }
    }
}