namespace Pollus.ECS;

using Pollus.Debugging;

public class WorldBuilder
{
    World world;
    PluginGraph pluginGraph;
    List<Action> onBuild;

    public static WorldBuilder Default => new();

    public WorldBuilder()
    {
        world = new World();
        pluginGraph = new();
        onBuild = [];
    }

    public WorldBuilder AddResource<TResource>(TResource resource)
        where TResource : notnull
    {
        world.Resources.Add(resource);
        return this;
    }

    public WorldBuilder InitResource<TResource>()
        where TResource : notnull
    {
        world.Resources.Init<TResource>();
        return this;
    }

    public WorldBuilder InitEvent<TEvent>()
        where TEvent : struct
    {
        world.Events.InitEvent<TEvent>();
        return this;
    }

    public WorldBuilder AddSystems(StageLabel stage, params ISystemBuilder[] builders)
    {
        onBuild.Add(() => world.Schedule.AddSystems(stage, builders));
        return this;
    }

    public WorldBuilder AddSystemSet<TSystemSet>()
        where TSystemSet : ISystemSet
    {
        onBuild.Add(() => world.Schedule.AddSystemSet<TSystemSet>());
        return this;
    }

    public WorldBuilder AddPlugin<TPlugin>(TPlugin plugin)
        where TPlugin : IPlugin
    {
        pluginGraph.Add(plugin);
        return this;
    }

    public WorldBuilder AddPlugin<TPlugin>()
        where TPlugin : IPlugin, new()
    {
        pluginGraph.Add(new TPlugin());
        return this;
    }

    public WorldBuilder AddPlugins(params IPlugin[] plugins)
    {
        foreach (var plugin in plugins)
        {
            pluginGraph.Add(plugin);
        }

        return this;
    }

    public World Build()
    {
        var plugins = pluginGraph.GetSortedPlugins();
        foreach (var plugin in plugins)
        {
            Log.Info($"Adding plugin {plugin.GetType().Name}");
            world.AddPlugin(plugin);
        }

        foreach (var action in onBuild)
        {
            action();
        }

        world.Prepare();
        return world;
    }
}