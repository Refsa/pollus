namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;

public class QueryFilterExample : IExample
{
    public string Name => "query-filter";

    struct Component1 : IComponent
    {
        public int Value { get; set; }
    }

    struct Component2 : IComponent
    {
        public int Value { get; set; }
    }

    record struct Tracker(float Timer, bool Spawn);

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new PerformanceTrackerPlugin(),
            ])
            .AddSystem(CoreStage.PostInit, FnSystem.Create(new("Spawn"),
            static (World world, Commands commands) =>
            {   
                Log.Info(world.Schedule.ToString());

                commands.Spawn(Entity.With(new Component1 { Value = 0 }));
                commands.Spawn(Entity.With(new Component1 { Value = 0 }, new Component2 { Value = 0 }));
            }))
            .AddSystem(CoreStage.Update, FnSystem.Create(new("Filter"),
            static (Query<Component1>.Filter<None<Component2>> query) =>
            {
                Log.Info($"Filter: {query.EntityCount()}");
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}

