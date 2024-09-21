namespace Pollus.Examples;

using System.Collections;
using Pollus.Coroutine;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;

public class CoroutineExample : IExample
{
    public string Name => "coroutine";

    IApplication? app;

    public void Run() => (app = Application.Builder
        .AddSystem(CoreStage.Update, Coroutine.Create(new("TestCoroutine")
        {
            Locals = [Local.From(1f)]
        },
        static (Param<Local<float>> param) =>
        {
            return Routine(param);
            static IEnumerator<Yield> Routine(Param<Local<float>> param)
            {
                yield return Yield.WaitForSeconds(param.Param0.Value);
                Log.Info("Coroutine Tick");
            }
        }))
        .Build())
        .Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}