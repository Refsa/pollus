namespace Pollus.ECS;

public struct PluginDependency(Type type, Func<IPlugin> factory)
{
    public Type Type => type;
    public Func<IPlugin> Factory => factory;

    public static implicit operator PluginDependency((Type type, Func<IPlugin> factory) dep) => new(dep.type, dep.factory);

    public void Deconstruct(out Type type, out Func<IPlugin> factory)
    {
        type = Type;
        factory = Factory;
    }
}

public interface IPlugin
{
    public PluginDependency[] Dependencies => [];

    void Apply(World world);
}