namespace Pollus.Benchmark;

using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Pollus.ECS;
using Pollus.Engine.Mathematics;

struct Component1 : IComponent
{
    public int First;
}

[MemoryDiagnoser]
[ReturnValueValidator(failOnError: true)]
public class TestBenchmarks
{
    const int SIZE = 100_000;
    Component1[] array;
    NativeArray<Component1> nativeArray;
    ArchetypeChunk chunk;
    Archetype archetype;

    public TestBenchmarks()
    {
        array = new Component1[SIZE];
        nativeArray = new(SIZE);
        chunk = new([Component.GetInfo<Component1>().ID], SIZE);
        chunk.SetCount(SIZE);
        archetype = new([Component.GetInfo<Component1>().ID]);

        for (int i = 0; i < SIZE; i++)
        {
            var comp = new Component1 { First = i };

            array[i] = comp;
            nativeArray[i] = comp;
            chunk.SetComponent(i, comp);

            var entity = archetype.AddEntity();
            archetype.SetComponent(entity, comp);
        }
    }

    ~TestBenchmarks()
    {
        nativeArray.Dispose();
        chunk.Dispose();
        archetype.Dispose();
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

    [Benchmark]
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
    }

    /* [Benchmark]
    public int Float3_Add()
    {
        Float3 sum = new(0, 0, 0);
        for (int i = 0; i < SIZE; i++)
        {
            sum += new Float3(i, i, i);
        }
        return (int)(sum.X + sum.Y + sum.Z);
    }

    [Benchmark]
    public int Float3_SIMD_Add()
    {
        Float3_SIMD sum = new(0, 0, 0);
        for (int i = 0; i < SIZE; i++)
        {
            sum += new Float3_SIMD(i, i, i);
        }
        return (int)(sum.X + sum.Y + sum.Z);
    } */
}
