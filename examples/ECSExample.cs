
namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.ECS.Core;
using Pollus.Engine;
using Pollus.Engine.Debug;
using static Pollus.ECS.SystemBuilder;

public class ECSExample
{
    public void Run() => Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new PerformanceTrackerPlugin(),
        ])
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
        .Run();
}