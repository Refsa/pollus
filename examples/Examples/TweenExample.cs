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
            var shape = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * 1f));

            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    SpawnAndTween(commands, Vec2f.One * 50f + new Vec2f(x * 3f, y * 3f), shapeMaterial, shape);
                }
            }
        }))
        .Build())
        .Run();

    static void SpawnAndTween(Commands commands, Vec2f pos, Handle<ShapeMaterial> shapeMaterial, Handle<Shape> shape)
    {
        var shapeEntity = commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2D.Default with
            {
                Position = pos,
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shape,
                Color = Color.RED,
            }));

        Tween.Create(2f, pos, pos + Vec2f.Up * 16f)
            .WithEasing(Easing.Quartic)
            .WithFlags(TweenFlag.PingPong)
            .OnEntity(shapeEntity)
            .OnField<Transform2D>(comp => comp.Position)
            .Append(commands);
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}