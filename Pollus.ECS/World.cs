using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public class World : IDisposable
{
    public ArchetypeStore Store { get; init; }
    public Schedule Schedule { get; init; }

    public World()
    {
        Store = new();
        Schedule = Schedule.CreateDefault();
    }

    public void Dispose()
    {
        Store.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn()
    {
        return Store.CreateEntity();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn<TBuilder>(in TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        return builder.Spawn(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Preallocate<TBuilder>(in TBuilder builder, int count)
        where TBuilder : IEntityBuilder
    {
        var archetypeInfo = Store.GetOrCreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        archetypeInfo.archetype.Preallocate(count);
    }

    public void Prepare()
    {
        Schedule.Prepare(this);
    }

    public void Tick()
    {
        Schedule.Tick(this);
    }
}