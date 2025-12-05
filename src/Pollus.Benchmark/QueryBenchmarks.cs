namespace Pollus.Benchmark;

using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Pollus.ECS;

[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
// [SimpleJob(RuntimeMoniker.[SimpleJob(RuntimeMoniker.Net10_0)]0)]
// [SimpleJob(RuntimeMoniker.Net80)]
// [HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class QueryBenchmarks
{
    const int SIZE = 100_000;
    World oneComponentWorld;
    World twoComponentWorld;

    public QueryBenchmarks()
    {
        oneComponentWorld = new();
        twoComponentWorld = new();
        for (int i = 0; i < SIZE; i++)
        {
            Entity.With(new Component1()).Spawn(oneComponentWorld);
            Entity.With(new Component1(), new Component2()).Spawn(twoComponentWorld);
        }
    }

    ~QueryBenchmarks()
    {
        oneComponentWorld.Dispose();
        twoComponentWorld.Dispose();
    }

    [Benchmark]
    public int Query_One_ForEach_Enumerator_ChangeTracking()
    {
        var query = new Query(oneComponentWorld);
        var q = new Query<Component1>(oneComponentWorld);
        foreach (var row in q)
        {
            row.Component0.First++;
            query.SetChanged<Component1>(row.Entity);
        }

        return 0;
    }

    [Benchmark]
    public int Query_One_ForEach_IForEach()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(new ForEachOne());
        return 0;
    }

    [Benchmark]
    public int Query_One_ForEach_Delegate()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(static (ref Component1 c) => c.First++);
        return 0;
    }

    [Benchmark]
    public int Query_One_ForEach_Delegate_UserData()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(1, static (in int inc, ref Component1 c) => c.First += inc);
        return 0;
    }

    [Benchmark]
    public int Query_One_ForEach_IChunkForEach()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(new ChunkForEachOne());
        return 0;
    }

    [Benchmark]
    public int Query_One_ForEach_Enumerator()
    {
        var q = new Query<Component1>(oneComponentWorld);
        foreach (var row in q)
        {
            row.Component0.First++;
        }

        return 0;
    }

    [Benchmark]
    public int Query_One_ForEach_IEntityForEach()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(new ForEachOne_Entity());
        return 0;
    }

    [Benchmark]
    public void Query_Two_ForEach_IForEach()
    {
        var q = new Query<Component1, Component2>(twoComponentWorld);
        q.ForEach(new ForEachTwo());
    }

    [Benchmark]
    public void Query_Two_ForEach_Delegate()
    {
        var q = new Query<Component1, Component2>(twoComponentWorld);
        q.ForEach(static (ref Component1 c1, ref Component2 c2) => c1.First += c2.First);
    }

    [Benchmark]
    public int Query_Two_ForEach_Enumerator()
    {
        var q = new Query<Component1, Component2>(twoComponentWorld);
        foreach (var row in q)
        {
            row.Component0.First += row.Component1.First;
        }

        return 0;
    }

    struct ForEachOne : IForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly void Execute(ref Component1 c)
        {
            c.First++;
        }
    }

    struct ForEachOne_Entity : IEntityForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly void Execute(in Entity e, ref Component1 c)
        {
            c.First++;
        }
    }

    struct ForEachTwo : IForEach<Component1, Component2>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly void Execute(ref Component1 c1, ref Component2 c2)
        {
            c1.First += c2.First;
        }
    }

    struct ChunkForEachOne : IChunkForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public readonly void Execute(in Span<Component1> chunk0)
        {
            for (int i = 0; i < chunk0.Length; i++)
            {
                chunk0[i].First++;
            }
        }
    }
}