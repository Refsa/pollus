
namespace Pollus.Examples;

using System.Data;
using System.Runtime.CompilerServices;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;

public partial class ECSExample : IExample
{
    public string Name => "ecs";
    IApplication? application;

    public partial struct Component1 : IComponent
    {
        public int Value;
    }

    public partial struct Component2 : IComponent
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
            .AddSystems(CoreStage.PostInit, FnSystem.Create("PrintSchedule", static (World world) => Log.Info(world.Schedule.ToString())))
            .AddSystemSet<ComponentSystemSet>()
            .AddSystemSet<EventSystemSet>()
            .AddSystemSet<StateSystemSet>()
            .Build())
        .Run();

    [SystemSet]
    partial class ComponentSystemSet
    {
        [System(nameof(CoreStage.PostInit))]
        static void Spawn(Commands commands)
        {
            for (int i = 0; i < 100_000; i++)
            {
                commands.Spawn(Entity.With(new Component1 { Value = i }, new Component2 { Value = 1 }));
            }
        }

        [System(nameof(CoreStage.Update))]
        static void Update(Query<Component1, Read<Component2>> query)
        {
            query.ForEach(static (ref Component1 c1, ref Read<Component2> c2) =>
            {
                c1.Value += c2.Component.Value;
            });
        }
    }

    [SystemSet]
    partial class EventSystemSet
    {
        [System(nameof(CoreStage.First))]
        static void ReadEventFirst(EventReader<TestEvent> eTestEvent)
        {
            foreach (var e in eTestEvent.Read())
            {
                Log.Info($"Event: {e.Value}");
            }
        }

        [System(nameof(CoreStage.Last))]
        static void WriteEventLast(Local<int> counter, EventWriter<TestEvent> eTestEvent)
        {
            eTestEvent.Write(new TestEvent { Value = counter.Value++ });
        }
    }

    [SystemSet]
    partial class StateSystemSet
    {
        [System(nameof(CoreStage.PostUpdate)), RunCriteria<RunFixed>(1f)]
        static void StateTransition(State<TestState> state, Local<float> timer, Time time)
        {
            timer.Value -= 1f;
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
        }

        [System, OnStateEnter(TestState.State1)]
        static void State1Enter(World world) => Log.Info("Enter State1");

        [System, OnStateExit(TestState.State1)]
        static void State1Exit(World world) => Log.Info("Exit State1");

        [System, InState(TestState.State1)]
        static void State1Update(Local<float> timer, Time time)
        {
            timer.Value -= time.DeltaTimeF;
            if (timer.Value <= 0f)
            {
                timer.Value = 1f;
                Log.Info("State1Update");
            }
        }
    }
}
