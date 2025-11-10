namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Engine.Tween;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public partial class TweenExample : IExample
{
    public string Name => "tween";

    IApplication? app;


    [Tweenable]
    partial struct Test : IComponent
    {
        public float Float;
    }

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new AssetPlugin {RootPath = "assets"},
            new RenderingPlugin(),
            new TransformPlugin<Transform2D>(),
            new ShapePlugin(),
            new InputPlugin(),
            new TweenPlugin(),
            new TweenablePlugin<Test>(),
            new TweenablePlugin<Transform2D>(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
        static (Commands commands, AssetServer assetServer, Assets<Shape> shapes, Assets<ShapeMaterial> shapeMaterials) =>
        {
            commands.Spawn(Camera2D.Bundle);

            var shapeMaterial = shapeMaterials.Add(new ShapeMaterial
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/shape.wgsl"),
            });

            var scale = 4f;
            var shape = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * scale));

            for (int x = 0; x < 100; x++)
                for (int y = 0; y < 10; y++)
                {
                    SpawnAndTween(commands, Vec2f.One * 50f + new Vec2f(x * scale * 3f, y * scale * 3f), shapeMaterial, shape);
                }
        }))
        .Build())
        .Run();

    static void SpawnAndTween(Commands commands, Vec2f pos, Handle<ShapeMaterial> shapeMaterial, Handle<Shape> shape)
    {
        var parent = commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2D.Default with
            {
                Position = pos,
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shape,
                Color = Color.RED,
            }))
            .Entity;

        Tween.Create(2f, pos, pos + Vec2f.Up * 64f)
                .OnEntity(parent)
                .OnField<Transform2D>(comp => comp.Position)
                .WithEasing(Easing.Quartic)
                .WithFlags(TweenFlag.PingPong)
                .Append(commands);

        var child1 = commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2D.Default with
            {
                Position = Vec2f.Up * 256f,
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shape,
                Color = Color.BLUE,
            }))
            .SetParent(parent)
            .Entity;

        Tween.Create(2f, Vec2f.Up * 256f, Vec2f.Up * 256f + Vec2f.Right * 32f)
            .OnEntity(child1)
            .OnField<Transform2D>(comp => comp.Position)
            .WithEasing(Easing.Quartic)
            .WithFlags(TweenFlag.PingPong)
            .Append(commands);

        var child2 = commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2D.Default with
            {
                Position = Vec2f.Up * 256f,
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shape,
                Color = Color.GREEN,
            }))
            .SetParent(child1)
            .Entity;

        Tween.Create(2f, Vec2f.One, Vec2f.One * 1.5f)
            .OnEntity(child2)
            .OnField<Transform2D>(comp => comp.Scale)
            .WithEasing(Easing.Quartic)
            .WithFlags(TweenFlag.PingPong)
            .Append(commands);
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}