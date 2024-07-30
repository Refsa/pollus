namespace Pollus.ECS;

public class ArchetypeStore : IDisposable
{
    public record struct EntityInfo
    {
        public int ArchetypeIndex { get; init; }
        public int ChunkIndex { get; init; }
        public int RowIndex { get; init; }
    }

    readonly List<Archetype> archetypes = [];
    NativeMap<ArchetypeID, int> archetypeLookup;
    NativeMap<Entity, EntityInfo> entities;
    volatile int entityCounter = 0;

    public ArchetypeStore()
    {
        entities = new(0);
        archetypeLookup = new(0);

        var aid = ArchetypeID.Create([]);
        archetypes.Add(new Archetype(aid, []));
        archetypeLookup.Add(aid, 0);
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

    public Archetype? GetArchetype(in ArchetypeID id)
    {
        if (archetypeLookup.TryGetValue(id, out var index))
        {
            return archetypes[index];
        }
        return null;
    }

    Archetype CreateArchetype(in ArchetypeID aid, in Span<ComponentID> cids)
    {
        var archetype = new Archetype(aid, cids);
        archetypes.Add(archetype);
        archetypeLookup.Add(aid, archetypes.Count - 1);
        return archetype;
    }

    public Entity CreateEntity()
    {
        var entity = new Entity(entityCounter++);
        var archetypeInfo = archetypes[0].AddEntity(entity);
        entities.Add(entity, new EntityInfo { ArchetypeIndex = 0, ChunkIndex = archetypeInfo.ChunkIndex, RowIndex = archetypeInfo.RowIndex });
        return entity;
    }

    public (Entity entity, EntityInfo entityInfo, Archetype archetype) CreateEntity<TBuilder>(in TBuilder builder)
        where TBuilder : struct, IEntityBuilder
    {
        var entity = new Entity(entityCounter++);
        var archetype = GetArchetype(TBuilder.ArchetypeID);
        archetype ??= CreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        var archetypeInfo = archetype.AddEntity(entity);
        var entityInfo = new EntityInfo
        {
            ArchetypeIndex = archetypeLookup.Get(TBuilder.ArchetypeID),
            ChunkIndex = archetypeInfo.ChunkIndex,
            RowIndex = archetypeInfo.RowIndex
        };
        return (entity, entityInfo, archetype);
    }
}
