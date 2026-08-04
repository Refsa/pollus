namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Engine.Camera;
using Pollus.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;

public class SpriteBenchmark : IExample
{
    public string Name => "sprite-benchmark";
    IApplication? app;

    struct FPS
    {
        public int Frames;
        public float Time;
    }

    class SharedAssets
    {
        public Handle<SpriteMaterial> SpriteMaterial;
    }

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new AssetPlugin { RootPath = "assets" },
                new RenderingPlugin(),
                new InputPlugin(),
                new RandomPlugin(),
            ])
            .AddResource(new SharedAssets())
            .AddSystems(CoreStage.PostInit, FnSystem.Create("SpriteBenchmark::Setup",
                static (Commands commands, AssetServer assetServer, SharedAssets sharedAssets, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
                {
                    commands.Spawn(Camera2D.Bundle);

                    sharedAssets.SpriteMaterial = materials.Add(new SpriteMaterial
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    });
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create("SpriteBenchmark::Update",
                static (Commands commands, SharedAssets sharedAssets, Time time, IWindow window, Random random, Local<FPS> fps, Query.Filter<All<Sprite>> qSprites) =>
                {
                    fps.Value.Frames++;
                    fps.Value.Time += time.DeltaTimeF;
                    if (fps.Value.Time < 2.0f) return;

                    var measuredFps = fps.Value.Frames / fps.Value.Time;
                    Log.Info($"FPS: {measuredFps:F1} | Sprites: {qSprites.EntityCount()}");

                    if (measuredFps > 60)
                    {
                        var targetDist = measuredFps - 60;
                        var toSpawn = int.Min((int)(float.Abs(targetDist) * 1_000f), 50_000);
                        for (int i = 0; i < toSpawn; i++)
                        {
                            commands.Spawn(Entity.With(
                                Transform2D.Default with
                                {
                                    Position = new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                                },
                                new Sprite
                                {
                                    Material = sharedAssets.SpriteMaterial,
                                    Slice = new Rect(0.0f, 0.0f, 16.0f, 16.0f),
                                    Color = Color.WHITE,
                                }
                            ));
                        }
                    }
                    else if (measuredFps < 60)
                    {
                        var targetDist = 60 - measuredFps;
                        var toDespawn = int.Min((int)(float.Abs(targetDist) * 1_000f), 50_000);
                        foreach (var entity in qSprites)
                        {
                            if (toDespawn-- <= 0) break;
                            commands.Despawn(entity);
                        }
                    }

                    fps.Value.Frames = 0;
                    fps.Value.Time = 0.0f;
                }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}