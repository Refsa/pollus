using Pollus.Debugging;

namespace Pollus.ECS;

public interface ISystemBuilder
{
    ISystem Build();
}

public struct SystemBuilderDescriptor
{
    public SystemLabel Label { get; }
    public HashSet<SystemLabel> RunsBefore { get; init; } = [];
    public HashSet<SystemLabel> RunsAfter { get; init; } = [];
    public HashSet<Type> Dependencies { get; init; } = [];
    public HashSet<Local> Locals { get; init; } = [];
    public bool IsExclusive { get; init; } = false;
    public IRunCriteria RunCriteria { get; init; } = RunAlways.Instance;

    public SystemBuilderDescriptor(SystemLabel label)
    {
        Label = label;
    }

    public static implicit operator SystemDescriptor(SystemBuilderDescriptor builder)
    {
        var descriptor = new SystemDescriptor(builder.Label);
        descriptor.Before(builder.RunsBefore);
        descriptor.After(builder.RunsAfter);
        descriptor.DependsOn(builder.Dependencies);
        return descriptor;
    }

    public static implicit operator SystemBuilderDescriptor(string label)
    {
        return new(label);
    }

    public static implicit operator SystemBuilderDescriptor(SystemLabel label)
    {
        return new(label);
    }
}
