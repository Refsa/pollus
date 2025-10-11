#pragma warning disable IL2062
#pragma warning disable IL2059

namespace Pollus.ECS;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

public delegate bool FilterArchetypeDelegate(Archetype archetype);
public delegate bool FilterChunkDelegate(in ArchetypeChunk chunk);

public interface IFilter : ITuple
{
    bool Filter(Archetype archetype);
    bool FilterChunk(ArchetypeChunk chunk) => true;
}

public interface IFilterChunk : IFilter
{
    new bool FilterChunk(ArchetypeChunk chunk);
}

public class NoFilter() : IFilter
{
    static readonly NoFilter instance = new();
    public static IFilter Instance => instance;
    static NoFilter() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 0;

    public bool Filter(Archetype archetype) => true;
}

public class None<C0>() : IFilter
    where C0 : unmanaged, IComponent
{
    static readonly None<C0> instance = new();
    public static IFilter Instance => instance;
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];
    static None() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent(componentIDs[0]) is false;
    }
}

public class All<C0>() : IFilter
    where C0 : unmanaged, IComponent
{
    static readonly All<C0> instance = new();
    public static IFilter Instance => instance;
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];
    static All() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent(componentIDs[0]) is true;
    }
}

public class Any<C0, C1>() : IFilter
    where C0 : unmanaged, IComponent
    where C1 : unmanaged, IComponent
{
    static readonly Any<C0, C1> instance = new();
    public static IFilter Instance => instance;
    static ComponentID[] componentIDs = [Component.Register<C0>().ID, Component.Register<C1>().ID];
    static Any() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public bool Filter(Archetype archetype)
    {
        return archetype.HasAny(componentIDs) is true;
    }
}

public class Added<C0>() : IFilterChunk
    where C0 : unmanaged, IComponent
{
    static readonly Added<C0> instance = new();
    public static IFilter Instance => instance;
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];
    static Added() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent(componentIDs[0]) is true;
    }

    public bool FilterChunk(ArchetypeChunk chunk)
    {
        return chunk.HasFlag<C0>(-1, ComponentFlags.Added);
    }
}

public class Removed<C0>() : IFilterChunk
    where C0 : unmanaged, IComponent
{
    static readonly Removed<C0> instance = new();
    public static IFilter Instance => instance;
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];
    static Removed() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent(componentIDs[0]) is false;
    }

    public bool FilterChunk(ArchetypeChunk chunk)
    {
        return chunk.HasFlag<C0>(-1, ComponentFlags.Removed);
    }
}

public class Changed<C0>() : IFilterChunk
    where C0 : unmanaged, IComponent
{
    static readonly Changed<C0> instance = new();
    public static IFilter Instance => instance;
    static ComponentID[] componentIDs = [Component.Register<C0>().ID];
    static Changed() => FilterLookup.Register(instance);

    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent(componentIDs[0]) is true;
    }

    public bool FilterChunk(ArchetypeChunk chunk)
    {
        return chunk.HasFlag<C0>(-1, ComponentFlags.Changed);
    }
}

public class Combine<T0, T1>() : IFilter
    where T0 : IFilter, new()
    where T1 : IFilter, new()
{
    static readonly Combine<T0, T1> instance = new();
    public static IFilter Instance => instance;
    static Combine() => FilterLookup.Register(instance);

    T0 t0 = new();
    T1 t1 = new();

    public object? this[int index] => null;
    public int Length => 2;

    public bool Filter(Archetype archetype)
    {
        return t0.Filter(archetype) && t1.Filter(archetype);
    }
}

public static class FilterLookup
{
    static class Lookup<T> where T : IFilter
    {
        public static IFilter? Instance;
    }

    static readonly ConcurrentDictionary<Type, IFilter> lookup = new();

    public static void Register<T>(T filter) where T : IFilter
    {
        Lookup<T>.Instance = filter;
        lookup.TryAdd(typeof(T), Lookup<T>.Instance);
    }

    public static IFilter? Get<T>() where T : IFilter
    {
        return Lookup<T>.Instance;
    }

    public static IFilter? Get(Type type)
    {
        if (lookup.TryGetValue(type, out var instance))
        {
            return instance;
        }

        return null;
    }
}

public static class FilterHelpers
{
    public static IFilter[] UnwrapFilters<TFilters>()
        where TFilters : ITuple, new()
    {
        if (typeof(TFilters).Name.StartsWith("ValueTuple") is false)
        {
            if (typeof(TFilters).IsAssignableTo(typeof(IFilter)) is false)
            {
                throw new ArgumentException("Type must implement IFilter");
            }

            RuntimeHelpers.RunClassConstructor(typeof(TFilters).TypeHandle);
            return [FilterLookup.Get(typeof(TFilters))!];
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

            RuntimeHelpers.RunClassConstructor(types[i].TypeHandle);
            filters[i] = FilterLookup.Get(types[i])!;
        }
        return filters;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool RunArchetypeFilters(Archetype archetype, IFilter[] filters)
    {
        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i].Filter(archetype) is false) return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool RunChunkFilters(in ArchetypeChunk chunk, IFilter[] filters)
    {
        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i] is not IFilterChunk) continue;
            if (filters[i].FilterChunk(chunk) is false) return false;
        }
        return true;
    }
}

#pragma warning restore IL2062
#pragma warning restore IL2059