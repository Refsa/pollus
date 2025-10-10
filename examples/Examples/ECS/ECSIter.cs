namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;

public partial class ECSIter : IExample
{
    public string Name => "ecs-iter";
    IApplication? application;

    public struct Component1 : IComponent
    {
        public int Value;
    }

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn", static (Commands commands) =>
        {
            for (int i = 0; i < 1_000_000; i++)
            {
                commands.Spawn(Entity.With(new Component1()));
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Iter", static (Query<Component1> query) =>
        {
            query.ForEach(static (ref Component1 c) =>
            {
                c.Value++;
            });
        }))
        .Build())
        .Run();

    public void Stop() => application?.Shutdown();
}

