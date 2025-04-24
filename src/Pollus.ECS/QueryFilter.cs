namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public interface IQueryFilter
{
    static abstract FilterArchetypeDelegate FilterArchetype { get; }
    static abstract FilterChunkDelegate FilterChunk { get; }
}

public class QueryFilter<TFilters> : IQueryFilter
    where TFilters : ITuple, new()
{
    static readonly IFilter[] filters;

    static readonly FilterArchetypeDelegate filterArchetype = RunArchetypeFilter;
    static readonly FilterChunkDelegate filterChunk = RunChunkFilter;

    public static FilterArchetypeDelegate FilterArchetype => filterArchetype;
    public static FilterChunkDelegate FilterChunk => filterChunk;

    static QueryFilter()
    {
        filters = FilterHelpers.UnwrapFilters<TFilters>();
    }

    static bool RunArchetypeFilter(Archetype archetype) => FilterHelpers.RunArchetypeFilters(archetype, filters);
    static bool RunChunkFilter(in ArchetypeChunk chunk) => FilterHelpers.RunChunkFilters(chunk, filters);
}

