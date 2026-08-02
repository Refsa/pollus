namespace Pollus.ECS;

public interface ISystemSet
{
    public static abstract void AddToSchedule(Schedule schedule);
}

public interface ISystemLabelProvider
{
    static abstract string Label { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class SystemSetAttribute : Attribute
{
    public SystemSetAttribute()
    {

    }

    public SystemSetAttribute(object group)
    {

    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class SystemAttribute : Attribute
{
    public string? Stage { get; }
    public string? Label { get; set; }
    public string[] RunsAfter { get; set; } = [];
    public string[] RunsBefore { get; set; } = [];
    public Type[] Dependencies { get; set; } = [];
    public bool IsExclusive { get; set; } = false;

    /// <summary>
    /// A callback method to modify SystemBuilderDescriptor
    /// </summary>
    public string? DescriptorCallback { get; set; }

    public SystemAttribute() { }

    public SystemAttribute(string stage)
    {
        Stage = stage;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RunsAfterAttribute<T> : Attribute
    where T : ISystemLabelProvider
{

}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RunsBeforeAttribute<T> : Attribute
    where T : ISystemLabelProvider
{

}

[AttributeUsage(AttributeTargets.Method)]
public sealed class RunCriteriaAttribute<T> : Attribute
    where T : IRunCriteria
{
    public RunCriteriaAttribute() { }
    public RunCriteriaAttribute(float value) { }
    public RunCriteriaAttribute(string factory) { }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class LocalAttribute<T> : Attribute
{
    public LocalAttribute() { }
    public LocalAttribute(T value) { }
    public LocalAttribute(string factory) { }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class OnStateEnterAttribute : Attribute
{
    public OnStateEnterAttribute(object value) { }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class OnStateExitAttribute : Attribute
{
    public OnStateExitAttribute(object value) { }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class InStateAttribute : Attribute
{
    public InStateAttribute(object value) { }
}
