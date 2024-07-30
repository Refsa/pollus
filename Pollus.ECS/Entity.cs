namespace Pollus.ECS;

public record struct Entity(int ID)
{
    public static readonly Entity NULL = new Entity(-1);

    public static EntityBuilder<C0> With<C0>(in C0 c0)
        where C0 : unmanaged, IComponent
    {
        return new EntityBuilder<C0>(c0);
    }
}

public interface IEntityBuilder
{
    static abstract ComponentID[] ComponentIDs { get; }
    static abstract ArchetypeID ArchetypeID { get; }

    Entity Spawn(World world);
}

public struct EntityBuilder : IEntityBuilder
{
    static readonly ComponentID[] componentIDs = [];
    public static ComponentID[] ComponentIDs => componentIDs;
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(componentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

    public Entity Spawn(World world)
    {
        return world.Archetypes.CreateEntity(this).entity;
    }

    public EntityBuilder<C0> With<C0>(in C0 c0)
        where C0 : unmanaged, IComponent
    {
        return new EntityBuilder<C0>(c0);
    }
}

public struct EntityBuilder<C0> : IEntityBuilder
    where C0 : unmanaged, IComponent
{
    static readonly ComponentID[] componentIDs = [Component.GetInfo<C0>().ID];
    public static ComponentID[] ComponentIDs => componentIDs;
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(componentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

    public C0 Component0;

    public EntityBuilder(in C0 c0)
    {
        Component0 = c0;
    }

    public Entity Spawn(World world)
    {
        var (entity, entityInfo, archetype) = world.Archetypes.CreateEntity(this);
        ref var chunk = ref archetype.GetChunk(entityInfo.ChunkIndex);
        chunk.SetComponent(entityInfo.RowIndex, Component0);
        return entity;
    }

    public EntityBuilder<C0, C1> With<C1>(in C1 c1)
        where C1 : unmanaged, IComponent
    {
        return new EntityBuilder<C0, C1>(Component0, c1);
    }
}
