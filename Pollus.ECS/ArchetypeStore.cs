namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Collections;

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

    readonly List<Archetype> archetypes = [];
    NativeMap<int, int> archetypeLookup;
    NativeMap<Entity, EntityInfo> entities;
    volatile int entityCounter = 0;

    public List<Archetype> Archetypes => archetypes;

    public ArchetypeStore()
    {
        entities = new(0);
        archetypeLookup = new(0);

        var aid = ArchetypeID.Create([]);
        archetypes.Add(new Archetype(aid, []));
        archetypeLookup.Add((int)aid, 0);
    }

    public void Dispose()
    {
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
        if (archetypeLookup.TryGetValue((int)aid, out var index))
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
        if (archetypeLookup.TryGetValue((int)aid, out var index))
        {
            return (archetypes[index], index);
        };

        var archetype = new Archetype(aid, cids);
        archetypes.Add(archetype);
        archetypeLookup.Add((int)aid, archetypes.Count - 1);
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
            var movedEntityInfo = archetype.RemoveEntity(new()
            {
                ChunkIndex = info.ChunkIndex,
                RowIndex = info.RowIndex
            });

            if (movedEntityInfo is not null)
            {
                ref var movedEntity = ref entities.Get(movedEntityInfo.Value.Entity);
                movedEntity.ChunkIndex = movedEntityInfo.Value.ChunkIndex;
                movedEntity.RowIndex = movedEntityInfo.Value.RowIndex;
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
            return ref archetype.GetComponent<C>(info.ChunkIndex, info.RowIndex);
        }
        throw new ArgumentException("Entity does not exist");
    }

    public void AddComponent<C>(in Entity entity, scoped in C component)
        where C : unmanaged, IComponent
    {
        if (entities.TryGetValue(entity, out var info))
        {
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

            var movedInfo = archetype.MoveEntity(new() { ChunkIndex = info.ChunkIndex, RowIndex = info.RowIndex }, nextArchetype, nextArchetypeInfo);
            if (movedInfo is not null)
            {
                ref var movedEntity = ref entities.Get(movedInfo.Value.Entity);
                movedEntity.ChunkIndex = movedInfo.Value.ChunkIndex;
                movedEntity.RowIndex = movedInfo.Value.RowIndex;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool EntityExists(in Entity entity)
    {
        return entities.Has(entity);
    }
}
