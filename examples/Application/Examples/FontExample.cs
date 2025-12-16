using Pollus.Debugging;

namespace Pollus.Examples;

using Pollus.Engine.Transform;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Mathematics;
using Pollus.Utils;
using Utf8StringInterpolation;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Rendering;

public class FontExample : IExample
{
    struct Counter : IComponent
    {
    }

    public string Name => "font";
    IApplication? application;
    public void Stop() => application?.Shutdown();

    public void Run()
    {
        application = Application.Builder
            .AddPlugin<FontPlugin>()
            .AddPlugin<PerformanceTrackerPlugin>()
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
                static (World world, AssetServer assetServer) =>
                {
                    world.Spawn(Camera2D.Bundle);

                    var spaceMono = assetServer.Load<FontAsset>("fonts/SpaceMono-Regular.ttf");

                    for (int i = 8; i <= 64; i += 4)
                    {
                        world.Spawn(TextDraw.Bundle
                            .Set(TextDraw.Default with
                            {
                                Font = spaceMono,
                                Size = i,
                                Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz\nThe Quick Brown Fox Jumps Over The Lazy Dog",
                                Color = Color.WHITE,
                            })
                            .Set(Transform2D.Default with
                            {
                                Position = Vec2f.Right * 32f + Vec2f.Up * (Math.Pow(i, 1.7f) + 16f),
                            }));
                    }

                    world.Spawn(Entity
                        .With(TextDraw.Default with
                        {
                            Font = spaceMono,
                            Size = 16f,
                            Text = "Seconds",
                            Color = Color.BLACK,
                        })
                        .With(TextMesh.Default)
                        .With(Transform2D.Default with
                        {
                            Position = Vec2f.One * 16f,
                        })
                        .With(new Counter())
                    );
                }))
            .AddSystem(CoreStage.Update, FnSystem.Create("Rotate",
                static (Time time, PerformanceTrackerPlugin.PerformanceMetrics perf, Query<TextDraw>.Filter<All<Counter>> query) =>
                {
                    foreach (var entity in query)
                    {
                        entity.Component0.Text = Utf8String.Format($"Seconds since startup: {time.SecondsSinceStartup:F2}s");
                    }
                }))
            .Build();
        application.Run();
    }
}