namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Input;
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
                new AssetPlugin{ RootPath = "assets"},
                new RenderingPlugin(),
                new InputPlugin(),
            ])
            .AddResource(new SharedAssets())
            .AddSystem(CoreStage.PostInit, SystemBuilder.FnSystem("SpriteBenchmark::Setup",
            static (Commands commands, AssetServer assetServer, SharedAssets sharedAssets, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
            {
                commands.Spawn(Camera2D.Bundle);

                sharedAssets.SpriteMaterial = materials.Add(new SpriteMaterial
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                    Texture = assetServer.Load<ImageAsset>("breakout/breakout_sheet.png"),
                    Sampler = samplers.Add(SamplerDescriptor.Nearest),
                });
            }))
            .AddSystem(CoreStage.Update, SystemBuilder.FnSystem("SpriteBenchmark::Update",
            static (Commands commands, SharedAssets sharedAssets, Time time, IWindow window, Random random, Local<FPS> fps, Query<Sprite> qSprites) =>
            {
                fps.Value.Frames++;
                fps.Value.Time += time.DeltaTimeF;
                if (fps.Value.Time < 5.0f) return;

                Log.Info($"FPS: {fps.Value.Frames} | Sprites: {qSprites.EntityCount()}");

                if (fps.Value.Frames > 60)
                {
                    var targetDist = fps.Value.Frames - 60;
                    for (int i = 0; i < int.Min(targetDist * 10000, 100_000); i++)
                    {
                        commands.Spawn(Entity.With(
                            Transform2.Default with
                            {
                                Position = new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                                Scale = Vec2f.One * 16f,
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
                else if (fps.Value.Frames < 60)
                {
                    var targetDist = 60 - fps.Value.Frames;
                    for (int i = 0; i < targetDist * 100; i++)
                    {
                        qSprites.ForEach((in Entity entity, ref Sprite sprite) =>
                        {
                            commands.Despawn(entity);
                        });
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