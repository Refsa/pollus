namespace Pollus.ECS.Core;

using System.Runtime.CompilerServices;

public class ExclusiveSystemMarker { }

public delegate void SystemDelegate();
public delegate void SystemDelegate<T1>(T1 arg1);
public delegate void SystemDelegate<T1, T2>(T1 arg1, T2 arg2);

public record struct SystemLabel(string Label)
{
    public static implicit operator SystemLabel(string label) => new(label);
}

public class SystemDescriptor
{
    public SystemLabel Label { get; }
    public HashSet<SystemLabel> RunsBefore { get; } = new();
    public HashSet<SystemLabel> RunsAfter { get; } = new();
    public HashSet<Type> Dependencies { get; } = new();

    public SystemDescriptor(SystemLabel label)
    {
        Label = label;
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
    IRunCriteria RunCriteria { get; set; }
    SystemDescriptor Descriptor { get; }

    bool ShouldRun(World world) => RunCriteria.ShouldRun(world);
    void Tick(World world) { }
}

public abstract class Sys(SystemDescriptor descriptor) : ISystem
{
    public IRunCriteria RunCriteria { get; set; } = RunAlways.Instance;
    public SystemDescriptor Descriptor { get; } = descriptor;

    public virtual bool ShouldRun(World world) => RunCriteria.ShouldRun(world);
    public virtual void Tick(World world)
    {
        OnTick();
    }

    protected abstract void OnTick();
}

public abstract class Sys<T0> : Sys
{
    static readonly Fetch.Info t0Fetch;
    static Sys()
    {
        // RuntimeHelpers.RunClassConstructor(typeof(T0).TypeHandle);
        t0Fetch = Fetch.Get<T0>();
    }

    public Sys(SystemDescriptor descriptor) : base(descriptor)
    {
        descriptor.DependsOn<T0>();
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

public class FnSystem<T0>(SystemDescriptor descriptor, SystemDelegate<T0> onTick) : Sys<T0>(descriptor)
{
    readonly SystemDelegate<T0> onTick = onTick;

    protected override void OnTick(T0 arg1)
    {
        onTick(arg1);
    }
}