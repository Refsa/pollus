namespace Pollus.ECS;

using Pollus.ECS.Core;

public partial class SystemBuilder
{
    public ISystem System { get; }

    public SystemBuilder(ISystem system)
    {
        System = system;
    }

    public ISystem Build()
    {
        return System;
    }

    public SystemBuilder Before(SystemLabel label)
    {
        System.Descriptor.Before(label);
        return this;
    }

    public SystemBuilder After(SystemLabel label)
    {
        System.Descriptor.After(label);
        return this;
    }

    public SystemBuilder DependsOn<T>()
    {
        System.Descriptor.DependsOn<T>();
        return this;
    }

    public SystemBuilder RunCriteria(IRunCriteria runCriteria)
    {
        System.RunCriteria = runCriteria;
        return this;
    }

    public SystemBuilder Exclusive()
    {
        System.Descriptor.DependsOn<ExclusiveSystemMarker>();
        return this;
    }

    public SystemBuilder InitLocal<T>(T value)
    {
        System.Resources.Add(new Local<T>(value));
        return this;
    }

    public static SystemBuilder FnSystem(SystemLabel label, SystemDelegate onTick)
    {
        return new SystemBuilder(new FnSystem(new SystemDescriptor(label), onTick));
    }

    public static SystemBuilder FnSystem<T1>(SystemLabel label, SystemDelegate<T1> onTick)
    {
        return new SystemBuilder(new FnSystem<T1>(new SystemDescriptor(label), onTick));
    }
}
