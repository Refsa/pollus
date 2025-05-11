namespace Pollus.ECS;

public record struct ArchetypeID(int Hash)
{
    public static ArchetypeID Create(int hash)
    {
        return new ArchetypeID(hash);
    }

    public static ArchetypeID Create(scoped in Span<ComponentID> cids)
    {
        var hash = 0;
        for (int i = 0; i < cids.Length; i++)
        {
            hash = HashCode.Combine(hash, cids[i].ID);
        }
        return new ArchetypeID(hash);
    }

    public static explicit operator int(ArchetypeID id) => id.Hash;
    public static explicit operator ArchetypeID(int hash) => new(hash);

    public ArchetypeID With<C>() where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        return new ArchetypeID(HashCode.Combine(Hash, cid));
    }

    public ArchetypeID With(in ComponentID cid)
    {
        return new ArchetypeID(HashCode.Combine(Hash, cid.ID));
    }

    public override int GetHashCode() => Hash;
}
