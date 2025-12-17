namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;

public partial class QueryFilterExample : IExample
{
    public string Name => "query-filter";

    partial struct Component1 : IComponent
    {
        public int Value { get; set; }
    }

    partial struct Component2 : IComponent
    {
        public int Value { get; set; }
    }

    record struct Tracker(float Timer, bool Spawn);

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new TimePlugin(),
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
            static (Local<float> logCD, Time time, Query<Component1> query, Query<Component1>.Filter<None<Component2>> queryFiltered) =>
            {
                logCD.Value -= time.DeltaTimeF;
                if (logCD.Value > 0f) return;
                logCD.Value = 1f;
                Log.Info($"Total: {query.EntityCount()} | Filter: {queryFiltered.EntityCount()}");
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}

