namespace Pollus.ECS;

public class ArchetypeChunk
{
    public Archetype Archetype { get; init; }
    List<ComponentColumn> columns;
    BitSet occupancy;

    public bool IsFull => occupancy.FirstClearBit() == -1;
    public int Index { get; init; }

    public ArchetypeChunk(Archetype archetype, int chunkIndex)
    {
        Archetype = archetype;
        Index = chunkIndex;
        occupancy = new();
        columns = [];

        foreach (var cid in archetype.components)
        {
            columns.Add(ComponentColumn.From(cid));
        }
    }

    public void Set<C1>(int row, C1 component) where C1 : unmanaged, IComponent
    {
        var column = columns.Find(c => c.ComponentID == Component.GetInfo<C1>().ID);
        column.Set(row, component);
    }

    public ref C1 Get<C1>(int row) where C1 : unmanaged, IComponent
    {
        var column = columns.Find(c => c.ComponentID == Component.GetInfo<C1>().ID);
        return ref column.Get<C1>(row);
    }

    public int Insert()
    {
        var row = occupancy.FirstClearBit();
        occupancy.Set(row);
        return row;
    }

    public void Remove(int row)
    {
        occupancy.Unset(row);
    }
}
