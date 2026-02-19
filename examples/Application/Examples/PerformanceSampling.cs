namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Mathematics;
using Pollus.Spatial;

public class PerformanceSampling : IExample
{
    public string Name => "performance-sampling";

    IApplication? app;

    public void Run()
    {
        ResourceFetch<SpatialLooseGrid<Entity>>.Register();

        app = Application.Builder
            .AddPlugins([
                new InputPlugin()
            ])
            .AddResource(new SpatialLooseGrid<Entity>(64, 64, 64))
            .AddSystems(CoreStage.PostInit, FnSystem.Create("Setup",
            static (Commands commands) =>
            {
                for (int x = 0; x < 32; x++)
                    for (int y = 0; y < 32; y++)
                        for (int z = 0; z < 512; z++)
                        {
                            commands.Spawn(Entity.With(Transform2D.Default with
                            {
                                Position = new Vec2f(x * 64, y * 64)
                            }));
                        }
            }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("Update")
            {
                Locals = [Local.From(256f)]
            },
            static (Local<float> size, ButtonInput<Key> keys, SpatialLooseGrid<Entity> spatialGrid, Query<Transform2D> qTransforms) =>
            {
                spatialGrid.Clear();
                qTransforms.ForEach(spatialGrid,
                static (in SpatialLooseGrid<Entity> spatialGrid, in Entity entity, ref Transform2D transform) =>
                {
                    spatialGrid.Insert(entity, transform.Position, 4f, 1u << 0);
                });

                if (keys.JustPressed(Key.ArrowDown)) size.Value += 4f;
                if (keys.JustPressed(Key.ArrowUp)) size.Value -= 4f;

                Span<Entity> result = stackalloc Entity[1024];
                var count = spatialGrid.Query(new Vec2f(0, 0), size.Value, 1u << 0, result);
                Log.Info($"Count: {count}, Size: {size.Value}");
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}