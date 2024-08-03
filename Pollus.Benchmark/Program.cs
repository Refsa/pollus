namespace Pollus.Benchmark;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.NativeAot;

public static class Program
{
    public static void Main()
    {
        // BenchmarkRunner.Run<TestBenchmarks>();
        // BenchmarkRunner.Run<NativeMapBenchmarks>();

        // BenchmarkRunner.Run<SpawnBenchmarks>();
        BenchmarkRunner.Run<QueryBenchmarks>();
    }
}