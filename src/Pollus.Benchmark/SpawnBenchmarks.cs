namespace Pollus.Benchmark;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Pollus.ECS;

[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
[SimpleJob(RuntimeMoniker.Net80)]
// [HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class SpawnBenchmarks
{
    const int SIZE = 100_000;

    [Benchmark]
    public void ArchetypeStore_CreateEntityWithOneComponent()
    {
        using var world = new World();
        for (int i = 0; i < SIZE; i++)
        {
            Entity.With(new Component1()).Spawn(world);
        }
    }

    [Benchmark]
    public void ArchetypeStore_CreateEntityWithTwoComponents()
    {
        using var world = new World();
        for (int i = 0; i < SIZE; i++)
        {
            world.Spawn(new Component1(), new Component2());
        }
    }

    [Benchmark]
    public void ArchetypeStore_CreateEntityWithThreeComponents()
    {
        using var world = new World();
        for (int i = 0; i < SIZE; i++)
        {
            world.Spawn(new Component1(), new Component2(), new Component3());
        }
    }

    /* [Benchmark]
    public void ArchetypeStore_CreateEntityWithThreeComponents_AddComponent()
    {
        using var world = new World();
        for (int i = 0; i < SIZE; i++)
        {
            var entity = world.Spawn();
            world.Store.AddComponent(entity, new Component1());
            world.Store.AddComponent(entity, new Component2());
            world.Store.AddComponent(entity, new Component3());
        }
    } */

    [Benchmark]
    public void ArchetypeStore_CreateEntityWithFourComponents()
    {
        using var world = new World();
        for (int i = 0; i < SIZE; i++)
        {
            world.Spawn(new Component1(), new Component2(), new Component3(), new Component4());
        }
    }
}