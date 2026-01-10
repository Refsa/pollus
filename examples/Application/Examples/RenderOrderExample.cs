namespace Pollus.Examples;

using Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public partial class RenderOrderExample : IExample
{
    public string Name => "render-order";

    IApplication? application;

    partial struct ZIndexSweep : IComponent
    {
        public float StartX;
        public required int MinValue;
        public required int MaxValue;
    }

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new RenderingPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("SetupShapes",
            static (World world, Commands commands, AssetServer assetServer, Assets<SpriteMaterial> spriteMaterials) =>
            {
                Log.Info(world.Schedule.ToString());

                commands.Spawn(Camera2D.Bundle);

                var spriteMaterial1 = spriteMaterials.Add(new SpriteMaterial
                {
                    ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                    Texture = assetServer.LoadAsync<Texture2D>("sprites/test_sheet.png"),
                    Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                });

                var spriteMaterial2 = spriteMaterials.Add(new SpriteMaterial
                {
                    ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                    Texture = assetServer.LoadAsync<Texture2D>("sprites/test_sheet.png"),
                    Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                });

                int count = 16;
                for (int i = 0; i < count; i++)
                {
                    var entity = commands.Spawn(Entity.With(
                        new Sprite()
                        {
                            Material = i % 2 == 0 ? spriteMaterial1 : spriteMaterial2,
                            Slice = Rect.FromOriginSize(new Vec2f((i % 4) * 16, (i / 4) * 16), new Vec2f(16, 16)),
                            Color = i == 0 ? Color.RED : Color.WHITE,
                        },
                        Transform2D.Default with
                        {
                            Position = new Vec2f(100 + i * 32, 256),
                            Scale = Vec2f.One * 4f,
                            ZIndex = i,
                        }
                    ));

                    if (i == 0)
                    {
                        entity.AddComponent(new ZIndexSweep
                        {
                            StartX = 100,
                            MinValue = 0,
                            MaxValue = count
                        });
                    }
                }
            }))
        .AddSystem(CoreStage.Update, FnSystem.Create("UpdateZIndex", static (Time time, Query<Transform2D, ZIndexSweep> query) =>
        {
            query.ForEach(time.SecondsSinceStartup, static (in secondsSinceStartup, ref transform, ref zIndexSweep) =>
            {
                var t = (float)(secondsSinceStartup * 0.4f).Sin().Remap(-1, 1, 0, 1);
                var x = Math.Lerp(zIndexSweep.StartX - 16, zIndexSweep.StartX + 15 * 32 + 16, t);
                transform.Position.X = x;

                var index = (int)((x - 100) / 32).Round();
                transform.ZIndex = index + 0.01f;
            });
        }))
        .Build()).Run();

    public void Stop()
    {
        application?.Shutdown();
    }
}