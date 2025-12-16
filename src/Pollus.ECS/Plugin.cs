namespace Pollus.ECS;

public struct PluginDependency
{
    public static PluginDependency From<T>() where T : IPlugin, new() => new(typeof(T), () => new T());
    public static PluginDependency From<T>(Func<T> factory) where T : IPlugin => new(typeof(T), () => factory());


    public Type Type { get; init; }
    public Func<IPlugin> Factory { get; init; }
    public IPlugin Plugin { get; init; }

    public PluginDependency(Type type, Func<IPlugin> factory)
    {
        Type = type;
        Factory = factory;
        Plugin = factory();
    }
}

public interface IPlugin
{
    public PluginDependency[] Dependencies => [];

    void Apply(World world);
}

public class PluginGraph
{
    readonly Dictionary<Type, IPlugin> plugins = new();
    readonly Dictionary<Type, PluginDependency[]> dependencies = new();

    public void Add(IPlugin plugin)
    {
        var type = plugin.GetType();
        if (!plugins.TryAdd(type, plugin)) return;

        dependencies[type] = plugin.Dependencies;
        foreach (var dependency in plugin.Dependencies)
        {
            Add(dependency.Plugin);
        }
    }

    public List<IPlugin> GetSortedPlugins()
    {
        var sorted = new List<IPlugin>();
        var visited = new HashSet<Type>();
        var visiting = new HashSet<Type>();

        foreach (var pluginType in plugins.Keys)
        {
            Visit(pluginType, visited, visiting, sorted);
        }

        return sorted;
    }

    void Visit(Type type, HashSet<Type> visited, HashSet<Type> visiting, List<IPlugin> sorted)
    {
        if (visited.Contains(type)) return;
        if (!visiting.Add(type))
            throw new InvalidOperationException($"Cyclic dependency detected involving {type.Name}");

        if (dependencies.TryGetValue(type, out var deps))
        {
            foreach (var dep in deps)
            {
                Visit(dep.Type, visited, visiting, sorted);
            }
        }

        visiting.Remove(type);
        visited.Add(type);
        sorted.Add(plugins[type]);
    }
}