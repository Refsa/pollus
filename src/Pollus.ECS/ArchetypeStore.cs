namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Debugging;

public class ArchetypeStore : IDisposable
{
    public record struct EntityInfo
    {
        public int ArchetypeIndex { get; set; }
        public int ChunkIndex { get; set; }
        public int RowIndex { get; set; }
    }

    public struct EntityChange
    {
        public Entity Entity { get; set; }
        public Archetype Archetype { get; set; }
        public int ChunkIndex { get; set; }
        public int RowIndex { get; set; }
    }

    ulong version = 0;

    readonly List<Archetype> archetypes;
    readonly ComponentChanges changes;
    NativeMap<ArchetypeID, int> archetypeLookup;
    NativeMap<Entity, EntityInfo> entities;
    volatile int entityCounter = 1;

    public List<Archetype> Archetypes => archetypes;
    public int EntityCount => entities.Count;
    public ComponentChanges Changes => changes;

    public ArchetypeStore()
    {
        NativeMap<Entity, EntityInfo>.Sentinel = Entity.NULL;

        entities = new(0);
        archetypeLookup = new(0);
        archetypes = [];
        changes = new();

        var aid = ArchetypeID.Create([]);
        archetypes.Add(new Archetype(aid, []));
        archetypeLookup.Add(aid, 0);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var archetype in archetypes)
        {
            archetype.Dispose();
        }
        entities.Dispose();
        archetypeLookup.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public (Archetype archetype, int index)? GetArchetype(in ArchetypeID aid)
    {
        if (archetypeLookup.TryGetValue(aid, out var index))
        {
            return (archetypes[index], index);
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Archetype GetArchetype(int index)
    {
        return archetypes[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public (Archetype archetype, int index) GetOrCreateArchetype(in ArchetypeID aid, scoped in Span<ComponentID> cids)
    {
        if (archetypeLookup.TryGetValue(aid, out var index))
        {
            return (archetypes[index], index);
        };

        var archetype = new Archetype(aid, cids);
        archetype.Tick(version);
        archetypes.Add(archetype);
        archetypeLookup.Add(aid, archetypes.Count - 1);
        return (archetype, archetypes.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity CreateEntity()
    {
        var entity = new Entity(entityCounter++);
        var archetypeInfo = archetypes[0].AddEntity(entity);
        entities.Add(entity, new EntityInfo { ArchetypeIndex = 0, ChunkIndex = archetypeInfo.ChunkIndex, RowIndex = archetypeInfo.RowIndex });
        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityChange CreateEntity<TBuilder>()
        where TBuilder : struct, IEntityBuilder
    {
        var entity = new Entity(entityCounter++);
        var (archetype, archetypeIndex) = GetOrCreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        var archetypeInfo = archetype.AddEntity(entity);
        archetype.Chunks[archetypeInfo.ChunkIndex].SetAllFlags(archetypeInfo.RowIndex, ComponentFlags.Added);

        entities.Add(entity, new()
        {
            ArchetypeIndex = archetypeIndex,
            ChunkIndex = archetypeInfo.ChunkIndex,
            RowIndex = archetypeInfo.RowIndex
        });

        return new()
        {
            Entity = entity,
            Archetype = archetype,
            ChunkIndex = archetypeInfo.ChunkIndex,
            RowIndex = archetypeInfo.RowIndex
        };
    }

    public void DestroyEntity(in Entity entity)
    {
        if (entities.TryGetValue(entity, out var info))
        {
            entities.Remove(entity);
            var archetype = archetypes[info.ArchetypeIndex];
            var movedEntity = archetype.RemoveEntity(new()
            {
                Entity = entity,
                ChunkIndex = info.ChunkIndex,
                RowIndex = info.RowIndex
            });

            if (movedEntity != Entity.NULL)
            {
                ref var movedEntityInfo = ref entities.Get(movedEntity);
                Guard.IsFalse(Unsafe.IsNullRef(ref movedEntityInfo), "Moved entity is null");

                movedEntityInfo.ChunkIndex = info.ChunkIndex;
                movedEntityInfo.RowIndex = info.RowIndex;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref C GetComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        if (entities.TryGetValue(entity, out var info))
        {
            var archetype = archetypes[info.ArchetypeIndex];
            if (!archetype.HasComponent<C>()) throw new ArgumentException("Entity does not have component");
            return ref archetype.GetComponent<C>(info.ChunkIndex, info.RowIndex);
        }

        throw new ArgumentException("Entity does not exist");
    }

    public void AddComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        if (!entities.TryGetValue(entity, out var info))
        {
            throw new ArgumentException("Entity does not exist");
        }

        var archetype = archetypes[info.ArchetypeIndex];
        if (archetype.HasComponent<C>()) return;

        Span<ComponentID> cids = stackalloc ComponentID[archetype.GetChunkInfo().ComponentIDs.Length + 1];
        archetype.GetChunkInfo().ComponentIDs.CopyTo(cids);
        cids[^1] = Component.GetInfo<C>().ID;

        var nextAid = ArchetypeID.Create(cids);
        var (nextArchetype, nextArchetypeIndex) = GetOrCreateArchetype(nextAid, cids);
        var nextArchetypeInfo = nextArchetype.AddEntity(entity);

        ref var nextInfo = ref entities.Get(entity);
        nextInfo.ArchetypeIndex = nextArchetypeIndex;
        nextInfo.ChunkIndex = nextArchetypeInfo.ChunkIndex;
        nextInfo.RowIndex = nextArchetypeInfo.RowIndex;

        nextArchetype.SetComponent(nextArchetypeInfo.ChunkIndex, nextArchetypeInfo.RowIndex, component);

        var movedEntity = archetype.MoveEntity(new() { ChunkIndex = info.ChunkIndex, RowIndex = info.RowIndex }, nextArchetype, nextArchetypeInfo);
        if (movedEntity != Entity.NULL)
        {
            ref var movedEntityInfo = ref entities.Get(movedEntity);
            movedEntityInfo.ChunkIndex = nextInfo.ChunkIndex;
            movedEntityInfo.RowIndex = nextInfo.RowIndex;
        }

        nextArchetype.Chunks[nextInfo.ChunkIndex].SetFlag<C>(info.RowIndex, ComponentFlags.Added);
    }

    public void RemoveComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!entities.TryGetValue(entity, out var info))
        {
            throw new ArgumentException("Entity does not exist");
        }

        var archetype = archetypes[info.ArchetypeIndex];
        if (!archetype.HasComponent<C>()) return;

        Span<ComponentID> cids = stackalloc ComponentID[archetype.GetChunkInfo().ComponentIDs.Length - 1];
        var index = 0;
        foreach (var cid in archetype.GetChunkInfo().ComponentIDs)
        {
            if (cid != Component.GetInfo<C>().ID) cids[index++] = cid;
        }

        var nextAid = ArchetypeID.Create(cids);
        var (nextArchetype, nextArchetypeIndex) = GetOrCreateArchetype(nextAid, cids);
        var nextArchetypeInfo = nextArchetype.AddEntity(entity);

        ref var nextInfo = ref entities.Get(entity);
        nextInfo.ArchetypeIndex = nextArchetypeIndex;
        nextInfo.ChunkIndex = nextArchetypeInfo.ChunkIndex;
        nextInfo.RowIndex = nextArchetypeInfo.RowIndex;

        var movedEntity = archetype.MoveEntity(new() { ChunkIndex = info.ChunkIndex, RowIndex = info.RowIndex }, nextArchetype, nextArchetypeInfo);
        if (movedEntity != Entity.NULL)
        {
            ref var movedEntityInfo = ref entities.Get(movedEntity);
            movedEntityInfo.ChunkIndex = nextInfo.ChunkIndex;
            movedEntityInfo.RowIndex = nextInfo.RowIndex;
        }

        changes.SetRemoved<C>(entity);
        nextArchetype.Chunks[nextInfo.ChunkIndex].SetFlag<C>(nextInfo.RowIndex, ComponentFlags.Removed);
    }

    public void SetComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        if (!entities.TryGetValue(entity, out var info))
        {
            throw new ArgumentException("Entity does not exist");
        }

        var archetype = archetypes[info.ArchetypeIndex];
        archetype.SetComponent(info.ChunkIndex, info.RowIndex, component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool EntityExists(in Entity entity)
    {
        return entities.Has(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityInfo GetEntityInfo(in Entity entity)
    {
        return entities.Get(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Tick(ulong version)
    {
        this.version = version;
        changes.Tick(version);
        foreach (var archetype in archetypes)
        {
            archetype.Tick(version);
        }
    }
}
