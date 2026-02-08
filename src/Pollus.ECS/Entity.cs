namespace Pollus.ECS;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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

    int counter = -1;
    int aliveCount = 0;
    EntityInfo[] entities = new EntityInfo[64];
    ConcurrentStack<Entity> freeList = new();

    public int AliveCount => aliveCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity ReserveId()
    {
        if (freeList.TryPop(out var recycled))
            return new Entity(recycled.ID, recycled.Version + 1);

        var id = Interlocked.Increment(ref counter);
        return new Entity(id, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Activate(in Entity entity)
    {
        if (entity.ID >= entities.Length)
            Array.Resize(ref entities, (entity.ID + 1) * 2);

        ref var info = ref entities[entity.ID];
        info.Entity = entity;
        info.IsAlive = true;
        aliveCount++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity Create()
    {
        var entity = ReserveId();
        Activate(entity);
        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        freeList.Push(entity);
        aliveCount--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive(in Entity entity)
    {
        ref readonly var otherEntity = ref entities[entity.ID];
        return aliveCount > 0 && otherEntity.IsAlive && entity.Version == otherEntity.Entity.Version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref EntityInfo GetEntityInfo(in Entity entity)
    {
        return ref entities[entity.ID];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                freeList.Push(entities[i].Entity);
            }
        }
    }
}