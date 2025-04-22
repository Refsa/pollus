using Pollus.Debugging;

namespace Pollus.ECS;

public interface ISystemBuilder
{
    ISystem Build();
}

public struct SystemBuilderDescriptor
{
    public SystemLabel Label { get; set; }
    public StageLabel Stage { get; set; }
    public HashSet<SystemLabel> RunsBefore { get; init; }
    public HashSet<SystemLabel> RunsAfter { get; init; }
    public HashSet<Type> Dependencies { get; init; }
    public HashSet<Local> Locals { get; init; }
    public IRunCriteria RunCriteria { get; init; }
    public bool IsExclusive { get; init; } = false;

    public SystemBuilderDescriptor()
    {
        RunsBefore = [];
        RunsAfter = [];
        Dependencies = [];
        Locals = [];
        RunCriteria = RunAlways.Instance;
    }

    public SystemBuilderDescriptor(SystemLabel label) : this()
    {
        Label = label;
    }

    public SystemBuilderDescriptor(SystemLabel label, StageLabel stage) : this(label)
    {
        Stage = stage;
    }

    public static implicit operator SystemDescriptor(SystemBuilderDescriptor builder)
    {
        var descriptor = new SystemDescriptor(builder.Label);
        if (builder.RunsBefore != null)
        {
            descriptor.Before(builder.RunsBefore);
        }
        if (builder.RunsAfter != null)
        {
            descriptor.After(builder.RunsAfter);
        }
        if (builder.Dependencies != null)
        {
            descriptor.DependsOn(builder.Dependencies);
        }
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
