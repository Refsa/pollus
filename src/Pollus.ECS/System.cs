#pragma warning disable IL2062

namespace Pollus.ECS;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Pollus.Debugging;

public delegate void SystemDelegate();
public delegate void SystemDelegate<T0>(T0 arg1);

public interface ISystem
{
    static HashSet<Type> Dependencies => [];

    IRunCriteria RunCriteria { get; set; }
    SystemDescriptor Descriptor { get; }
    Resources Resources { get; }

    bool ShouldRun(World world) => RunCriteria.ShouldRun(world);
    void Tick(World world) { }
}

public abstract class SystemBase(SystemDescriptor descriptor) : ISystem, IDisposable
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

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        Resources.Dispose();
    }
}

public abstract class SystemBase<T0> : SystemBase
{
    static readonly HashSet<Type> dependencies;
    public static new HashSet<Type> Dependencies => dependencies;

    static readonly Fetch.Info t0Fetch;
    static SystemBase()
    {
#pragma warning disable IL2059
        RuntimeHelpers.RunClassConstructor(typeof(T0).TypeHandle);
#pragma warning restore IL2059
        t0Fetch = Fetch.Get<T0>();
        dependencies = [.. t0Fetch.Dependencies];
    }

    public SystemBase(SystemDescriptor descriptor) : base(descriptor)
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

public partial class FnSystem(SystemDescriptor descriptor, SystemDelegate onTick) : SystemBase(descriptor)
{
    readonly SystemDelegate onTick = onTick;

    protected override void OnTick()
    {
        onTick();
    }

    public static FnSystem Create(SystemBuilderDescriptor descriptor, SystemDelegate onTick)
    {
        var system = new FnSystem(descriptor, onTick)
        {
            RunCriteria = descriptor.RunCriteria
        };
        if (descriptor.IsExclusive) system.Descriptor.DependsOn<ExclusiveSystemMarker>();
        foreach (var local in descriptor.Locals) system.Resources.Add(local, local.TypeID);
        return system;
    }

    public static FnSystem<T0> Create<T0>(SystemBuilderDescriptor descriptor, SystemDelegate<T0> onTick)
    {
        var system = new FnSystem<T0>(descriptor, onTick)
        {
            RunCriteria = descriptor.RunCriteria,
        };
        if (descriptor.IsExclusive) system.Descriptor.DependsOn<ExclusiveSystemMarker>();
        foreach (var local in descriptor.Locals) system.Resources.Add(local, local.TypeID);
        return system;
    }
}

public class FnSystem<T0> : SystemBase<T0>
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

#pragma warning restore IL2062