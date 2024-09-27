namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public class TransformExample : IExample
{
    public string Name => "transform";

    IApplication? app;

    struct Component1 : IComponent { }
    struct Base : IComponent { }
    struct Rotate : IComponent
    {
        public float MinAngle;
        public float MaxAngle;
        public float Speed;
    }

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new AssetPlugin() { RootPath = "assets" },
            new HierarchyPlugin(),
            new TransformPlugin<Transform2D>(),
            new RenderingPlugin(),
            new ShapePlugin(),
            new InputPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn",
        static (
            World world, Commands commands,
            AssetServer assetServer, Assets<Shape> shapes,
            Assets<ShapeMaterial> shapeMaterials
        ) =>
        {
            commands.Spawn(Camera2D.Bundle);
            var shapeMaterial = shapeMaterials.Add(new ShapeMaterial
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/shape.wgsl"),
            });

            var parent = world.Spawn(
                GlobalTransform.Default,
                Transform2D.Default with
                {
                    Position = new Vec2f(300, 300),
                },
                new Base(),
                new ShapeDraw()
                {
                    MaterialHandle = shapeMaterial,
                    ShapeHandle = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * 32f)),
                    Color = Color.GREEN,
                });

            commands.AddChild(parent, world.Spawn(
                GlobalTransform.Default,
                Transform2D.Default with
                {
                    Position = new Vec2f(1, 0) * 128f,
                },
                new ShapeDraw()
                {
                    MaterialHandle = shapeMaterial,
                    ShapeHandle = shapes.Add(Shape.EquilateralTriangle(Vec2f.Zero, 32f)),
                    Color = Color.RED,
                }
            ));

            var child2 = world.Spawn(
                GlobalTransform.Default,
                Transform2D.Default with
                {
                    Position = new Vec2f(0, 1) * 128f,
                },
                new ShapeDraw()
                {
                    MaterialHandle = shapeMaterial,
                    ShapeHandle = shapes.Add(Shape.Circle(Vec2f.Zero, 32f)),
                    Color = Color.RED,
                },
                new Rotate()
                {
                    MinAngle = -45f,
                    MaxAngle = 45f,
                    Speed = 180f,
                }
            );
            commands.AddChild(parent, child2);

            commands.AddChild(child2, world.Spawn(
                GlobalTransform.Default,
                Transform2D.Default with
                {
                    Position = new Vec2f(0, 1) * 128f,
                },
                new ShapeDraw()
                {
                    MaterialHandle = shapeMaterial,
                    ShapeHandle = shapes.Add(Shape.Circle(Vec2f.Zero, 32f)),
                    Color = Color.BLUE,
                }
            ));
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Move",
        static (Time time, ButtonInput<Key> keys, 
                Query<Transform2D>.Filter<All<Base>> qRoots,
                Query<Transform2D, Rotate> qRotate
        ) =>
        {
            var move = keys.GetAxis2D(Key.KeyA, Key.KeyD, Key.KeyS, Key.KeyW);
            var rotate = keys.GetAxis(Key.KeyE, Key.KeyQ);

            if (move != Vec2f.Zero || rotate != 0f)
            {
                foreach (var entity in qRoots)
                {
                    ref var transform = ref entity.Component0;
                    transform.Position += move * time.DeltaTimeF * 1000f;
                    transform.Rotation += rotate * time.DeltaTimeF * 360f;
                }
            }

            qRotate.ForEach(time.DeltaTimeF, static (in float deltaTime, ref Transform2D transform, ref Rotate rotate) =>
            {
                if (transform.Rotation < rotate.MinAngle)
                {
                    transform.Rotation = rotate.MinAngle;
                    rotate.Speed = -rotate.Speed;
                }
                else if (transform.Rotation > rotate.MaxAngle)
                {
                    transform.Rotation = rotate.MaxAngle;
                    rotate.Speed = -rotate.Speed;
                }

                transform.Rotation += rotate.Speed * deltaTime;
            });
        }))
        .Build()).Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}

