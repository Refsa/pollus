using Pollus.Debugging;

namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Utils;

public class World : IDisposable
{
    static World()
    {
        ResourcesFetch.Register();
        WorldFetch.Register();
        CommandsFetch.Register();
        ResourceFetch<Events>.Register();
    }

    ulong version = 0;
    bool isDisposed;

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
        registeredPlugins = [];
        commandBuffersQueue = new();
    }

    public void Dispose()
    {
        if (isDisposed) return;
        GC.SuppressFinalize(this);
        isDisposed = true;

        Store.Dispose();
        Resources.Dispose();
        Schedule.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Prepare()
    {
        Resources.Add(this);
        Resources.Add(Events);

        Schedule.Prepare(this);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Despawn(Entity entity)
    {
        Store.DestroyEntity(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EntityRef GetEntityRef(in Entity entity)
    {
        var entityInfo = Store.GetEntityInfo(entity);
        var archetype = Store.GetArchetype(entityInfo.ArchetypeIndex);
        ref var chunk = ref archetype.Chunks[entityInfo.ChunkIndex];
        return new EntityRef(in entity, entityInfo.RowIndex, ref chunk);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Clone(in Entity toClone, in Entity cloned)
    {
        Store.CloneEntity(toClone, cloned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Preallocate<TBuilder>(in TBuilder _, int count)
        where TBuilder : IEntityBuilder
    {
        var (archetype, _) = Store.GetOrCreateArchetype(TBuilder.ArchetypeID, TBuilder.ComponentIDs);
        archetype.Preallocate(count);
    }

    public World AddPlugin<TPlugin>(TPlugin plugin, bool addDependencies = false)
        where TPlugin : IPlugin
    {
        var pluginType = plugin.GetType();
        if (!registeredPlugins.Add(pluginType))
        {
            Log.Info($"Plugin {pluginType.Name} already added");
            return this;
        }

        if (addDependencies)
        {
            foreach (var dependency in plugin.Dependencies)
            {
                if (!registeredPlugins.Contains(dependency.Type))
                {
                    AddPlugin(dependency.Plugin, true);
                }
            }
        }

        registeredPlugins.Add(pluginType);
        plugin.Apply(this);
        return this;
    }

    public World AddPlugin<TPlugin>(bool addDependencies = false)
        where TPlugin : IPlugin, new()
    {
        return AddPlugin(new TPlugin(), addDependencies);
    }

    public World AddPlugins(bool addDependencies, params IPlugin[] plugins)
    {
        foreach (var plugin in plugins)
        {
            AddPlugin(plugin, addDependencies);
        }

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Commands GetCommands()
    {
        var commands = commandBuffers.Rent();
        commandBuffersQueue.Enqueue(commands);
        return commands;
    }

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

public class WorldFetch : IFetch<World>
{
    public static void Register() => Fetch.Register(new WorldFetch(), [typeof(World)]);
    public World DoFetch(World world, ISystem system) => world;
}
