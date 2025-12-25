namespace Pollus.Examples;

using System.Collections;
using Pollus.Coroutine;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;

public partial class CoroutineExample : IExample
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
            new StatePlugin<TestState>(TestState.Second),
        ])
        .AddSystemSet<CoroutineSystemSet>()
        .AddSystem(CoreStage.Update, Coroutine.Create(new("TestCoroutine")
        {
            Locals = [Local.From(1f)],
        },
        static (param) =>
        {
            return Routine();
            static IEnumerable<Yield> Routine()
            {
                yield return Yield.WaitForSeconds(1f);
                Log.Info("Coroutine Tick, Press Space to enter First State");
                yield return Coroutine.WaitForEnterState(TestState.First);
                Log.Info("Entered First State, Press Space to exit First State");
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

    [SystemSet]
    public partial class CoroutineSystemSet
    {
        [Coroutine(nameof(Routine))]
        static readonly SystemBuilderDescriptor RoutineDescriptor = new()
        {
            Stage = CoreStage.Update,
        };

        static IEnumerable<Yield> Routine(Time time)
        {
            yield return Yield.WaitForSeconds(1f);
            Log.Info($"Coroutine from SystemSet Tick, time: {time.SecondsSinceStartup}");
        }
    }
}