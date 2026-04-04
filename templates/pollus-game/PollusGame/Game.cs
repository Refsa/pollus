namespace PollusGame;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public static class Game
{
    public static void Run(string[] args)
    {
        Application.Builder
            .AddPlugins([
                new AssetPlugin { RootPath = "assets" },
                new RenderingPlugin(),
                new TransformPlugin<Transform2D>(),
                new ShapePlugin(),
            ])
            .AddSystems(CoreStage.PostInit, FnSystem.Create("Setup",
            static (Commands commands, AssetServer assetServer, Assets<Shape> shapes, Assets<ShapeMaterial> shapeMaterials) =>
            {
                commands.Spawn(Camera2D.Bundle);

                var material = shapeMaterials.Add(new ShapeMaterial
                {
                    ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/shape.wgsl"),
                });

                commands.Spawn(ShapeDraw.Bundle
                    .Set(Transform2D.Default with
                    {
                        Position = new Vec2f(400f, 300f),
                    })
                    .Set(new ShapeDraw
                    {
                        MaterialHandle = material,
                        ShapeHandle = shapes.Add(Shape.Circle(Vec2f.Zero, 64f)),
                        Color = Color.ORANGE,
                    }));
            }))
            .Build()
            .Run();
    }
}
