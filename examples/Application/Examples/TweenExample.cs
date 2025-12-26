namespace Pollus.Examples;

using System.Text;
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
            new TweenPlugin()
                .Register<Test>()
                .Register<Transform2D>(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
        static (Commands commands, AssetServer assetServer, Assets<Shape> shapes, Assets<ShapeMaterial> shapeMaterials) =>
        {
            commands.Spawn(Camera2D.Bundle);

            var shapeMaterial = shapeMaterials.Add(new ShapeMaterial
            {
                ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/shape.wgsl"),
            });

            var scale = 4f;
            var shape = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * scale));

            SpawnAndSequence(commands, Vec2f.Right * 1400f + Vec2f.Up * 50f, shapeMaterial, shape);

            for (int x = 0; x < 100; x++)
                for (int y = 0; y < 10; y++)
                {
                    SpawnAndTween(commands, Vec2f.One * 50f + new Vec2f(x * scale * 3f, y * scale * 3f), shapeMaterial, shape);
                }
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create(new()
        {
            Label = "Print",
            Locals = [
                Local.From(new StringBuilder())
            ]
        },
        static (Local<float> logCD, Local<StringBuilder> sb, Time time, Query<Parent>.Filter<None<Child>> qRoots, Query query) =>
        {
            logCD.Value -= time.DeltaTimeF;
            if (logCD.Value > 0) return;
            logCD.Value = 1;

            /* Log.Info("########################");
            query.ForEach((query, sb.Value), static (in userdata, in entity) =>
            {
                userdata.Value.Clear();
                userdata.Value.AppendFormat("Entity: {0}\n", entity);
                foreach (var cid in userdata.query.GetComponents(entity))
                {
                    var cinfo = Component.GetInfo(cid);
                    userdata.Value.AppendFormat("\t{0}\n", cinfo.TypeName);
                }
                Log.Info(userdata.Value.ToString());
            }); */

            /* foreach (var root in qRoots)
            {
                Log.Info($"{root.Entity}");
                foreach (var child in query.HierarchyDFS(root.Entity))
                {
                    Log.Info($"{new string(' ', child.Depth * 2)}{child.Entity}");
                }
            }
            Log.Info(""); */
        }))
        .Build())
        .Run();

    static void SpawnAndSequence(Commands commands, Vec2f pos, Handle<ShapeMaterial> shapeMaterial, Handle<Shape> shape)
    {
        var entity = commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2D.Default with
            {
                Position = pos,
                Scale = Vec2f.One * 4,
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shape,
                Color = Color.ORANGE,
            }))
            .Entity;

        Tween.Sequence(commands)
            .WithFlags(TweenFlag.Loop)
            .Then(Tween.Create(entity, (Transform2D comp) => comp.Position)
                .WithFromTo(pos, pos + Vec2f.Up * 64f)
                .WithDuration(2f)
                .WithFlags(TweenFlag.None)
                .WithEasing(Easing.Quartic)
            )
            .Then(Tween.Create(entity, (Transform2D comp) => comp.Scale)
                .WithFromTo(Vec2f.One * 4, Vec2f.One * 8)
                .WithDuration(1f)
                .WithFlags(TweenFlag.None)
                .WithEasing(Easing.Quartic)
            )
            .Then(Tween.Create(entity, (Transform2D comp) => comp.Scale)
                .WithFromTo(Vec2f.One * 8, Vec2f.One * 4)
                .WithDuration(1f)
                .WithFlags(TweenFlag.None)
                .WithEasing(Easing.Quartic)
            )
            .Then(Tween.Create(entity, (Transform2D comp) => comp.Position)
                .WithFromTo(pos + Vec2f.Up * 64f, pos)
                .WithDuration(2f)
                .WithFlags(TweenFlag.None)
                .WithEasing(Easing.Quartic)
            )
            .Append();
    }

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

        Tween.Create(parent, (Transform2D comp) => comp.Position)
            .WithFromTo(pos, pos + Vec2f.Up * 64f)
            .WithDuration(2f)
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

        Tween.Create(child1, (Transform2D comp) => comp.Position)
            .WithFromTo(Vec2f.Up * 256f, Vec2f.Up * 256f + Vec2f.Right * 32f)
            .WithDuration(2f)
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

        Tween.Create(child2, (Transform2D comp) => comp.Scale)
            .WithFromTo(Vec2f.One, Vec2f.One * 1.5f)
            .WithDuration(2f)
            .WithEasing(Easing.Quartic)
            .WithFlags(TweenFlag.PingPong)
            .Append(commands);
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}