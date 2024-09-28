namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Debugging;

public class ArchetypeStore : IDisposable
{
    public record struct EntityChange(Entity Entity, Archetype Archetype, int ChunkIndex, int RowIndex);
    public record struct ArchetypeInfo(Archetype Archetype, int Index);

    ulong version = 0;

    readonly List<Archetype> archetypes;
    readonly RemovedTracker changes;
    NativeMap<ArchetypeID, int> archetypeLookup;
    Entities entityHandler;

    public List<Archetype> Archetypes => archetypes;
    public int EntityCount => entityHandler.AliveCount;
    public RemovedTracker Changes => changes;

    internal Entities Entities => entityHandler;

    public ArchetypeStore()
    {
        archetypeLookup = new(0);
        archetypes = [];
        changes = new();
        entityHandler = new();

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
    public ArchetypeInfo GetOrCreateArchetype(in ArchetypeID aid, scoped in Span<ComponentID> cids)
    {
        if (archetypeLookup.TryGetValue(aid, out var index))
            return new(archetypes[index], index);

        var archetype = new Archetype(aid, cids);
        archetype.Tick(version);
        archetypes.Add(archetype);
        archetypeLookup.Add(aid, archetypes.Count - 1);
        return new(archetype, archetypes.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity CreateEntity()
    {
        var entity = entityHandler.Create();
        var archetypeInfo = archetypes[0].AddEntity(entity);
        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        entityInfo.ArchetypeIndex = 0;
        entityInfo.ChunkIndex = archetypeInfo.ChunkIndex;
        entityInfo.RowIndex = archetypeInfo.RowIndex;
        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityChange CreateEntity<TBuilder>()
        where TBuilder : struct, IEntityBuilder
    {
        var entity = entityHandler.Create();
        return InsertEntity<TBuilder>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityChange InsertEntity<TBuilder>(in Entity entity)
        where TBuilder : struct, IEntityBuilder
    {
        var archetypeInfo = GetOrCreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        var archetypeEntityInfo = archetypeInfo.Archetype.AddEntity(entity);
        archetypeInfo.Archetype.Chunks[archetypeEntityInfo.ChunkIndex].SetAllFlags(archetypeEntityInfo.RowIndex, ComponentFlags.Added);

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        entityInfo.ArchetypeIndex = archetypeInfo.Index;
        entityInfo.ChunkIndex = archetypeEntityInfo.ChunkIndex;
        entityInfo.RowIndex = archetypeEntityInfo.RowIndex;
        return new(entity, archetypeInfo.Archetype, archetypeEntityInfo.ChunkIndex, archetypeEntityInfo.RowIndex);
    }

    public void DestroyEntity(in Entity entity)
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        entityHandler.Free(entity);

        var archetype = archetypes[entityInfo.ArchetypeIndex];
        var movedEntity = archetype.RemoveEntity(entityInfo.ChunkIndex, entityInfo.RowIndex);

        if (!movedEntity.IsNull)
        {
            ref var movedEntityInfo = ref entityHandler.GetEntityInfo(movedEntity);
            Guard.IsFalse(Unsafe.IsNullRef(ref movedEntityInfo), "Moved entity is null");

            movedEntityInfo.ChunkIndex = entityInfo.ChunkIndex;
            movedEntityInfo.RowIndex = entityInfo.RowIndex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool HasComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }
        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        return archetypes[entityInfo.ArchetypeIndex].HasComponent<C>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ref C GetComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        var archetype = archetypes[entityInfo.ArchetypeIndex];
        if (!archetype.HasComponent<C>()) throw new ArgumentException("Entity does not have component");
        return ref archetype.GetComponent<C>(entityInfo.ChunkIndex, entityInfo.RowIndex);
    }

    public void AddComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }

        var prevEntityInfo = entityHandler.GetEntityInfo(entity);
        var archetype = archetypes[prevEntityInfo.ArchetypeIndex];
        if (archetype.HasComponent<C>()) return;

        Span<ComponentID> cids = stackalloc ComponentID[archetype.GetChunkInfo().ComponentIDs.Length + 1];
        archetype.GetChunkInfo().ComponentIDs.CopyTo(cids);
        cids[^1] = Component.GetInfo<C>().ID;

        var nextAid = ArchetypeID.Create(cids);
        var (nextArchetype, nextArchetypeIndex) = GetOrCreateArchetype(nextAid, cids);
        var nextArchetypeInfo = nextArchetype.AddEntity(entity);

        ref var nextInfo = ref entityHandler.GetEntityInfo(entity);
        nextInfo.ArchetypeIndex = nextArchetypeIndex;
        nextInfo.ChunkIndex = nextArchetypeInfo.ChunkIndex;
        nextInfo.RowIndex = nextArchetypeInfo.RowIndex;

        nextArchetype.SetComponent(nextArchetypeInfo.ChunkIndex, nextArchetypeInfo.RowIndex, component);

        var movedEntity = archetype.MoveEntity(prevEntityInfo.ChunkIndex, prevEntityInfo.RowIndex, nextArchetype, nextArchetypeInfo.ChunkIndex, nextArchetypeInfo.RowIndex);
        if (!movedEntity.IsNull)
        {
            ref var movedEntityInfo = ref entityHandler.GetEntityInfo(movedEntity);
            movedEntityInfo.ChunkIndex = nextInfo.ChunkIndex;
            movedEntityInfo.RowIndex = nextInfo.RowIndex;
        }

        nextArchetype.Chunks[nextInfo.ChunkIndex].SetFlag<C>(nextInfo.RowIndex, ComponentFlags.Added);
    }

    public void RemoveComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }

        var prevEntityInfo = entityHandler.GetEntityInfo(entity);
        var archetype = archetypes[prevEntityInfo.ArchetypeIndex];
        if (!archetype.HasComponent<C>()) return;

        var component = archetype.GetChunk(prevEntityInfo.ChunkIndex).GetComponent<C>(prevEntityInfo.RowIndex);

        Span<ComponentID> cids = stackalloc ComponentID[archetype.GetChunkInfo().ComponentIDs.Length - 1];
        var index = 0;
        foreach (var cid in archetype.GetChunkInfo().ComponentIDs)
        {
            if (cid != Component.GetInfo<C>().ID) cids[index++] = cid;
        }

        var nextAid = ArchetypeID.Create(cids);
        var (nextArchetype, nextArchetypeIndex) = GetOrCreateArchetype(nextAid, cids);
        var nextArchetypeInfo = nextArchetype.AddEntity(entity);

        ref var nextInfo = ref entityHandler.GetEntityInfo(entity);
        nextInfo.ArchetypeIndex = nextArchetypeIndex;
        nextInfo.ChunkIndex = nextArchetypeInfo.ChunkIndex;
        nextInfo.RowIndex = nextArchetypeInfo.RowIndex;

        var movedEntity = archetype.MoveEntity(prevEntityInfo.ChunkIndex, prevEntityInfo.RowIndex, nextArchetype, nextArchetypeInfo.ChunkIndex, nextArchetypeInfo.RowIndex);
        if (!movedEntity.IsNull)
        {
            ref var movedEntityInfo = ref entityHandler.GetEntityInfo(movedEntity);
            movedEntityInfo.ChunkIndex = nextInfo.ChunkIndex;
            movedEntityInfo.RowIndex = nextInfo.RowIndex;
        }

        changes.SetRemoved(entity, in component);
        nextArchetype.Chunks[nextInfo.ChunkIndex].SetFlag<C>(nextInfo.RowIndex, ComponentFlags.Removed);
    }

    public void SetComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        var archetype = archetypes[entityInfo.ArchetypeIndex];
        archetype.SetComponent(entityInfo.ChunkIndex, entityInfo.RowIndex, component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool EntityExists(in Entity entity)
    {
        return entityHandler.IsAlive(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entities.EntityInfo GetEntityInfo(in Entity entity)
    {
        return entityHandler.GetEntityInfo(entity);
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
