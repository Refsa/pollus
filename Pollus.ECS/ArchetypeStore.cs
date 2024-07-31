namespace Pollus.ECS;

public class ArchetypeStore : IDisposable
{
    public record struct EntityInfo
    {
        public int ArchetypeIndex { get; set; }
        public int ChunkIndex { get; set; }
        public int RowIndex { get; set; }
    }

    public struct EntityRef
    {
        public EntityRef(Entity entity, Archetype archetype, int chunkIndex, int rowIndex)
        {
            Entity = entity;
            Archetype = archetype;
            ChunkIndex = chunkIndex;
            RowIndex = rowIndex;
        }

        public Entity Entity { get; set; }
        public Archetype Archetype { get; set; }
        public int ChunkIndex { get; set; }
        public int RowIndex { get; set; }
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

    public EntityRef CreateEntity<TBuilder>()
        where TBuilder : struct, IEntityBuilder
    {
        var entity = new Entity(entityCounter++);
        var archetype = GetArchetype(TBuilder.ArchetypeID) ?? CreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        var archetypeInfo = archetype.AddEntity(entity);
        return new(entity, archetype, archetypeInfo.ChunkIndex, archetypeInfo.RowIndex);
    }
}
