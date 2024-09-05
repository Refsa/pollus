namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.ECS.Core;

public interface ISystemParam
{

}

public struct Param<T0> : IFetch<Param<T0>>, ISystemParam
{
    static readonly HashSet<Type> dependencies;
    public static HashSet<Type> Dependencies => dependencies;
    static readonly Fetch.Info t0Fetch;
    static Param()
    {
#pragma warning disable IL2059
        RuntimeHelpers.RunClassConstructor(typeof(T0).TypeHandle);
#pragma warning restore IL2059
        t0Fetch = Fetch.Get<T0>();
        dependencies = [.. t0Fetch.Dependencies];

        Register();
    }

    public T0 Param0;

    public static void Register()
    {
        Fetch.Register<Param<T0>>(new Param<T0>(), [.. dependencies]);
    }

    public Param<T0> DoFetch(World world, ISystem system)
    {
        var t0 = ((IFetch<T0>)t0Fetch.Fetch).DoFetch(world, system);
        return new Param<T0> { Param0 = t0 };
    }
}

public struct Param<T0, T1> : IFetch<Param<T0, T1>>, ISystemParam
{
    static readonly HashSet<Type> dependencies;
    public static HashSet<Type> Dependencies => dependencies;
    static readonly Fetch.Info t0Fetch;
    static readonly Fetch.Info t1Fetch;
    static Param()
    {
#pragma warning disable IL2059
        RuntimeHelpers.RunClassConstructor(typeof(T0).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(T1).TypeHandle);
#pragma warning restore IL2059
        t0Fetch = Fetch.Get<T0>();
        t1Fetch = Fetch.Get<T1>();
        dependencies = [.. t0Fetch.Dependencies, .. t1Fetch.Dependencies];

        Register();
    }

    public static void Register()
    {
        Fetch.Register<Param<T0, T1>>(new Param<T0, T1>(), [.. dependencies]);
    }

    public T0 Param0;
    public T1 Param1;

    public Param<T0, T1> DoFetch(World world, ISystem system)
    {
        var t0 = ((IFetch<T0>)t0Fetch.Fetch).DoFetch(world, system);
        var t1 = ((IFetch<T1>)t1Fetch.Fetch).DoFetch(world, system);
        return new Param<T0, T1> { Param0 = t0, Param1 = t1 };
    }
}