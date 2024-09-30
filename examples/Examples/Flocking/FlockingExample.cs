namespace Pollus.Examples.Flocking;

using Pollus.Collections;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Audio;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Imgui;
using Pollus.Engine.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Spatial;
using Pollus.Utils;

public class FlockingExample : IExample
{
    public string Name => "flocking";

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .WithWindowOptions(WindowOptions.Default with
            {
                Width = 2048,
                Height = 1024,
            })
            .AddPlugins([
                new TimePlugin(),
                new AssetPlugin() {RootPath = "assets"},
                new HierarchyPlugin(),
                new TransformPlugin<Transform2D>(),
                new RenderingPlugin(),
                new ShapePlugin(),
                new InputPlugin(),
                new ImguiPlugin(),
                new AudioPlugin(),
                new PerformanceTrackerPlugin(),
                new RandomPlugin(),
                new FlockingGame(),
            ])
            .Build();

        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}

class CommonResources
{
    public Handle<Shape> BoidShape = Handle<Shape>.Null;
    public Handle<ShapeMaterial> BoidMaterial = Handle<ShapeMaterial>.Null;
}

class FlockingGame : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(new SpatialQuery(64, 2048 / 64, 1024 / 64));
        world.Resources.Add(new CommonResources());

        world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create("SpawnBoids",
        static (Commands commands, AssetServer assetServer, CommonResources commonResources,
                Assets<Shape> shapes, Assets<ShapeMaterial> shapeMaterials, Random random, IWindow window
        ) =>
        {
            commands.Spawn(Camera2D.Bundle);

            commonResources.BoidShape = shapes.Add(Shape.Kite(Vec2f.Zero, 4f, 4f));
            commonResources.BoidMaterial = shapeMaterials.Add(new ShapeMaterial()
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/shape.wgsl"),
            });

            for (int i = 0; i < 10_000; i++)
            {
                var entity = SpawnBoid<BoidGroup0>(commands, commonResources,
                    position: new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                    velocity: new Vec2f(random.NextFloat(-100, 100), random.NextFloat(-100, 100)).ClampLength(10f, 100f),
                    color: Color.GREEN
                );
            }

            var debugEntity = commands.Spawn(Entity.With(
                GlobalTransform.Default,
                Transform2D.Default,
                new DebugBoid(),
                new BoidAvoid() { AvoidanceRadius = 100f },
                new ShapeDraw()
                {
                    MaterialHandle = commonResources.BoidMaterial,
                    ShapeHandle = shapes.Add(Shape.Circle(Vec2f.Zero, 100f)),
                    Color = Color.RED.WithAlpha(0.1f),
                }
            ));
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("UpdateBoids")
        {
            Locals = [Local.From(new ArrayList<Entity>())]
        },
        static (Local<ArrayList<Entity>> neighbors, SpatialQuery spatialQuery,
                Query<Transform2D, Velocity> qBoids,
                Query query, IWindow window, Time time) =>
        {
            qBoids.ForEach((spatialQuery, neighbors.Value, query, time.DeltaTimeF),
            static (in (SpatialQuery spatialQuery, ArrayList<Entity> neighbors, Query query, float deltaTime) userData,
                in Entity entity, ref Transform2D transform, ref Velocity velocity) =>
            {
                const float MAX_NEIGHBOR_DIST = 50f;

                const float SEPARATION_WEIGHT = 14f;
                const float COHESION_WEIGHT = 0.5f;
                const float ALIGNMENT_WEIGHT = 0.1f;
                const float MAX_SPEED = 200f;

                userData.neighbors.Clear();
                userData.spatialQuery.Query(transform.Position, MAX_NEIGHBOR_DIST, 1 << 0, userData.neighbors);

                Vec2f separationForce = Vec2f.Zero;
                Vec2f cohesionForce = Vec2f.Zero;
                Vec2f alignmentForce = Vec2f.Zero;
                int neighborCount = 0;

                foreach (var neighbor in userData.neighbors.AsSpan())
                {
                    if (neighbor == entity) continue;

                    ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);
                    ref var otherVelocity = ref userData.query.Get<Velocity>(neighbor);

                    Vec2f toNeighbor = otherTransform.Position - transform.Position;
                    float distance = toNeighbor.Length();

                    if (distance < MAX_NEIGHBOR_DIST)
                    {
                        separationForce -= toNeighbor.Normalized() / distance;
                        cohesionForce += otherTransform.Position;
                        alignmentForce += otherVelocity.Value;
                        neighborCount++;
                    }
                }

                if (neighborCount > 1)
                {
                    cohesionForce = cohesionForce / neighborCount - transform.Position;
                    alignmentForce = alignmentForce / neighborCount;

                    Vec2f steeringForce =
                        separationForce * SEPARATION_WEIGHT +
                        cohesionForce * COHESION_WEIGHT +
                        alignmentForce * ALIGNMENT_WEIGHT;

                    velocity.Value += steeringForce * userData.deltaTime * 10;
                    velocity.Value = velocity.Value.ClampLength(0, MAX_SPEED);
                }
            });
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create("Avoid",
        static (Query<Transform2D, BoidAvoid> qAvoid, Query<Transform2D, Velocity> qBoids, Time time) =>
        {
            qBoids.ForEach(qAvoid, static (in Query<Transform2D, BoidAvoid> qAvoid, in Entity entity, ref Transform2D transform, ref Velocity velocity) =>
            {
                foreach (var avoid in qAvoid)
                {
                    Vec2f toAvoid = avoid.Component0.Position - transform.Position;
                    float distance = toAvoid.Length();
                    if (distance < avoid.Component1.AvoidanceRadius)
                    {
                        velocity.Value -= toAvoid.Normalized() / ((distance * distance) / 250f) * 50f;
                    }
                }
            });
        }));

        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create("AlignWithVelocity",
        static (Query<Transform2D, Velocity> qBoids, Time time) =>
        {
            qBoids.ForEach(time.DeltaTimeF, static (in float deltaTime, ref Transform2D transform, ref Velocity velocity) =>
            {
                if (velocity.Value.LengthSquared() > 0)
                {
                    var target = velocity.Value.Normalized().Angle().Degrees() - 90f;
                    transform.Rotation = Math.Lerp(transform.Rotation, target, deltaTime * 10f);
                }
            });
        }));

        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create("ApplyVelocity",
        static (Query<Transform2D, Velocity> qDynamics, Time time) =>
        {
            qDynamics.ForEach(time.DeltaTimeF, static (in float deltaTime, ref Transform2D transform, ref Velocity velocity) =>
            {
                transform.Position += velocity.Value * deltaTime;
            });
        }));

        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create(new("WrapBoids")
        {
            RunsAfter = ["ApplyVelocity"]
        },
        static (Query<Transform2D> qBoids, IWindow window) =>
        {
            qBoids.ForEach(window.Size, static (in Vec2<uint> size, ref Transform2D transform) =>
            {
                if (transform.Position.X > size.X) transform.Position.X = 0;
                else if (transform.Position.X < 0) transform.Position.X = size.X;
                if (transform.Position.Y > size.Y) transform.Position.Y = 0;
                else if (transform.Position.Y < 0) transform.Position.Y = size.Y;
            });
        }));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create("UpdateSpatialQuery",
        static (Commands commands, SpatialQuery spatialQuery, Query<Transform2D, Collider> qBoids) =>
        {
            spatialQuery.Clear();
            qBoids.ForEach(spatialQuery, static (in SpatialQuery spatialQuery, in Entity entity, ref Transform2D transform, ref Collider collider) =>
            {
                spatialQuery.Insert(entity, transform.Position, collider.Radius, collider.Layer);
            });
        }));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new("DebugBoids")
        {
            RunsAfter = ["UpdateSpatialQuery"],
            Locals = [
                Local.From(new ArrayList<Entity>())
            ]
        },
        static (
            Local<ArrayList<Entity>> neighbors,
            SpatialQuery spatialQuery,
            InputManager input, IWindow window,
            Query<Transform2D, DebugBoid> qDebug,
            Query query, Time time) =>
        {
            if (input.GetDevice<Mouse>("mouse") is not { } mouse) return;
            var mousePos = mouse.Position;
            var mousePosWorld = new Vec2f(mousePos.X, window.Size.Y - mousePos.Y);

            var debugDraw = qDebug.Single();
            debugDraw.Component0.Position = mousePosWorld;

            foreach (var neighbor in neighbors.Value.AsSpan())
            {
                ref var otherShapeDraw = ref query.Get<ShapeDraw>(neighbor);
                otherShapeDraw.Color = Color.GREEN;
            }

            neighbors.Value.Clear();
            spatialQuery.Query(mousePosWorld, 100f, 1 << 0, neighbors.Value);
            foreach (var neighbor in neighbors.Value.AsSpan())
            {
                ref var otherShapeDraw = ref query.Get<ShapeDraw>(neighbor);
                otherShapeDraw.Color = Color.RED;
            }
        }));
    }

    static Entity SpawnBoid<TGroup>(Commands commands, CommonResources commonResources, Vec2f position, Vec2f velocity, Color color)
        where TGroup : unmanaged, IBoidGroup
    {
        return commands.Spawn(Entity.With(
            GlobalTransform.Default,
            Transform2D.Default with
            {
                Position = position,
                Scale = Vec2f.One,
            },
            new Velocity() { Value = velocity, MaxSpeed = 100f },
            new BoidGroup<TGroup>(),
            new Collider() { Radius = 8f, Layer = 1 << 0 },
            new ShapeDraw()
            {
                MaterialHandle = commonResources.BoidMaterial,
                ShapeHandle = commonResources.BoidShape,
                Color = color,
            }
        ));
    }
}

struct DebugBoid : IComponent { }
struct BoidAvoid : IComponent
{
    public required float AvoidanceRadius;
}

struct BoidCalc
{
    public Vec2f Sum;
    public int Count;
    public Vec2f Average => Sum / Count;
}

struct Velocity : IComponent
{
    public required Vec2f Value;
    public required float MaxSpeed;
}

struct Collider : IComponent
{
    public required float Radius;
    public uint Layer;
}

interface IBoidGroup : IComponent
{
    static abstract int Group { get; }
}

struct BoidGroup<TGroup> : IComponent
    where TGroup : unmanaged, IBoidGroup
{
    public int Group => TGroup.Group;
}

struct BoidGroup0 : IBoidGroup
{
    public static int Group => 0;
}
