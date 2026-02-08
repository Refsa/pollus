namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Utils;

public interface IEntityBuilder
{
    static abstract ComponentID[] ComponentIDs { get; }
    static abstract ArchetypeID ArchetypeID { get; }

    Entity Spawn(World world);
    Entity Spawn(World world, in Entity entity);
}

public struct EntityBuilder : IEntityBuilder
{
    static readonly ComponentID[] componentIDs = [];
    public static ComponentID[] ComponentIDs => componentIDs;
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(componentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

    public Entity Spawn(World world)
    {
        return world.Store.CreateEntity<EntityBuilder>().Entity;
    }

    public Entity Spawn(World world, in Entity entity)
    {
        return world.Store.InsertEntity<EntityBuilder>(entity).Entity;
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
    static readonly ComponentID[] componentIDs = [Component.Register<C0>().ID];
    public static ComponentID[] ComponentIDs { get; } = CollectionUtils.Distinct(RequiredComponents.Get<C0>().ComponentIDs, componentIDs);
    static readonly ComponentDefaultData[] requiredComponents = CollectRequiredComponents();
    static readonly ArchetypeID archetypeID = ArchetypeID.Create(ComponentIDs);
    public static ArchetypeID ArchetypeID => archetypeID;

    static ComponentDefaultData[] CollectRequiredComponents()
    {
        var tmp = new Dictionary<ComponentID, ComponentDefaultData>();
        Collect(RequiredComponents.Get<C0>().Defaults);
        return tmp.Values.ToArray();

        void Collect(Dictionary<ComponentID, byte[]> defaults)
        {
            foreach (var kvp in defaults)
            {
                if (componentIDs.Contains(kvp.Key)) continue;
                tmp[kvp.Key] = new ComponentDefaultData(kvp.Key, kvp.Value);
            }
        }
    }

    public C0 Component0;

    public EntityBuilder(scoped in C0 c0)
    {
        Component0 = c0;
    }

    public static implicit operator EntityBuilder<C0>(scoped in C0 c0)
    {
        return new EntityBuilder<C0>(c0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity Spawn(World world)
    {
        var entity = world.Store.Entities.Create();
        return Spawn(world, entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity Spawn(World world, in Entity entity)
    {
        var entityRef = world.Store.InsertEntity<EntityBuilder<C0>>(entity);
        ref var chunk = ref entityRef.Archetype.GetChunk(entityRef.ChunkIndex);

        chunk.SetComponent(entityRef.RowIndex, Component0);

        foreach (scoped ref readonly var required in requiredComponents.AsSpan())
        {
            chunk.SetComponent(entityRef.RowIndex, required.CID, required.Data);
        }

        return entityRef.Entity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder<C0, C1> With<C1>(scoped in C1 c1)
        where C1 : unmanaged, IComponent
    {
        return new(Component0, c1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityBuilder<C0> Set(in C0 c0)
    {
        Component0 = c0;
        return this;
    }
}