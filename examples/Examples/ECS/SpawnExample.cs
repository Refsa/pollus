namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;

public class ECSSpawnExample : IExample
{
    public string Name => "ecs-spawn";

    struct Component1 : IComponent
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
            .AddSystem(CoreStage.First, FnSystem.Create(new("Spawn")
            {
                Locals = [Local.From(new Tracker(0.0f, true))],
            },
            static (Local<Tracker> doAction, Time time, Commands commands, Query<Component1> query) =>
            {
                if (doAction.Value.Timer <= 0f)
                {
                    if (doAction.Value.Spawn)
                    {
                        for (int i = 0; i < 100_000; i++)
                        {
                            commands.Spawn(Entity.With(new Component1 { Value = i }));
                        }
                    }
                    else
                    {
                        foreach (var entity in query)
                        {
                            commands.Despawn(entity.Entity);
                        }
                    }

                    doAction.Value.Timer = 0f;
                    doAction.Value.Spawn = !doAction.Value.Spawn;
                }
                else
                {
                    doAction.Value.Timer -= time.DeltaTimeF;
                }
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}

