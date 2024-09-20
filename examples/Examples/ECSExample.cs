
namespace Pollus.Examples;

using System.Runtime.CompilerServices;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;

public class ECSExample : IExample
{
    public string Name => "ecs";
    IApplication? application;

    public struct Component1 : IComponent
    {
        public int Value;
    }

    public struct Component2 : IComponent
    {
        public int Value;
    }

    enum TestState : int
    {
        State1 = 0,
        State2,
        State3,
    }

    struct TestEvent
    {
        public int Value;
    }

    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new StatePlugin<TestState>(TestState.State3),
        ])
        .InitEvent<TestEvent>()
        .AddSystem(CoreStage.PostInit, FnSystem.Create("PrintSchedule", static (World world) => Log.Info(world.Schedule.ToString())))
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
        static (Commands commands) =>
        {
            for (int i = 0; i < 100_000; i++)
            {
                commands.Spawn(Entity.With(new Component1(), new Component2()));
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Update",
        static (Query<Component1, Component2> query) =>
        {
            query.ForEach(static (ref Component1 c1, ref Component2 c2) =>
            {
                c1.Value += c2.Value;
            });
        }))
        .AddSystem(CoreStage.First, FnSystem.Create("ReadEventFirst",
        static (EventReader<TestEvent> eTestEvent) =>
        {
            foreach (var e in eTestEvent.Read())
            {
                Log.Info($"Event: {e.Value}");
            }
        }))
        .AddSystem(CoreStage.Last, FnSystem.Create(new("WriteEventLast")
        {
            RunCriteria = new RunFixed(1f)
        },
        static (Local<int> counter, EventWriter<TestEvent> eTestEvent) =>
        {
            eTestEvent.Write(new TestEvent { Value = counter.Value++ });
        }))
        .AddSystem(CoreStage.PostUpdate, FnSystem.Create("StateTransition", 
        static (State<TestState> state, Local<float> timer, Time time) =>
        {
            timer.Value -= time.DeltaTimeF;
            if (timer.Value <= 0f)
            {
                timer.Value = 5f;
                state.Set(state.Current switch 
                {
                    TestState.State1 => TestState.State2,
                    TestState.State2 => TestState.State3,
                    TestState.State3 => TestState.State1,
                    _ => throw new NotImplementedException(),
                });
                Log.Info($"State: {state.Current} -> {state.Next}");
            }
        }))
        .AddSystem(StateEnter.On(TestState.State1), FnSystem.Create("State1Enter", static (World world) => Log.Info("Enter State1")))
        .AddSystem(StateExit.On(TestState.State1), FnSystem.Create("State1Exit", static (World world) => Log.Info("Exit State1")))
        .AddSystem(CoreStage.Update, FnSystem.Create(new("State1Update")
        {
            RunCriteria = StateRunCriteria<TestState>.OnCurrent(TestState.State1)
        }, 
        static (Local<float> timer, Time time) =>
        {
            timer.Value -= time.DeltaTimeF;
            if (timer.Value <= 0f)
            {
                timer.Value = 1f;
                Log.Info("State1Update");
            }
        }))
        .Build())
        .Run();
}