namespace Pollus.Benchmark;

using BenchmarkDotNet.Attributes;
using Pollus.ECS;

[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
public class NativeMapBenchmarks
{
    const int SIZE = 100_000;

    Dictionary<int, int> dictionary;
    NativeMap<int, int> nativeMap;

    public NativeMapBenchmarks()
    {
        dictionary = new Dictionary<int, int>();
        nativeMap = new NativeMap<int, int>(SIZE);
        for (int i = 0; i < SIZE; i++)
        {
            dictionary.Add(i, i);
            nativeMap.Add(i, i);
        }
    }

    ~NativeMapBenchmarks()
    {
        nativeMap.Dispose();
    }	

    [Benchmark]
    [BenchmarkCategory("Add")]
    public void DictionaryAdd()
    {
        var dict = new Dictionary<int, int>();

        for (int i = 0; i < SIZE; i++)
        {
            dict.Add(i, i);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Add")]
    public void NativeMapAdd()
    {
        using var map = new NativeMap<int, int>(0);

        for (int i = 0; i < SIZE; i++)
        {
            map.Add(i, i);
        }
    }

    [Benchmark]
    [BenchmarkCategory("Get")]
    public int DictionaryGet()
    {
        var sum = 0;
        for (int i = 0; i < SIZE; i++)
        {
            sum += dictionary[i];
        }
        return sum;
    }

    [Benchmark]
    [BenchmarkCategory("Get")]
    public int NativeMapGet()
    {
        var sum = 0;
        for (int i = 0; i < SIZE; i++)
        {
            sum += nativeMap.Get(i);
        }
        return sum;
    }

    [Benchmark]
    [BenchmarkCategory("Has")]
    public int DictionaryHas()
    {
        var sum = 0;
        for (int i = 0; i < SIZE; i++)
        {
            sum += dictionary.ContainsKey(i) ? 1 : 0;
        }
        return sum;
    }

    [Benchmark]
    [BenchmarkCategory("Has")]
    public int NativeMapHas()
    {
        var sum = 0;
        for (int i = 0; i < SIZE; i++)
        {
            sum += nativeMap.Has(i) ? 1 : 0;
        }
        return sum;
    }
}