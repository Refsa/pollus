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
    World singleComponentWorld;

    public QueryBenchmarks()
    {
        singleComponentWorld = new();
        for (int i = 0; i < SIZE; i++)
        {
            Entity.With(new Component1()).Spawn(singleComponentWorld);
        }
    }

    ~QueryBenchmarks()
    {
        singleComponentWorld.Dispose();
    }

    /* [Benchmark]
    public void Query_ForEach_IForEach()
    {
        var q = new Query<Component1>(singleComponentWorld);
        q.ForEach(new ForEach());
    }

    [Benchmark]
    public void Query_ForEach_IChunkForEach()
    {
        var q = new Query<Component1>(singleComponentWorld);
        q.ForEach(new ChunkForEach());
    } */

    [Benchmark]
    public void Query_ForEach_Delegate()
    {
        var q = new Query<Component1>(singleComponentWorld);
        q.ForEach((ref Component1 c) =>
        {
            c.First++;
        });
    }

    struct ForEach : IForEach<Component1>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Execute(scoped ref Component1 c)
        {
            c.First++;
        }
    }

    struct ChunkForEach : IChunkForEach<Component1>
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