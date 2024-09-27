namespace Pollus.ECS.Generators
{
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;

    [Generator]
    public class QueryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(GenerateForEach);
            context.RegisterPostInitializationOutput(GenerateQueries);
        }

        private void GenerateQueries(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public struct Query<$gen_args$> : IQuery, IQueryCreate<Query<$gen_args$>>
    $gen_constraints$
{
    public struct Filter<TFilters> : IQuery, IQueryCreate<Filter<TFilters>>
        where TFilters : ITuple, new()
    {
        public static Component.Info[] Infos => infos;
        static readonly IFilter[] filters;
        static FilterArchetypeDelegate filterArchetype = RunArchetypeFilter;
        static FilterChunkDelegate filterChunk = RunChunkFilter;

        static Filter()
        {
            filters = FilterHelpers.UnwrapFilters<TFilters>();
            QueryFetch<Filter<TFilters>>.Register();
        }

        public static Filter<TFilters> Create(World world) => new Filter<TFilters>(world);

        static bool RunArchetypeFilter(Archetype archetype) => FilterHelpers.RunArchetypeFilters(archetype, filters);
        static bool RunChunkFilter(in ArchetypeChunk chunk) => FilterHelpers.RunChunkFilters(chunk, filters);

        public static implicit operator Query<$gen_args$>(in Filter<TFilters> filter)
        {
            return filter.query;
        }

        Query<$gen_args$> query;

        public Filter(World world)
        {
            query = new Query<$gen_args$>(world, filterArchetype, filterChunk);
        }

        public void ForEach(ForEachDelegate<$gen_args$> pred)
        {
            query.ForEach(pred);
        }

        public void ForEach(ForEachEntityDelegate<$gen_args$> pred)
        {
            query.ForEach(pred);
        }

        public void ForEach<TUserData>(in TUserData userData, ForEachUserDataDelegate<TUserData, $gen_args$> pred)
        {
            query.ForEach(userData, pred);
        }

        public void ForEach<TUserData>(in TUserData userData, ForEachEntityUserDataDelegate<TUserData, $gen_args$> pred)
        {
            query.ForEach(userData, pred);
        }

        public readonly void ForEach<TForEach>(TForEach iter)
            where TForEach : struct, IForEachBase<$gen_args$>
        {
            query.ForEach(iter);
        }

        public EntityRow Single()
        {
            return query.Single();
        }

        public int EntityCount()
        {
            return query.EntityCount();
        }

        public Enumerator GetEnumerator()
        {
            return query.GetEnumerator();
        }
    }

    static readonly Component.Info[] infos = [$infos$];
    static readonly ComponentID[] cids = [$comp_ids$];
    public static Component.Info[] Infos => infos;

    static Query<$gen_args$> IQueryCreate<Query<$gen_args$>>.Create(World world) => new Query<$gen_args$>(world);
    static Query()
    {
        QueryFetch<Query<$gen_args$>>.Register();
    }

    readonly World world;
    readonly FilterArchetypeDelegate? filterArchetype;
    readonly FilterChunkDelegate? filterChunk;

    public Query(World world, FilterArchetypeDelegate? filterArchetype = null, FilterChunkDelegate? filterChunk = null)
    {
        this.world = world;
        this.filterArchetype = filterArchetype;
        this.filterChunk = filterChunk;
    }

    public readonly void ForEach(ForEachDelegate<$gen_args$> pred)
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            $comp_spans$
            
            for (int i = 0; i < count; i++)
            {
                pred($comp_args$);
            }
        }
    }

    public readonly void ForEach<TUserData>(in TUserData userData, ForEachUserDataDelegate<TUserData, $gen_args$> pred)
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            $comp_spans$
            
            for (int i = 0; i < count; i++)
            {
                pred(in userData, $comp_args$);
            }
        }
    }

    public readonly void ForEach(ForEachEntityDelegate<$gen_args$> pred)
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            $comp_spans$
            scoped var entities = chunk.GetEntities();

            for (int i = 0; i < count; i++)
            {
                pred(entities[i], $comp_args$);
            }
        }
    }

    public readonly void ForEach<TUserData>(in TUserData userData, ForEachEntityUserDataDelegate<TUserData, $gen_args$> pred)
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            $comp_spans$
            scoped var entities = chunk.GetEntities();

            for (int i = 0; i < count; i++)
            {
                pred(in userData, entities[i], $comp_args$);
            }
        }
    }

    public readonly void ForEach<TForEach>(TForEach iter)
        where TForEach : struct, IForEachBase<$gen_args$>
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            var count = chunk.Count;
            $comp_spans$

            if (iter is IForEach<$gen_args$>)
            {
                for (int i = 0; i < count; i++)
                {
                    iter.Execute($comp_args$);
                }
            }
            else if (iter is IEntityForEach<$gen_args$>)
            {
                scoped var entities = chunk.GetEntities();
                for (int i = 0; i < count; i++)
                {
                    iter.Execute(entities[i], $comp_args$);
                }
            }
            else if (iter is IChunkForEach<$gen_args$>)
            {
                iter.Execute($chunk_args$);
            }
        }
    }

    public int EntityCount()
    {
        int count = 0;
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            count += chunk.Count;
        }
        return count;
    }

    public EntityRow Single()
    {
        foreach (ref var chunk in new ArchetypeChunkEnumerable(world.Store.Archetypes, cids, filterArchetype, filterChunk))
        {
            return new EntityRow
            {
                entity = ref chunk.GetEntity(0),
                $set_entity_row$
            };
        }

        throw new InvalidOperationException(""No entities found"");
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ref struct Enumerator
    {
        ArchetypeChunkEnumerable chunks;
        ArchetypeChunkEnumerable.ChunkEnumerator chunksEnumerator;
        ref Entity endEntity;
        EntityRow currentRow;

        public Enumerator(scoped in Query<$gen_args$> query)
        {
            chunks = new ArchetypeChunkEnumerable(query.world.Store.Archetypes, cids, query.filterArchetype, query.filterChunk);
            chunksEnumerator = chunks.GetEnumerator();
        }

        public EntityRow Current => currentRow;

        public bool MoveNext()
        {
            if (!Unsafe.IsNullRef(ref currentRow.entity) && Unsafe.IsAddressLessThan(ref currentRow.entity, ref endEntity))
            {
                currentRow.entity = ref Unsafe.Add(ref currentRow.entity, 1);
                $enumerator_move_next$
                return true;
            }

            if (!chunksEnumerator.MoveNext()) return false;

            scoped ref var currentChunk = ref chunksEnumerator.Current;
            currentRow.entity = ref currentChunk.GetEntity(0);
            endEntity = ref Unsafe.Add(ref currentRow.entity, currentChunk.Count - 1);
            $enumerator_set_fields$
            return true;
        }
    }

    public ref struct EntityRow
    {
        internal ref Entity entity;
        public readonly ref readonly Entity Entity => ref entity;
        $entity_row_fields$
    }
}";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var infos = "Component.Register<C0>()";
            var comp_ids = "infos[0].ID";
            var comp_args = "ref comp0[i]";
            var comp_spans = "scoped var comp0 = chunk.GetComponents<C0>(cids[0]);";
            var chunk_args = "comp0";
            var set_entity_row = "Component0 = ref chunk.GetComponents<C0>(cids[0])[0],";
            var entity_row_fields = "public ref C0 Component0;";
            var enumerator_set_fields = "currentRow.Component0 = ref currentChunk.GetComponents<C0>(cids[0])[0];";
            var enumerator_move_next = "currentRow.Component0 = ref Unsafe.Add(ref currentRow.Component0, 1);";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\nwhere C{i} : unmanaged, IComponent";
                infos += $", Component.Register<C{i}>()";
                comp_ids += $", infos[{i}].ID";
                comp_args += $", ref comp{i}[i]";
                comp_spans += $"\nscoped var comp{i} = chunk.GetComponents<C{i}>(cids[{i}]);";
                chunk_args += $", comp{i}";
                set_entity_row += $"\n                    Component{i} = ref chunk.GetComponents<C{i}>(cids[{i}])[0],";
                entity_row_fields += $"\n        public ref C{i} Component{i};";
                enumerator_set_fields += $"\n            currentRow.Component{i} = ref currentChunk.GetComponents<C{i}>(cids[{i}])[0];";
                enumerator_move_next += $"\n            currentRow.Component{i} = ref Unsafe.Add(ref currentRow.Component{i}, 1);";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_args$", gen_args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$infos$", infos)
                    .Replace("$comp_ids$", comp_ids)
                    .Replace("$gen_count$", (i + 1).ToString())
                    .Replace("$comp_spans$", comp_spans)
                    .Replace("$comp_args$", comp_args)
                    .Replace("$chunk_args$", chunk_args)
                    .Replace("$set_entity_row$", set_entity_row)
                    .Replace("$entity_row_fields$", entity_row_fields)
                    .Replace("$enumerator_set_fields$", enumerator_set_fields)
                    .Replace("$enumerator_move_next$", enumerator_move_next);

                context.AddSource($"Query{i + 1}.gen.cs", sb.ToString());
            }
        }

        private void GenerateForEach(IncrementalGeneratorPostInitializationContext context)
        {
            const string TEMPLATE =
@"namespace Pollus.ECS;
using System.Runtime.CompilerServices;

public delegate void ForEachDelegate<$gen_args$>($args$) 
    $gen_constraints$;

public delegate void ForEachUserDataDelegate<TUserData, $gen_args$>(scoped in TUserData userData, $args$)
    $gen_constraints$;

public delegate void ForEachEntityDelegate<$gen_args$>(scoped in Entity entity, $args$)
    $gen_constraints$;

public delegate void ForEachEntityUserDataDelegate<TUserData, $gen_args$>(scoped in TUserData userData, scoped in Entity entity, $args$)
    $gen_constraints$;

public interface IForEachBase<$gen_args$>
    $gen_constraints$
{
    void Execute($args$) { }
    void Execute(in Entity entity, $args$) { }
    void Execute($chunk_spans$) { }
}

public interface IForEach<$gen_args$> : IForEachBase<$gen_args$>
    $gen_constraints$
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute($args$);
}

public interface IEntityForEach<$gen_args$> : IForEachBase<$gen_args$>
    $gen_constraints$
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(in Entity entity, $args$);
}

public interface IChunkForEach<$gen_args$> : IForEachBase<$gen_args$>
    $gen_constraints$
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute($chunk_spans$);
}
";

            var sb = new StringBuilder();
            var gen_args = "C0";
            var gen_constraints = "where C0 : unmanaged, IComponent";
            var args = "ref C0 c0";
            var chunk_spans = "in Span<C0> chunk0";

            for (int i = 1; i < 16; i++)
            {
                gen_args += $", C{i}";
                gen_constraints += $"\nwhere C{i} : unmanaged, IComponent";
                args += $", ref C{i} c{i}";
                chunk_spans += $", in Span<C{i}> chunk{i}";

                sb.Clear().Append(TEMPLATE)
                    .Replace("$gen_args$", gen_args)
                    .Replace("$args$", args)
                    .Replace("$gen_constraints$", gen_constraints)
                    .Replace("$chunk_spans$", chunk_spans);

                context.AddSource($"ForEach{i + 1}.gen.cs", sb.ToString());
            }
        }
    }
}