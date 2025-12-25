namespace Pollus.ECS;

public interface ISystemSet
{
    public static abstract void AddToSchedule(Schedule schedule);
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class SystemSetAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Field)]
public sealed class SystemAttribute : Attribute
{
    public string CallbackMethod { get; }

    public SystemAttribute(string callbackMethod)
    {
        CallbackMethod = callbackMethod;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class CoroutineAttribute : Attribute
{
    public string CallbackMethod { get; }

    public CoroutineAttribute(string callbackMethod)
    {
        CallbackMethod = callbackMethod;
    }
}