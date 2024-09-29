#pragma warning disable IL2062

namespace Pollus.ECS;

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

public class Added<C0> : IFilterChunk
    where C0 : unmanaged, IComponent
{
    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is true;
    }

    public bool FilterChunk(ArchetypeChunk chunk)
    {
        return chunk.HasFlag<C0>(-1, ComponentFlags.Added);
    }
}

public class Removed<C0> : IFilterChunk
    where C0 : unmanaged, IComponent
{
    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is false;
    }

    public bool FilterChunk(ArchetypeChunk chunk)
    {
        return chunk.HasFlag<C0>(-1, ComponentFlags.Removed);
    }
}

public class Changed<C0> : IFilterChunk
    where C0 : unmanaged, IComponent
{
    public object? this[int index] => null;
    public int Length => 1;

    public bool Filter(Archetype archetype)
    {
        return archetype.HasComponent<C0>() is true;
    }

    public bool FilterChunk(ArchetypeChunk chunk)
    {
        return chunk.HasFlag<C0>(-1, ComponentFlags.Changed);
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

#pragma warning disable IL2062
            filters[i] = (IFilter)Activator.CreateInstance(types[i])!;
#pragma warning restore IL2062
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