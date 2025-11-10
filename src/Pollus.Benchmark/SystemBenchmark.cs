namespace Pollus.Benchmark;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Pollus.Coroutine;
using Pollus.ECS;

[MemoryDiagnoser]
// [ReturnValueValidator(failOnError: true)]
// [SimpleJob(RuntimeMoniker.Net90)]
// [SimpleJob(RuntimeMoniker.Net80)]
// [HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class SystemBenchmark
{
    World coroutineWorld;

    public SystemBenchmark()
    {
        coroutineWorld = new World()
            .AddPlugin<TimePlugin>();
        coroutineWorld.Schedule.AddSystems(CoreStage.Update, Coroutine.Create(new("TestCoroutine")
        {
            Locals = [Local.From(0.001f)]
        },
        static (Param<Local<float>, Time> param) =>
        {
            return Routine(param);
            static IEnumerable<Yield> Routine(Param<Local<float>, Time> param)
            {
                (var timer, var time) = param;
                if (timer.Value > 0)
                {
                    timer.Value -= time.DeltaTimeF;
                    yield return Yield.Return;
                }

                timer.Value = 0.001f;
            }
        }));
    }

    ~SystemBenchmark()
    {
        coroutineWorld.Dispose();
    }

    [Benchmark]
    public void CourotineSystem()
    {
        coroutineWorld.Update();
    }
}