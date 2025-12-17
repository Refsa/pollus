
namespace Pollus.Examples;

using System.Runtime.CompilerServices;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;

public partial class ChangeTrackingExample : IExample
{
    public string Name => "change-tracking";
    IApplication? application;

    public partial struct Component1 : IComponent
    {
        public int Value;
    }

    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("PrintSchedule", static (World world) => Log.Info(world.Schedule.ToString())))
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
        static (Commands commands) =>
        {
            for (int i = 0; i < 100_000; i++)
            {
                commands.Spawn(Entity.With(new Component1()));
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("ChangeTracking",
        static (Query<Component1> qComponents, Query query) =>
        {
            foreach (var row in qComponents)
            {
                row.Component0.Value++;
                query.SetChanged<Component1>(row.Entity);
            }
        }))
        .Build())
        .Run();
}