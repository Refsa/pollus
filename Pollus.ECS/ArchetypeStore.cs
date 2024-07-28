namespace Pollus.ECS;

public class ArchetypeStore : IDisposable
{
    record struct EntityInfo
    {
        public int ArchetypeIndex { get; init; }
        public int ChunkIndex { get; init; }
        public int RowIndex { get; init; }
    }

    readonly List<Archetype> archetypes = [];
    readonly Dictionary<ArchetypeID, int> archetypeLookup = [];
    NativeMap<Entity, EntityInfo> entities;

    public ArchetypeStore()
    {

    }

    public void Dispose()
    {
        foreach (var archetype in archetypes)
        {
            archetype.Dispose();
        }
    }
}
