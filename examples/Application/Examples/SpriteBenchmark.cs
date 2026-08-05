namespace Pollus.Examples;

using System.Diagnostics;
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
    const float TargetFps = 60.0f;
    const float TargetTolerance = 0.5f;
    const int InitialAdjustmentStep = 100_000;
    const int MinimumAdjustmentStep = 100;

    const bool UseMultipleMaterials = false;
    const bool SetZIndex = true;

    public string Name => "sprite-benchmark";
    IApplication? app;

    struct FPS
    {
        public Stopwatch? Stopwatch;
        public int Frames;
        public int AdjustmentStep;
        public int LastDirection;
    }

    readonly struct RestartStopwatchCommand(Stopwatch stopwatch) : ICommand
    {
        public static int Priority => int.MinValue;

        public void Execute(World world)
        {
            stopwatch.Restart();
        }
    }

    class SharedAssets
    {
        public Handle<SpriteMaterial>[] SpriteMaterials = Array.Empty<Handle<SpriteMaterial>>();
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
                    sharedAssets.SpriteMaterials = [
                        materials.Add(new SpriteMaterial
                        {
                            ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                            Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                            Sampler = samplers.Add(SamplerDescriptor.Nearest),
                        }),
                        materials.Add(new SpriteMaterial
                        {
                            ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                            Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                            Sampler = samplers.Add(SamplerDescriptor.Nearest),
                        }),
                        materials.Add(new SpriteMaterial
                        {
                            ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                            Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                            Sampler = samplers.Add(SamplerDescriptor.Nearest),
                        }),
                        materials.Add(new SpriteMaterial
                        {
                            ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                            Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                            Sampler = samplers.Add(SamplerDescriptor.Nearest),
                        }),
                    ];
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create("SpriteBenchmark::Update",
                static (Commands commands, SharedAssets sharedAssets, IWindow window, Random random, Local<FPS> fps, Query.Filter<All<Sprite>> qSprites) =>
                {
                    if (fps.Value.Stopwatch is null)
                    {
                        fps.Value.Stopwatch = Stopwatch.StartNew();
                        return;
                    }

                    fps.Value.Frames++;
                    var elapsed = fps.Value.Stopwatch.Elapsed.TotalSeconds;
                    if (elapsed < 2.0) return;

                    var measuredFps = fps.Value.Frames / elapsed;
                    if (fps.Value.AdjustmentStep == 0)
                    {
                        fps.Value.AdjustmentStep = InitialAdjustmentStep;
                    }

                    var direction = measuredFps switch
                    {
                        > TargetFps + TargetTolerance => 1,
                        < TargetFps - TargetTolerance => -1,
                        _ => 0,
                    };

                    if (direction != 0)
                    {
                        if (fps.Value.LastDirection != 0 && direction != fps.Value.LastDirection)
                        {
                            fps.Value.AdjustmentStep = int.Max(
                                fps.Value.AdjustmentStep / 2,
                                MinimumAdjustmentStep
                            );
                        }

                        if (direction > 0)
                        {
                            for (int i = 0; i < fps.Value.AdjustmentStep; i++)
                            {
                                commands.Spawn(Entity.With(
                                    Transform2D.Default with
                                    {
                                        Position = new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                                        ZIndex = SetZIndex ? random.NextFloat(0, 1024) : 0f,
                                    },
                                    new Sprite
                                    {
                                        Material = UseMultipleMaterials ? sharedAssets.SpriteMaterials[i % sharedAssets.SpriteMaterials.Length] : sharedAssets.SpriteMaterials[0],
                                        Slice = new Rect<int>(0, 0, 16, 16),
                                        Color = Color.From(Color.Hex.From(random.NextUInt()), 1f),
                                    }
                                ));
                            }
                        }
                        else
                        {
                            var toDespawn = fps.Value.AdjustmentStep;
                            foreach (var entity in qSprites)
                            {
                                if (toDespawn-- <= 0) break;
                                commands.Despawn(entity);
                            }
                        }

                        fps.Value.LastDirection = direction;
                    }

                    var msg = $"FPS: {measuredFps:F1} | Sprites: {qSprites.EntityCount()} | Adjustment: {fps.Value.AdjustmentStep}";
                    Log.Info(msg);
                    window.SetTitle(msg);

                    fps.Value.Frames = 0;
                    commands.AddCommand(new RestartStopwatchCommand(fps.Value.Stopwatch));
                }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}
