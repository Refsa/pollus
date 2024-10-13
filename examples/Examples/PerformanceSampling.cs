namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Transform;
using Pollus.Mathematics;
using Pollus.Spatial;

public class PerformanceSampling : IExample
{
    public string Name => "performance-sampling";

    IApplication? app;

    public void Run()
    {
        ResourceFetch<SpatialHashGrid<Entity>>.Register();

        app = Application.Builder
            .AddResource(new SpatialHashGrid<Entity>(64, 2048, 2048))
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
            static (Commands commands) =>
            {
                for (int x = 0; x < 32; x++)
                    for (int y = 0; y < 32; y++)
                        for (int z = 0; z < 256; z++)
                        {
                            commands.Spawn(Entity.With(Transform2D.Default with
                            {
                                Position = new Vec2f(x, y)
                            }));
                        }
            }))
            .AddSystem(CoreStage.Update, FnSystem.Create("Update",
            static (SpatialHashGrid<Entity> spatialGrid, Query<Transform2D> qTransforms) =>
            {
                spatialGrid.Clear();
                qTransforms.ForEach(spatialGrid,
                static (in SpatialHashGrid<Entity> spatialGrid, in Entity entity, ref Transform2D transform) =>
                {
                    spatialGrid.Insert(entity, transform.Position, 4, 1u << 0);
                });
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}