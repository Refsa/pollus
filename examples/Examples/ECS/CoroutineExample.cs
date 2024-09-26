namespace Pollus.Examples;

using System.Collections;
using Pollus.Coroutine;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;

public class CoroutineExample : IExample
{
    public string Name => "coroutine";

    IApplication? app;

    enum TestState
    {
        First,
        Second,
    }

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new InputPlugin(),
            new StatePlugin<TestState>(),
        ])
        .AddSystem(CoreStage.Update, Coroutine.Create(new("TestCoroutine")
        {
            Locals = [Local.From(1f)],
        },
        static (Param<Local<float>, Time> param) =>
        {
            return Routine(param);
            static IEnumerator<Yield> Routine(Param<Local<float>, Time> param)
            {
                (var timer, var time) = param;
                while (timer.Value > 0f)
                {
                    timer.Value -= time.DeltaTimeF;
                    yield return Yield.Return();
                }

                timer.Value = 1f;
                Log.Info("Coroutine Tick");
                yield return Coroutine.WaitForEnterState(TestState.First);
                Log.Info("Entered First State");
                yield return Coroutine.WaitForExitState(TestState.First);
                Log.Info("Exited First State");
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Input", static (ButtonInput<Key> keyboard, State<TestState> state) =>
        {
            if (keyboard.JustPressed(Key.Space))
            {
                state.Set(state.Current switch
                {
                    TestState.First => TestState.Second,
                    TestState.Second => TestState.First,
                    _ => throw new NotImplementedException(),
                });
            }
        }))
        .Build())
        .Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}