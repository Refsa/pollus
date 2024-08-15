namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public interface IEntityBuilder
{
    static abstract ComponentID[] ComponentIDs { get; }
    static abstract ArchetypeID ArchetypeID { get; }

    Entity Spawn(World world);
}

unsafe public struct EntityBuilder : IEntityBuilder
{
    static readonly ComponentID[] componentIDs = [];
    public static ComponentID[] ComponentIDs => componentIDs;
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(componentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn(World world)
    {
        return world.Store.CreateEntity<EntityBuilder>().Entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
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

    public EntityBuilder(scoped in C0 c0)
    {
        Component0 = c0;
    }

    public static implicit operator EntityBuilder<C0>(scoped in C0 c0)
    {
        return new EntityBuilder<C0>(c0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn(World world)
    {
        var entityRef = world.Store.CreateEntity<EntityBuilder<C0>>();
        ref var chunk = ref entityRef.Archetype.GetChunk(entityRef.ChunkIndex);
        chunk.SetComponent(entityRef.RowIndex, Component0);
        return entityRef.Entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityBuilder<C0, C1> With<C1>(scoped in C1 c1)
        where C1 : unmanaged, IComponent
    {
        return new(Component0, c1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityBuilder<C0> Set(in C0 c0)
    {
        Component0 = c0;
        return this;
    }
}