
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

    struct TestEvent
    {
        public int Value;
    }

    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new TimePlugin(),
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
        .Build())
        .Run();
}