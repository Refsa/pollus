
namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.ECS.Core;
using Pollus.Engine;
using Pollus.Engine.Debug;
using Pollus.Graphics.Windowing;
using static Pollus.ECS.SystemBuilder;

public class ECSExample : IExample
{
    public string Name => "ecs";
    IApplication? application;

    struct TestEvent
    {
        public int Value;
    }

    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            // new PerformanceTrackerPlugin(),
        ])
        .InitEvent<TestEvent>()
        .AddSystem(CoreStage.PostInit, FnSystem("PrintSchedule", static (World world) => Log.Info(world.Schedule.ToString())))
        .AddSystem(CoreStage.PostInit, FnSystem("Setup",
        static (Commands commands) =>
        {
            for (int i = 0; i < 100_000; i++)
            {
                commands.Spawn(Entity.With(new Component1(), new Component2()));
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("Update",
        static (Query<Component1, Component2> query) =>
        {
            query.ForEach((ref Component1 c1, ref Component2 c2) =>
            {
                c1.Value += c2.Value;
            });
        }))
        .AddSystem(CoreStage.First, FnSystem("ReadEventFirst",
        static (EventReader<TestEvent> eTestEvent) =>
        {
            foreach (var e in eTestEvent.Read())
            {
                Log.Info($"Event: {e.Value}");
            }
        }))
        .AddSystem(CoreStage.Last, FnSystem("WriteEventLast",
        static (Local<int> counter, EventWriter<TestEvent> eTestEvent) =>
        {
            eTestEvent.Write(new TestEvent { Value = counter.Value++ });
        }).RunCriteria(new RunFixed(1f)))
        .Build())
        .Run();
}