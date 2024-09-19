#pragma warning disable IL2062

namespace Pollus.ECS;

public class ExclusiveSystemMarker { }

public record struct SystemLabel(string Label)
{
    public static implicit operator SystemLabel(string label) => new(label);
    public override string ToString() => $"System<{Label}>";
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

#pragma warning restore IL2062