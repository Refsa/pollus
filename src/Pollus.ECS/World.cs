namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Utils;

public class World : IDisposable
{
    static World()
    {
        ResourceFetch<World>.Register();
        ResourceFetch<Resources>.Register();
        ResourceFetch<Events>.Register();
        CommandsFetch.Register();
    }

    ulong version = 0;

    readonly HashSet<Type> registeredPlugins;
    readonly Pool<Commands> commandBuffers;
    readonly Queue<Commands> commandBuffersQueue;

    public Schedule Schedule { get; init; }
    public ArchetypeStore Store { get; init; }
    public Resources Resources { get; init; }
    public Events Events { get; init; }

    public World()
    {
        Store = new();
        Schedule = Schedule.CreateDefault();
        Resources = new();
        Events = new();
        
        commandBuffers = new(() => new(Store.Entities), 1);
        registeredPlugins = new();
        commandBuffersQueue = new();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Store.Dispose();
        Resources.Dispose();
        Schedule.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn()
    {
        return Store.CreateEntity();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Despawn(in Entity entity)
    {
        Store.DestroyEntity(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn<TBuilder>(in TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        return builder.Spawn(this);
    }

    public void Despawn(Entity entity)
    {
        Store.DestroyEntity(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Preallocate<TBuilder>(in TBuilder _, int count)
        where TBuilder : IEntityBuilder
    {
        var (archetype, _) = Store.GetOrCreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        archetype.Preallocate(count);
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

    public Commands GetCommands()
    {
        var commands = commandBuffers.Rent();
        commandBuffersQueue.Enqueue(commands);
        return commands;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Prepare()
    {
        Resources.Add(this);
        Resources.Add(Resources);
        Resources.Add(Events);

        Schedule.Prepare(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Update()
    {
        try
        {
            version++;
            Store.Tick(version);

            foreach (var stage in Schedule.Stages)
            {
                stage.Tick(this);

                while (commandBuffersQueue.Count > 0)
                {
                    var commands = commandBuffersQueue.Dequeue();
                    commands.Flush(this);
                    commandBuffers.Return(commands);
                }
            }

            Events.ClearEvents();
        }
        catch
        {
            throw;
        }
    }
}