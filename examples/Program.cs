using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Examples;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

// new DrawTriangle().Run();
// new ECSExample().Run();
// new InputExample().Run();
// new AudioExample().Run();
// new ImGuiExample().Run();
// new BreakoutGame().Run();

Application.Builder
    .AddPlugins([
        new AssetPlugin { RootPath = "assets" },
        new RenderingPlugin(),
    ])
    .AddSystem(CoreStage.PostInit, SystemBuilder.FnSystem("Spawn",
    static (Commands commands, AssetServer assetServer, Assets<SamplerAsset> samplers, Assets<SpriteMaterial> materials) =>
    {
        var material = materials.Add(new SpriteMaterial
        {
            ShaderSource = assetServer.Load<ShaderAsset>("shaders/sprite.wgsl"),
            Texture = assetServer.Load<ImageAsset>("breakout/breakout_sheet.png"),
            Sampler = samplers.Add(SamplerDescriptor.Nearest),
        });

        commands.Spawn(Camera2D.Bundle);

        for (int i = 0; i < 100; i++)
        {
            commands.Spawn(Entity.With(
                new Counter1(),
                Transform2.Default with
                {
                    Position = new Vec2f(i / 10, i % 10) * 36f + new Vec2f(16f, 16f),
                    Scale = (32, 32)
                },
                new Sprite
                {
                    Material = material,
                    Slice = new(0, 0, 16, 16),
                    Color = Color.WHITE,
                }
            ));
        }
    }))
    .AddSystem(CoreStage.Update, SystemBuilder.FnSystem("Update",
    static (Query<Counter1> qCounter) =>
    {
        qCounter.ForEach((ref Counter1 counter) =>
        {
            counter.Value++;
        });
    }))
    .Run();


struct Counter1 : IComponent
{
    public int Value;
}