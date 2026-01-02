namespace Pollus.Examples;

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
        public required int MinValue;
        public required int MaxValue;
    }

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new RenderingPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("SetupShapes",
            static (Commands commands, AssetServer assetServer, Assets<SpriteMaterial> spriteMaterials) =>
            {
                commands.Spawn(Camera2D.Bundle);

                var spriteMaterial = spriteMaterials.Add(new SpriteMaterial
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
                            Material = spriteMaterial,
                            Slice = Rect.FromOriginSize(new Vec2f((i % 4) * 16, (i / 4) * 16), new Vec2f(16, 16)),
                            Color = Color.WHITE,
                        },
                        Transform2D.Default with
                        {
                            Position = new Vec2f(100 + i * 32, 256),
                            Scale = Vec2f.One * 64f,
                            ZIndex = i,
                        },
                        GlobalTransform.Default)
                    );

                    if (i == 0)
                    {
                        entity.AddComponent(new ZIndexSweep
                        {
                            MinValue = 0,
                            MaxValue = count,
                        });
                    }
                }
            }))
        .AddSystem(CoreStage.Update, FnSystem.Create("UpdateZIndex", static (Time time, Query<Transform2D, ZIndexSweep> query) =>
        {
            query.ForEach(time.SecondsSinceStartup, static (in secondsSinceStartup, ref transform, ref zIndexSweep) =>
            {
                var t = (float)secondsSinceStartup.Sin().Remap(-1, 1, 0, 1);
                var x = Math.Lerp(100, 100 + 15 * 32, t);
                transform.Position.X = x;

                var index = (int)((x - 100) / 32).Round();
                transform.ZIndex = index + 1;
            });
        }))
        .Build()).Run();

    public void Stop()
    {
        application?.Shutdown();
    }
}