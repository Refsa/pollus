namespace Pollus.ECS;

using Mathematics;

public record struct ArchetypeID(int Hash)
{
    public static ArchetypeID Create(int hash)
    {
        return new ArchetypeID(hash);
    }

    public static ArchetypeID Create(scoped in ReadOnlySpan<ComponentID> cids)
    {
        var hash = 0;
        for (int i = 0; i < cids.Length; i++)
        {
            hash ^= Hashes.ZobristHash(cids[i].ID);
        }

        return new ArchetypeID(hash);
    }

    public static explicit operator int(ArchetypeID id) => id.Hash;
    public static explicit operator ArchetypeID(int hash) => new(hash);

    public ArchetypeID With<C>() where C : unmanaged, IComponent
    {
        var cid = Component.GetInfo<C>().ID;
        return new ArchetypeID(Hash ^ Hashes.ZobristHash(cid));
    }

    public ArchetypeID With(in ComponentID cid)
    {
        return new ArchetypeID(Hash ^ Hashes.ZobristHash(cid.ID));
    }

    public override int GetHashCode() => Hash;
}
