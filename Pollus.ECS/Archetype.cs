namespace Pollus.ECS;

public record struct ArchetypeID(int ID, int Hash, int Version = 0);

public static class ArchetypeCounter
{
    volatile static int counter = 0;
    public static int Next()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class Archetype
{
    record class Row
    {
        public Entity Entity;
        public int ChunkIndex;
        public int RowIndex;
    }

    public int RowSize { get; init; }
    public ArchetypeID ID => id;

    internal int[] components { get; init; }

    List<Row> entities;
    List<ArchetypeChunk> chunks;
    ArchetypeID id;

    public Archetype(Span<ComponentID> cids)
    {
        chunks = [];
        entities = [];
        components = [.. cids];

        var compHash = 0;
        foreach (var cid in cids)
        {
            RowSize += Component.GetInfo(cid).Size;
            compHash = HashCode.Combine(compHash, (int)cid);
        }

        id = new ArchetypeID(ArchetypeCounter.Next(), compHash);
    }

    public bool Has<C1>() where C1 : unmanaged, IComponent
    {
        return components.Contains(Component.GetInfo<C1>().ID);
    }

    public bool Has(ComponentID cid)
    {
        return components.Contains(cid);
    }

    public bool HasAll(Span<ComponentID> cids)
    {
        foreach (var component in cids)
        {
            if (!components.Contains(component)) return false;
        }

        return true;
    }

    public bool HasAny(Span<ComponentID> cids)
    {
        foreach (var component in cids)
        {
            if (components.Contains(component)) return true;
        }

        return false;
    }

    public void Insert(Entity entity)
    {
        var chunk = FirstVacantChunk();

        entities.Add(new()
        {
            Entity = entity,
            ChunkIndex = chunk.Index,
            RowIndex = chunk.Insert()
        });
    }

    public void Remove(Entity entity)
    {
        var row = entities.Find(e => e.Entity == entity);
        var chunk = chunks[row.ChunkIndex];
        chunk.Remove(row.RowIndex);
        entities.Remove(row);
    }

    public void Set<C1>(Entity entity, C1 component) where C1 : unmanaged, IComponent
    {
        var row = entities.Find(e => e.Entity == entity);
        var chunk = chunks[row.ChunkIndex];
        chunk.Set(row.RowIndex, component);
    }

    public ref C1 Get<C1>(Entity entity) where C1 : unmanaged, IComponent
    {
        var row = entities.Find(e => e.Entity == entity);
        var chunk = chunks[row.ChunkIndex];
        return ref chunk.Get<C1>(row.RowIndex);
    }

    ArchetypeChunk FirstVacantChunk()
    {
        foreach (var chunk in chunks)
        {
            if (!chunk.IsFull) return chunk;
        }

        var created = new ArchetypeChunk(this, chunks.Count);
        chunks.Add(created);
        return created;
    }
}
