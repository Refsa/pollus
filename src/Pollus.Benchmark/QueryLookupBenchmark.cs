namespace Pollus.Benchmark;

using BenchmarkDotNet.Attributes;
using Pollus.ECS;

[MemoryDiagnoser]
public class QueryLookupBenchmark
{
    World oneComponentWorld;

    public QueryLookupBenchmark()
    {
        oneComponentWorld = new();
        for (int i = 0; i < 1_000; i++)
        {
            Entity.With(new Component1()).Spawn(oneComponentWorld);
        }
    }

    ~QueryLookupBenchmark()
    {
        oneComponentWorld.Dispose();
    }

    [Benchmark]
    public void Query_TryGet()
    {
        var query = new Query(oneComponentWorld);
        query.ForEach(query, static (in Query query, in Entity entity) =>
        {
            ref var component1 = ref query.TryGet<Component1>(entity, out var hasComponent);
            if (hasComponent)
            {
                component1.First++;
            }
        });
    }

    [Benchmark]
    public void Query_HasGet()
    {
        var query = new Query(oneComponentWorld);
        query.ForEach(query, static (in Query query, in Entity entity) =>
        {
            if (query.Has<Component1>(entity))
            {
                ref var component1 = ref query.Get<Component1>(entity);
                component1.First++;
            }
        });
    }

    [Benchmark]
    public void Query_HasGet_EntityInfo()
    {
        var query = new Query(oneComponentWorld);
        query.ForEach(query, static (in Query query, in Entity entity) =>
        {
            if (query.Has<Component1>(entity, out var entityInfo))
            {
                ref var component1 = ref query.Get<Component1>(entityInfo);
                component1.First++;
            }
        });
    }
}
