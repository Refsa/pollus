namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Physics;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Mathematics.Collision2D;
using Pollus.Utils;

public class CollisionExample : IExample
{
    public string Name => "collision";
    IApplication? application;
    public void Stop() => application?.Shutdown();

    struct MoveShape : IComponent
    {
        public Vec2f Start;
        public Vec2f End;
        public float Speed;
    }

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new AssetPlugin {RootPath = "assets"},
            new RenderingPlugin(),
            new ShapePlugin(),
        ])
        .AddSystem(CoreStage.PostInit, SystemBuilder.FnSystem("Setup",
        static (Commands commands, AssetServer assetServer, Assets<ShapeMaterial> shapeMaterials, Assets<Shape> shapes) =>
        {
            commands.Spawn(Camera2D.Bundle);

            var shapeMaterial = shapeMaterials.Add(new ShapeMaterial
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/shape.wgsl"),
            });

            commands.Spawn(Entity.With(
                Transform2.Default with { Position = new Vec2f(128f, 128f) },
                new ShapeDraw
                {
                    MaterialHandle = shapeMaterial,
                    ShapeHandle = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * 32f)),
                    Color = Color.BLUE,
                },
                CollisionShape.Rectangle(Vec2f.Zero, Vec2f.One * 32f)
            ));

            commands.Spawn(Entity.With(
                Transform2.Default with { Position = new Vec2f(128f - 96f, 128f - 96f) },
                new ShapeDraw
                {
                    MaterialHandle = shapeMaterial,
                    ShapeHandle = shapes.Add(Shape.Circle(Vec2f.Zero, 32f)),
                    Color = Color.RED,
                },
                new MoveShape
                {
                    Start = new Vec2f(128f - 96f, 128f - 96f),
                    End = new Vec2f(128f + 96f, 128f + 96f),
                    Speed = 150f,
                },
                CollisionShape.Circle(32f)
            ));
        }))
        .AddSystem(CoreStage.Update, SystemBuilder.FnSystem("Update",
        static (Query<Transform2, MoveShape, ShapeDraw> qMoveShapes, Query<Transform2, CollisionShape, ShapeDraw> qCollisions, Time time) =>
        {
            qMoveShapes.ForEach((ref Transform2 transform, ref MoveShape moveShape, ref ShapeDraw shapeDraw) =>
            {
                var direction = (moveShape.End - transform.Position).Normalized();
                transform.Position += direction * moveShape.Speed * (float)time.DeltaTimeF;

                if ((transform.Position - moveShape.End).LengthSquared() < 1f)
                {
                    var temp = moveShape.Start;
                    moveShape.Start = moveShape.End;
                    moveShape.End = temp;
                }
            });

            qCollisions.ForEach((in Entity entity, ref Transform2 transform, ref CollisionShape shape, ref ShapeDraw draw) =>
            {
                var _entity = entity;
                var _shape = shape;
                var _transform = transform;
                bool collision = false;

                qCollisions.ForEach((in Entity oEntity, ref Transform2 oTransform, ref CollisionShape oShape, ref ShapeDraw oDraw) =>
                {
                    if (_entity == oEntity) return;
                    
                    collision = collision || _shape.GetIntersection(_transform, oShape, oTransform).IsIntersecting;
                });

                draw.Color = collision ? Color.GREEN : Color.RED;
            });
        }))
        .Build())
        .Run();
}