namespace Pollus.ECS.Core;

using System.Runtime.CompilerServices;

public class ExclusiveSystemMarker { }

public delegate void SystemDelegate();
public delegate void SystemDelegate<T0>(T0 arg1);

public record struct SystemLabel(string Label)
{
    public static implicit operator SystemLabel(string label) => new(label);
}

public class SystemDescriptor
{
    public SystemLabel Label { get; private set; }
    public HashSet<SystemLabel> RunsBefore { get; } = [];
    public HashSet<SystemLabel> RunsAfter { get; } = [];
    public HashSet<Type> Parameters { get; } = [];
    public HashSet<Type> Dependencies { get; } = [];

    public SystemDescriptor() { }

    public SystemDescriptor(SystemLabel label)
    {
        Label = label;
    }

    public SystemDescriptor WithLabel(SystemLabel label)
    {
        Label = label;
        return this;
    }

    public SystemDescriptor Before(SystemLabel label)
    {
        RunsBefore.Add(label);
        return this;
    }

    public SystemDescriptor After(SystemLabel label)
    {
        RunsAfter.Add(label);
        return this;
    }

    public SystemDescriptor DependsOn<T>()
    {
        Dependencies.Add(typeof(T));
        return this;
    }
}

public interface ISystem
{
    static HashSet<Type> Dependencies => [];

    IRunCriteria RunCriteria { get; set; }
    SystemDescriptor Descriptor { get; }
    Resources Resources { get; }

    bool ShouldRun(World world) => RunCriteria.ShouldRun(world);
    void Tick(World world) { }
}

public abstract class Sys(SystemDescriptor descriptor) : ISystem
{
    public static HashSet<Type> Dependencies => [];

    public IRunCriteria RunCriteria { get; set; } = RunAlways.Instance;
    public SystemDescriptor Descriptor { get; init; } = descriptor;
    public Resources Resources { get; init; } = new();

    public virtual bool ShouldRun(World world) => RunCriteria.ShouldRun(world);
    public virtual void Tick(World world)
    {
        OnTick();
    }

    protected abstract void OnTick();
}

public abstract class Sys<T0> : Sys
{
    static readonly HashSet<Type> dependencies;
    public static new HashSet<Type> Dependencies => dependencies;

    static readonly Fetch.Info t0Fetch;
    static Sys()
    {
#pragma warning disable IL2059
        RuntimeHelpers.RunClassConstructor(typeof(T0).TypeHandle);
#pragma warning restore IL2059
        t0Fetch = Fetch.Get<T0>();
        dependencies = [.. t0Fetch.Dependencies];
    }

    public Sys(SystemDescriptor descriptor) : base(descriptor)
    {
        descriptor.Parameters.Add(typeof(T0));
        descriptor.Dependencies.UnionWith(dependencies);
    }

    public override void Tick(World world)
    {
        var t0 = ((IFetch<T0>)t0Fetch.Fetch).DoFetch(world, this);

        OnTick(t0);
    }

    protected override void OnTick() { }
    protected abstract void OnTick(T0 arg1);
}

public class FnSystem(SystemDescriptor descriptor, SystemDelegate onTick) : Sys(descriptor)
{
    readonly SystemDelegate onTick = onTick;

    protected override void OnTick()
    {
        onTick();
    }
}

public class FnSystem<T0> : Sys<T0>
{
    readonly SystemDelegate<T0> onTick;

    public FnSystem(SystemDescriptor descriptor, SystemDelegate<T0> onTick) : base(descriptor)
    {
        this.onTick = onTick;
    }

    protected override void OnTick(T0 arg1)
    {
        onTick(arg1);
    }
}