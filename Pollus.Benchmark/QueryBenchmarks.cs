namespace Pollus.Benchmark;

using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Pollus.ECS;

[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
[SimpleJob(RuntimeMoniker.Net80)]
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
    public void Query_One_ForEach_IForEach()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(new ForEachOne());
    }

    [Benchmark]
    public void Query_One_ForEach_IChunkForEach()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach(new ChunkForEachOne());
    }

    [Benchmark]
    public void Query_One_ForEach_Delegate()
    {
        var q = new Query<Component1>(oneComponentWorld);
        q.ForEach((ref Component1 c) => c.First++);
    }

    [Benchmark]
    public void Query_Two_ForEach_IForEach()
    {
        var q = new Query<Component1, Component2>(twoComponentWorld);
    }

    struct ForEachOne : IForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute(ref Component1 c)
        {
            c.First++;
        }
    }

    struct ForEachTwo : IForEach<Component1, Component2>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute(ref Component1 c1, ref Component2 c2)
        {
            c1.First += c2.First;
        }
    }

    struct ChunkForEachOne : IChunkForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute(in Span<Component1> chunk0)
        {
            for (int i = 0; i < chunk0.Length; i++)
            {
                chunk0[i].First++;
            }
        }
    }
}