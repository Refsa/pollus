using System.Runtime.CompilerServices;
using Pollus.Logging;

namespace Pollus.ECS;

public class World : IDisposable
{
    static World()
    {
        FetchInit.Init();
        ResourceFetch<World>.Register();
        ResourceFetch<Resources>.Register();
    }

    public Schedule Schedule { get; init; }
    public ArchetypeStore Store { get; init; }
    public Resources Resources { get; init; }

    HashSet<Type> registeredPlugins = new();

    public World()
    {
        Store = new();
        Schedule = Schedule.CreateDefault();
        Resources = new();
    }

    public void Dispose()
    {
        Store.Dispose();
        Resources.Dispose();
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
        if (registeredPlugins.Contains(typeof(TPlugin))) return this;
        registeredPlugins.Add(typeof(TPlugin));

        plugin.Apply(this);
        return this;
    }

    public World AddPlugin<TPlugin>()
        where TPlugin : IPlugin, new()
    {
        if (registeredPlugins.Contains(typeof(TPlugin))) return this;
        registeredPlugins.Add(typeof(TPlugin));

        var plugin = new TPlugin();
        plugin.Apply(this);
        return this;
    }

    public World AddPlugins(params IPlugin[] plugins)
    {
        foreach (var plugin in plugins)
        {
            if (registeredPlugins.Contains(plugin.GetType())) continue;
            registeredPlugins.Add(plugin.GetType());

            plugin.Apply(this);
        }
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Prepare()
    {
        Resources.Add(this);
        Resources.Add(Resources);
        Schedule.Prepare(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Tick()
    {
        try
        {
            Schedule.Tick(this);
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while running the world schedule.");
        }
    }
}