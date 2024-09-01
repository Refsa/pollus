namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public class ShapesExample : IExample
{
    public string Name => "shapes";
    IApplication? application;

    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
    .AddPlugins([
        new AssetPlugin {RootPath = "assets"},
        new RenderingPlugin(),
        new ShapePlugin(),
    ])
    .AddSystem(CoreStage.PostInit, SystemBuilder.FnSystem("SetupShapes",
    static (Commands commands, AssetServer assetServer, Assets<Shape> shapes, Assets<ShapeMaterial> shapeMaterials) =>
    {
        commands.Spawn(Camera2D.Bundle);

        var shapeMaterial = shapeMaterials.Add(new ShapeMaterial
        {
            ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/shape.wgsl"),
        });

        commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2.Default with
            {
                Position = new Vec2f(128f, 128f),
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * 64f)),
                Color = Color.RED,
            }));

        commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2.Default with
            {
                Position = new Vec2f(256f, 128f),
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shapes.Add(Shape.Polygon(Vec2f.Zero, 64f)),
                Color = Color.GREEN,
            }));

        commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2.Default with
            {
                Position = new Vec2f(384f, 128f),
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shapes.Add(Shape.Arc(Vec2f.Zero, 64f, 135f)),
                Color = Color.BLUE,
            }));

        commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2.Default with
            {
                Position = new Vec2f(512f, 128f),
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shapes.Add(Shape.Circle(Vec2f.Zero, 64f)),
                Color = Color.ORANGE,
            }));

        commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2.Default with
            {
                Position = new Vec2f(640f, 128f),
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shapes.Add(Shape.Capsule(Vec2f.Down * 32f, Vec2f.Up * 32f, 32f)),
                Color = Color.ORANGE,
            }));
    })).Build()
    ).Run();
}