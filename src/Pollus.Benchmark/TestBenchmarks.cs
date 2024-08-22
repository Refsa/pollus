namespace Pollus.Benchmark;

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Collections;


[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
[SimpleJob(RuntimeMoniker.Net80)]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class TestBenchmarks
{
    const int SIZE = 100_000;
    Component1[] array;
    NativeArray<Component1> nativeArray;
    ArchetypeChunk chunk;
    Archetype archetype;
    World world;

    public TestBenchmarks()
    {
        array = new Component1[SIZE];
        nativeArray = new(SIZE);
        chunk = new([Component.GetInfo<Component1>().ID], SIZE);
        chunk.SetCount(SIZE);
        archetype = new([Component.GetInfo<Component1>().ID]);
        world = new();

        for (int i = 0; i < SIZE; i++)
        {
            var comp = new Component1 { First = i };

            array[i] = comp;
            nativeArray[i] = comp;
            chunk.SetComponent(i, comp);

            var entity = new Entity(i + 1);
            var entityInfo = archetype.AddEntity(entity);
            archetype.SetComponent(entityInfo.ChunkIndex, entityInfo.RowIndex, comp);

            world.Spawn(Entity.With(new Component1()));
        }
    }

    ~TestBenchmarks()
    {
        nativeArray.Dispose();
        chunk.Dispose();
        archetype.Dispose();
        world.Dispose();
    }

    /* [Benchmark]
    public int ArrayIteration()
    {
        for (int i = 0; i < SIZE; i++)
        {
            array[i].First++;
        }
        return array[0].First;
    } */

    /* [Benchmark]
    public int SpanArrayIteration()
    {
        var span = new Span<Thing>(array);
        var sum = 0;
        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i].First + span[i].Second;
        }
        return sum;
    } */

    /* [Benchmark]
    public int NativeArrayIteration()
    {
        var sum = 0;
        for (int i = 0; i < SIZE; i++)
        {
            sum += nativeArray[i];
        }
        return sum;
    } */

    /* [Benchmark]
    public int SpanNativeArrayIteration()
    {
        var span = nativeArray.AsSpan();
        var sum = 0;
        for (int i = 0; i < span.Length; i++)
        {
            sum += span[i].First + span[i].Second;
        }
        return sum;
    } */

    /* [Benchmark]
    public int RefNativeArrayIteration()
    {
        var span = nativeArray.AsSpan();
        ref var current = ref span[0];
        for (int i = 0; i < nativeArray.Length; i++, current = ref Unsafe.Add(ref current, 1))
        {
            current.First++;
        }
        return span[0].First;
    } */

    /* [Benchmark]
    public int ChunkIteration()
    {
        var span = chunk.GetComponents<Component1>();
        for (int i = 0; i < span.Length; i++)
        {
            span[i].First++;
        }
        return span[0].First;
    } */

    /* [Benchmark]
    public int ArchetypeIteration()
    {
        for (int i = 0; i < archetype.Chunks.Length; i++)
        {
            var chunk = archetype.Chunks[i];
            var span = chunk.GetComponents<Component1>();
            for (int j = 0; j < span.Length; j++)
            {
                span[j].First++;
            }
        }

        return archetype.GetComponent<Component1>(new()
        {
            ChunkIndex = 0,
            RowIndex = 0
        }).First;
    } */
}
