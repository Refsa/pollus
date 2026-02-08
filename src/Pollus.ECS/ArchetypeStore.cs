namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Debugging;

public class ArchetypeStore : IDisposable
{
    public record struct EntityChange(Entity Entity, Archetype Archetype, int ChunkIndex, int RowIndex);

    public record struct ArchetypeInfo(Archetype Archetype, int Index);

    ulong version = 0;

    readonly List<Archetype> archetypes;
    NativeMap<ArchetypeID, int> archetypeLookup;
    readonly RemovedTracker removedTracker;
    Entities entityHandler;

    public List<Archetype> Archetypes => archetypes;
    public int EntityCount => entityHandler.AliveCount;
    public RemovedTracker Removed => removedTracker;

    public Entities Entities => entityHandler;

    public ArchetypeStore()
    {
        archetypeLookup = new(0);
        archetypes = [];
        removedTracker = new();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (Archetype archetype, int index)? GetArchetype(in ArchetypeID aid)
    {
        if (archetypeLookup.TryGetValue(aid, out var index))
        {
            return (archetypes[index], index);
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Archetype GetArchetype(int index)
    {
        return archetypes[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArchetypeInfo GetOrCreateArchetype(in ArchetypeID aid, scoped in ReadOnlySpan<ComponentID> cids)
    {
        if (archetypeLookup.TryGetValue(aid, out var index))
            return new(archetypes[index], index);

        foreach (var cid in cids)
        {
            var cinfo = Component.GetInfo(cid);
            Guard.IsNotNull(cinfo.RegisterTracker, "No register tracker for component found");
            cinfo.RegisterTracker(removedTracker);
        }

        var archetype = new Archetype(aid, cids);
        archetype.Tick(version);
        archetypes.Add(archetype);
        archetypeLookup.Add(aid, archetypes.Count - 1);
        return new(archetype, archetypes.Count - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity CreateEntity()
    {
        var entity = entityHandler.Create();
        var (chunkIndex, rowIndex) = archetypes[0].AddEntity(entity);
        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        entityInfo.ArchetypeIndex = 0;
        entityInfo.ChunkIndex = chunkIndex;
        entityInfo.RowIndex = rowIndex;
        return entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityChange CreateEntity<TBuilder>()
        where TBuilder : struct, IEntityBuilder
    {
        var entity = entityHandler.Create();
        return InsertEntity<TBuilder>(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityChange CreateEntity(in ArchetypeID aid, scoped in ReadOnlySpan<ComponentID> cids)
    {
        var entity = entityHandler.Create();
        return InsertEntity(entity, aid, cids);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityChange InsertEntity<TBuilder>(in Entity entity)
        where TBuilder : struct, IEntityBuilder
    {
        return InsertEntity(entity, TBuilder.ArchetypeID, TBuilder.ComponentIDs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityChange InsertEntity(in Entity entity, in ArchetypeID aid, scoped in ReadOnlySpan<ComponentID> cids)
    {
        var archetypeInfo = GetOrCreateArchetype(aid, cids);
        var (chunkIndex, rowIndex) = archetypeInfo.Archetype.AddEntity(entity);
        archetypeInfo.Archetype.Chunks[chunkIndex].SetAllFlags(rowIndex, ComponentFlags.Added);

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        entityInfo.ArchetypeIndex = archetypeInfo.Index;
        entityInfo.ChunkIndex = chunkIndex;
        entityInfo.RowIndex = rowIndex;
        return new(entity, archetypeInfo.Archetype, chunkIndex, rowIndex);
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
        ref var chunk = ref archetype.GetChunk(entityInfo.ChunkIndex);

        foreach (var cid in archetype.GetChunkInfo().ComponentIDs)
        {
            var tracker = removedTracker.GetTracker(cid);
            tracker.SetRemoved(entity, chunk.GetComponent(entityInfo.RowIndex, cid));
        }

        var movedEntity = archetype.RemoveEntity(entityInfo.ChunkIndex, entityInfo.RowIndex);
        if (!movedEntity.IsNull)
        {
            ref var movedEntityInfo = ref entityHandler.GetEntityInfo(movedEntity);
            Guard.IsFalse(Unsafe.IsNullRef(ref movedEntityInfo), "Moved entity is null");

            movedEntityInfo.ChunkIndex = entityInfo.ChunkIndex;
            movedEntityInfo.RowIndex = entityInfo.RowIndex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityChange CloneEntity(in Entity toClone, in Entity cloned)
    {
        ref var entityInfo = ref entityHandler.GetEntityInfo(toClone);
        var archetype = archetypes[entityInfo.ArchetypeIndex];
        var (chunkIndex, rowIndex) = archetype.AddEntity(cloned);
        archetype.Chunks[chunkIndex].SetAllFlags(rowIndex, ComponentFlags.Added);

        ref var clonedInfo = ref entityHandler.GetEntityInfo(cloned);
        clonedInfo.ArchetypeIndex = entityInfo.ArchetypeIndex;
        clonedInfo.ChunkIndex = chunkIndex;
        clonedInfo.RowIndex = rowIndex;
        ref var clonedChunk = ref archetype.GetChunk(chunkIndex);

        var chunkInfo = archetype.GetChunkInfo();
        ref var chunk = ref archetype.GetChunk(entityInfo.ChunkIndex);
        for (int i = 0; i < chunkInfo.ComponentIDs.Length; i++)
        {
            var cid = chunkInfo.ComponentIDs[i];
            var component = chunk.GetComponent(entityInfo.RowIndex, cid);
            clonedChunk.SetComponent(rowIndex, cid, component);
        }

        return new(cloned, archetype, chunkIndex, rowIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref C GetComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        Guard.IsTrue(entityHandler.IsAlive(entity), $"Entity {entity} does not exist");

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        var archetype = archetypes[entityInfo.ArchetypeIndex];
        Guard.IsTrue(archetype.HasComponent<C>(), $"Entity {entity} does not have component {typeof(C)}");
        return ref archetype.GetComponent<C>(entityInfo.ChunkIndex, entityInfo.RowIndex);
    }

    public void AddComponent(in Entity entity, in ComponentID componentID, in ReadOnlySpan<byte> data)
    {
        if (!entityHandler.IsAlive(entity))
        {
            throw new ArgumentException("Entity does not exist");
        }

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        var prevEntityInfo = entityInfo;
        var archetype = archetypes[prevEntityInfo.ArchetypeIndex];
        if (archetype.HasComponent(componentID)) return;

        var prevInfo = archetype.GetChunkInfo();
        Span<ComponentID> cids = stackalloc ComponentID[prevInfo.ComponentIDs.Length + 1];
        prevInfo.ComponentIDs.CopyTo(cids);
        cids[^1] = componentID;

        var nextAid = ArchetypeID.Create(cids);
        var (nextArchetype, nextArchetypeIndex) = GetOrCreateArchetype(nextAid, cids);
        var (chunkIndex, rowIndex) = nextArchetype.AddEntity(entity);

        entityInfo.ArchetypeIndex = nextArchetypeIndex;
        entityInfo.ChunkIndex = chunkIndex;
        entityInfo.RowIndex = rowIndex;

        nextArchetype.SetComponent(chunkIndex, rowIndex, componentID, data);

        var movedEntity = archetype.MoveEntity(prevEntityInfo.ChunkIndex, prevEntityInfo.RowIndex, nextArchetype, chunkIndex, rowIndex);
        if (!movedEntity.IsNull)
        {
            ref var movedEntityInfo = ref entityHandler.GetEntityInfo(movedEntity);
            movedEntityInfo.ChunkIndex = prevEntityInfo.ChunkIndex;
            movedEntityInfo.RowIndex = prevEntityInfo.RowIndex;
        }

        nextArchetype.Chunks[entityInfo.ChunkIndex].SetFlag(entityInfo.RowIndex, componentID, ComponentFlags.Added);
    }

    public void AddComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        var componentSpan = MemoryMarshal.CreateReadOnlySpan(in component, 1);
        var span = MemoryMarshal.Cast<C, byte>(componentSpan);
        AddComponent(entity, Component.GetInfo<C>().ID, span);
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

        var prevInfo = archetype.GetChunkInfo();
        Span<ComponentID> cids = stackalloc ComponentID[prevInfo.ComponentIDs.Length - 1];
        var index = 0;
        foreach (var cid in prevInfo.ComponentIDs)
        {
            if (cid != Component.GetInfo<C>().ID) cids[index++] = cid;
        }

        var nextAid = ArchetypeID.Create(cids);
        var (nextArchetype, nextArchetypeIndex) = GetOrCreateArchetype(nextAid, cids);
        var (chunkIndex, rowIndex) = nextArchetype.AddEntity(entity);

        ref var nextInfo = ref entityHandler.GetEntityInfo(entity);
        nextInfo.ArchetypeIndex = nextArchetypeIndex;
        nextInfo.ChunkIndex = chunkIndex;
        nextInfo.RowIndex = rowIndex;

        var movedEntity = archetype.MoveEntity(prevEntityInfo.ChunkIndex, prevEntityInfo.RowIndex, nextArchetype, chunkIndex, rowIndex);
        if (!movedEntity.IsNull)
        {
            ref var movedEntityInfo = ref entityHandler.GetEntityInfo(movedEntity);
            movedEntityInfo.ChunkIndex = prevEntityInfo.ChunkIndex;
            movedEntityInfo.RowIndex = prevEntityInfo.RowIndex;
        }

        removedTracker.SetRemoved(entity, in component);
        nextArchetype.Chunks[nextInfo.ChunkIndex].SetFlag<C>(nextInfo.RowIndex, ComponentFlags.Removed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        Guard.IsTrue(entityHandler.IsAlive(entity), $"Entity {entity} does not exist");

        ref var entityInfo = ref entityHandler.GetEntityInfo(entity);
        var archetype = archetypes[entityInfo.ArchetypeIndex];
        Guard.IsTrue(archetype.HasComponent<C>(), $"Entity {entity} does not have component {typeof(C)}");
        archetype.SetComponent(entityInfo.ChunkIndex, entityInfo.RowIndex, component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool EntityExists(in Entity entity)
    {
        return entityHandler.IsAlive(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entities.EntityInfo GetEntityInfo(in Entity entity)
    {
        return ref entityHandler.GetEntityInfo(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Tick(ulong version)
    {
        this.version = version;
        removedTracker.Tick(version);
        foreach (var archetype in archetypes)
        {
            archetype.Tick(version);
        }
    }
}
