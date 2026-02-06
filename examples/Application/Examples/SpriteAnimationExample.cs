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

public class SpriteAnimationExample : IExample
{
    public string Name => "sprite-animation";

    IApplication? app;

    public void Run() => (app = Application.Builder
            .AddPlugins([
                SpritePlugin.Default,
                AssetPlugin.Default,
            ])
            .AddSystems(CoreStage.PostInit, FnSystem.Create("Init",
                static (Commands commands, AssetServer assetServer,
                    Assets<SpriteMaterial> materials,
                    Assets<SpriteAnimation> spriteAnimations,
                    Assets<TextureAtlas> atlases
                ) =>
                {
                    commands.Spawn(Camera2D.Bundle);

                    var sheet = assetServer.LoadAsync<Texture2D>("sprites/test_sheet.png");
                    var atlas = TextureAtlas.FromGrid("Atlas1", sheet, 5, 4, new Vec2<int>(16, 16));
                    var atlasHandle = atlases.Add(atlas);

                    var material = materials.Add(new SpriteMaterial()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                        Texture = sheet,
                        Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                    });

                    var anim1 = spriteAnimations.Add(SpriteAnimation.From(
                        "Anim1", atlasHandle, atlas, 2, [0, 1, 2, 3]
                    ));

                    var spriteEntity = commands.Spawn(Entity
                        .With(new Sprite()
                        {
                            Material = material,
                            Slice = atlas.GetRect("1"),
                            Color = Color.WHITE,
                        })
                        .With(Transform2D.Default with
                        {
                            Position = Vec2f.One * 256f,
                            Scale = Vec2f.One * 4f,
                        })
                        .With(new SpriteAnimator()
                        {
                            Animation = anim1,
                            CurrentFrame = 0,
                            Playing = true,
                            Timer = 0.25f,
                            Flags = SpriteAnimatorFlag.Loop,
                        }));
                }))
            .Build()
        ).Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}
