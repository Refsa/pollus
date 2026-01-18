namespace Pollus.Examples;

using ECS;
using Engine;
using Engine.Assets;
using Engine.Camera;
using Engine.Rendering;
using Engine.Transform;
using Graphics.Rendering;
using Mathematics;
using Utils;

public class SpriteMaterialExample : IExample
{
    public string Name => "sprite-material";

    IApplication? app;

    public void Run() => (app = Application.Builder
            .AddPlugins([
                SpritePlugin.Default,
                AssetPlugin.Default,
            ])
            .AddSystems(CoreStage.PostInit, FnSystem.Create("Init",
                static (Commands commands, AssetServer assetServer, Assets<SpriteMaterial> materials) =>
                {
                    commands.Spawn(Camera2D.Bundle);

                    var material = materials.Add(new SpriteMaterial()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("sprites/test_sheet.png"),
                        Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                    });

                    commands.Spawn(Entity.With(
                        new Sprite()
                        {
                            Material = material,
                            Slice = Rect.FromOriginSize(Vec2f.Zero, Vec2f.One * 16),
                            Color = Color.WHITE,
                        },
                        Transform2D.Default with
                        {
                            Position = new Vec2f(256, 256),
                            Scale = Vec2f.One * 8f,
                        }
                    ));

                    var customShader = materials.Add(new SpriteMaterial()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/custom_sprite.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("sprites/test_sheet.png"),
                        Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                    });

                    commands.Spawn(Entity.With(
                        new Sprite()
                        {
                            Material = customShader,
                            Slice = Rect.FromOriginSize(Vec2f.Zero, Vec2f.One * 16),
                            Color = Color.WHITE,
                        },
                        Transform2D.Default with
                        {
                            Position = new Vec2f(512, 256),
                            Scale = Vec2f.One * 8f,
                        }
                    ));
                }))
            .Build()
        ).Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}
