using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public class World : IDisposable
{
    static World()
    {
        FetchInit.Init();
    }

    public Schedule Schedule { get; init; }
    public ArchetypeStore Store { get; init; }
    public Resources Resources { get; init; }

    public World()
    {
        Store = new();
        Schedule = Schedule.CreateDefault();
        Resources = new();
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

    public World AddPlugin<TPlugin>(TPlugin plugin)
        where TPlugin : IPlugin
    {
        plugin.Apply(this);
        return this;
    }

    public World AddPlugin<TPlugin>()
        where TPlugin : IPlugin, new()
    {
        var plugin = new TPlugin();
        plugin.Apply(this);
        return this;
    }

    public World AddPlugins(params IPlugin[] plugins)
    {
        foreach (var plugin in plugins)
        {
            plugin.Apply(this);
        }
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Prepare()
    {
        Schedule.Prepare(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Tick()
    {
        Schedule.Tick(this);
    }
}