namespace Pollus.ECS;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public delegate bool FilterDelegate(Archetype archetype);

public interface IFilter : ITuple
{
    bool Filter(Archetype archetype);
}

public class None<C0> : IFilter
    where C0 : unmanaged, IComponent
{
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];
    public object? this[int index] => null;
    public int Length => 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is false;
    }
}

public class All<C0> : IFilter
    where C0 : unmanaged, IComponent
{
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];

    public object? this[int index] => null;
    public int Length => 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is true;
    }
}

public class Any<C0, C1> : IFilter
    where C0 : unmanaged, IComponent
    where C1 : unmanaged, IComponent
{
    static ComponentID[] componentIDs = [Component.Register<C0>().ID, Component.Register<C1>().ID];
    public object? this[int index] => null;
    public int Length => 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasAny(componentIDs) is true;
    }
}


public static class FilterHelpers
{
    public static IFilter[] UnwrapFilters<TFilters>()
        where TFilters : ITuple, new()
    {
        if (typeof(TFilters).IsAssignableFrom(typeof(ITuple)) is false)
        {
            if (typeof(TFilters).IsAssignableTo(typeof(IFilter)) is false)
            {
                throw new ArgumentException("Type must implement IFilter");
            }

            var filter = (IFilter)Activator.CreateInstance<TFilters>()!;
            return [filter];
        }

        var types = typeof(TFilters).GetGenericArguments();
        var length = types.Length;
        var filters = new IFilter[length];
        for (int i = 0; i < length; i++)
        {
            if (types[i].IsAssignableTo(typeof(IFilter)) is false)
            {
                throw new ArgumentException("Type must implement IFilter");
            }
            if (types[i].IsValueType is false)
            {
                throw new ArgumentException("Type must be a value type");
            }

#pragma warning disable IL2062
            filters[i] = (IFilter)Activator.CreateInstance(types[i])!;
#pragma warning restore IL2062
        }
        return filters;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool RunFilters(Archetype archetype, IFilter[] filters)
    {
        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i].Filter(archetype) is false) return false;
        }
        return true;
    }
}