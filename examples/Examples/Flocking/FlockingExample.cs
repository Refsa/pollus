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
using Pollus.Engine.Physics;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Mathematics.Collision2D;
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
    public Handle<Shape> InsectShape = Handle<Shape>.Null;
    public Handle<Shape> SmallBirdShape = Handle<Shape>.Null;
    public Handle<Shape> LargeBirdShape = Handle<Shape>.Null;
    public Handle<ShapeMaterial> BoidMaterial = Handle<ShapeMaterial>.Null;
}

class FlockingGame : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(new SpatialQuery(64, 2048 / 64, 1024 / 64));
        world.Resources.Add(new CommonResources());
        world.Resources.Add(new BoidSettings()
        {
            Insect = new BoidSettings.Settings()
            {
                MaxSpeed = 50f,
                SeparationFactor = 25f,
                CohesionFactor = 0.01f,
                AlignmentFactor = 0.1f,
            },
            SmallBird = new BoidSettings.Settings()
            {
                MaxSpeed = 100f,
                SeparationFactor = 15f,
                CohesionFactor = 0.25f,
                AlignmentFactor = 0.25f,
            },
            LargeBird = new BoidSettings.Settings()
            {
                MaxSpeed = 150f,
                SeparationFactor = 25f,
                CohesionFactor = 0.25f,
                AlignmentFactor = 0.25f,
            },
        });

        world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create("SpawnBoids",
        static (Commands commands, AssetServer assetServer, CommonResources commonResources,
                Assets<Shape> shapes, Assets<ShapeMaterial> shapeMaterials, Random random, IWindow window
        ) =>
        {
            commands.Spawn(Camera2D.Bundle);

            commonResources.InsectShape = shapes.Add(Shape.Circle(Vec2f.Zero, 1f));
            commonResources.SmallBirdShape = shapes.Add(Shape.Kite(Vec2f.Zero, 4f, 4f));
            commonResources.LargeBirdShape = shapes.Add(Shape.Kite(Vec2f.Zero, 8f, 8f));

            commonResources.BoidMaterial = shapeMaterials.Add(new ShapeMaterial()
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/shape.wgsl"),
            });

            for (int i = 0; i < 0; i++)
            {
                var entity = SpawnBoid(commands, commonResources,
                    position: new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                    velocity: new Vec2f(random.NextFloat(-100, 100), random.NextFloat(-100, 100)).ClampLength(10f, 100f),
                    color: Color.YELLOW,
                    group: BoidType.Insect
                );
            }

            for (int i = 0; i < 1; i++)
            {
                var entity = SpawnBoid(commands, commonResources,
                    position: new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                    velocity: new Vec2f(random.NextFloat(-100, 100), random.NextFloat(-100, 100)).ClampLength(10f, 100f),
                    color: Color.GREEN,
                    group: BoidType.SmallBird
                );
            }

            for (int i = 0; i < 0; i++)
            {
                var entity = SpawnBoid(commands, commonResources,
                    position: new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                    velocity: new Vec2f(random.NextFloat(-100, 100), random.NextFloat(-100, 100)).ClampLength(10f, 100f),
                    color: Color.RED,
                    group: BoidType.LargeBird
                );
            }

            // edge avoids
            {
                // Bottom
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(window.Size.X / 2f, 32f),
                    },
                    new BoidAvoid() { Force = 1000f },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(window.Size.X, 32f))
                ));

                // Top
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(window.Size.X / 2f, window.Size.Y - 32f),
                    },
                    new BoidAvoid() { Force = 1000f },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(window.Size.X, 32f))
                ));

                // Left
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(32f, window.Size.Y / 2f),
                    },
                    new BoidAvoid() { Force = 1000f },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(32f, window.Size.Y))
                ));

                // Right
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(window.Size.X - 32f, window.Size.Y / 2f),
                    },
                    new BoidAvoid() { Force = 1000f },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(32f, window.Size.Y))
                ));
            }

            var debugEntity = commands.Spawn(Entity.With(
                GlobalTransform.Default,
                Transform2D.Default,
                new DebugBoid(),
                new BoidAvoid() { Force = 1000f },
                CollisionShape.Rectangle(Vec2f.Zero, Vec2f.One * 100f),
                new ShapeDraw()
                {
                    MaterialHandle = commonResources.BoidMaterial,
                    ShapeHandle = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * 100f)),
                    Color = Color.RED.WithAlpha(0.1f),
                }
            ));
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("UpdateBoids")
        {
            Locals = [],
        },
        static (
            SpatialQuery spatialQuery,
            Query<Transform2D, Velocity, Boid> qBoids,
            BoidSettings boidSettings,
            Query query, IWindow window, Time time) =>
        {
            qBoids.ForEach((spatialQuery, query, time.DeltaTimeF, boidSettings),
            static (in (SpatialQuery spatialQuery, Query query, float deltaTime, BoidSettings boidSettings) userData,
                in Entity entity, ref Transform2D transform, ref Velocity velocity, ref Boid boid) =>
            {
                const float MAX_NEIGHBOR_DIST = 50f;
                const float MAX_NEIGHBOR_DIST_SQR = MAX_NEIGHBOR_DIST * MAX_NEIGHBOR_DIST;

                var settings = boid.Group switch
                {
                    BoidType.Insect => userData.boidSettings.Insect,
                    BoidType.SmallBird => userData.boidSettings.SmallBird,
                    BoidType.LargeBird => userData.boidSettings.LargeBird,
                    _ => throw new IndexOutOfRangeException(),
                };

                var separation = new BoidCalc();
                var cohesion = new BoidCalc();
                var alignment = new BoidCalc();
                Span<Entity> neighbors = stackalloc Entity[1024];

                {
                    var count = userData.spatialQuery.Query(transform.Position, MAX_NEIGHBOR_DIST, boid.Group, neighbors);
                    foreach (var neighbor in neighbors[..count])
                    {
                        if (neighbor == entity) continue;

                        ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);
                        ref var otherVelocity = ref userData.query.Get<Velocity>(neighbor);

                        float distance = (otherTransform.Position - transform.Position).LengthSquared();
                        if (distance < MAX_NEIGHBOR_DIST_SQR)
                        {
                            cohesion.Add(otherTransform.Position);
                            alignment.Add(otherVelocity.Value);
                        }
                    }
                }
                {
                    var count = userData.spatialQuery.Query(transform.Position, MAX_NEIGHBOR_DIST / 4f, ~0u, neighbors);
                    foreach (var neighbor in neighbors[..count])
                    {
                        if (neighbor == entity) continue;

                        ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);

                        Vec2f toNeighbor = otherTransform.Position - transform.Position;
                        float distance = toNeighbor.Length();
                        if (distance < MAX_NEIGHBOR_DIST)
                        {
                            separation.Add(-toNeighbor.Normalized() / distance);
                        }
                    }
                }

                Vec2f steeringForce = Vec2f.Zero;
                if (separation.Count > 0)
                {
                    steeringForce += separation.Sum * settings.SeparationFactor;
                }
                if (cohesion.Count > 0)
                {
                    steeringForce += (cohesion.Average - transform.Position) * settings.CohesionFactor;
                }
                if (alignment.Count > 0)
                {
                    steeringForce += alignment.Average * settings.AlignmentFactor;
                }

                velocity.Value += steeringForce * userData.deltaTime * 1f;
                velocity.Value = velocity.Value.ClampLength(0, settings.MaxSpeed);
            });
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("Avoid"),
        static (
            Time time, Query query, SpatialQuery spatial,
            Query<Transform2D, CollisionShape, BoidAvoid> qAvoid,
            Query<Transform2D, CollisionShape, Velocity> qBoids
        ) =>
        {
            qAvoid.ForEach(
                (spatial, query, time.DeltaTimeF),
                static (
                    in (SpatialQuery spatial, Query query, float deltaTime) userData,
                    in Entity entity, ref Transform2D transform, ref CollisionShape shape, ref BoidAvoid avoid
                ) =>
                {
                    var boundingCircle = shape.GetBoundingCircle(transform);

                    Span<Entity> neighbors = stackalloc Entity[1024];
                    var count = userData.spatial.Query(transform.Position, boundingCircle.Radius, ~0u, neighbors);

                    foreach (var neighbor in neighbors[..count])
                    {
                        if (neighbor == entity) continue;
                        ref var boidCollider = ref userData.query.Get<CollisionShape>(neighbor);
                        ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);

                        var intersection = shape.GetIntersection(transform, boidCollider, otherTransform);
                        if (intersection.IsIntersecting)
                        {
                            ref var otherVelocity = ref userData.query.Get<Velocity>(neighbor);
                            otherVelocity.Value += intersection.Normal * intersection.Distance * avoid.Force * userData.deltaTime;
                            Log.Info($"{intersection}");
                        }
                    }
                }
            );
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

        /* world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create(new("WrapBoids")
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
        })); */

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create("UpdateSpatialQuery",
        static (Commands commands, SpatialQuery spatialQuery, Query<Transform2D, CollisionShape, Boid> qBoids) =>
        {
            spatialQuery.Clear();
            qBoids.ForEach(spatialQuery, static (in SpatialQuery spatialQuery, in Entity entity, ref Transform2D transform, ref CollisionShape shape, ref Boid boid) =>
            {
                var circle = shape.GetShape<Circle2D>();
                spatialQuery.Insert(entity, transform.Position, circle.Radius, boid.Group);
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
            Query<Transform2D, DebugBoid, CollisionShape> qDebug,
            Query query, Time time) =>
        {
            if (input.GetDevice<Mouse>("mouse") is not { } mouse) return;

            var debugDraw = qDebug.Single();
            if (mouse.Pressed(MouseButton.Left))
            {
                var mousePos = mouse.Position;
                var mousePosWorld = new Vec2f(mousePos.X, window.Size.Y - mousePos.Y);
                debugDraw.Component0.Position = mousePosWorld;
            }

            foreach (var neighbor in neighbors.Value.AsSpan(neighbors.Value.Count))
            {
                ref var otherShapeDraw = ref query.Get<ShapeDraw>(neighbor);
                otherShapeDraw.Color = Color.GREEN;
            }

            neighbors.Value.EnsureCapacity(1024);
            var neighborsSpan = neighbors.Value.AsSpan(1024);
            var count = spatialQuery.Query(
                debugDraw.Component0.Position,
                debugDraw.Component2.GetBoundingCircle(debugDraw.Component0).Radius,
                BoidType.SmallBird, neighborsSpan);

            neighbors.Value.SetCount(count);
            foreach (var neighbor in neighborsSpan[..count])
            {
                ref var otherShape = ref query.Get<CollisionShape>(neighbor);
                ref var otherTransform = ref query.Get<Transform2D>(neighbor);

                var intersection = debugDraw.Component2.GetIntersection(debugDraw.Component0, otherShape, otherTransform);
                if (intersection.IsIntersecting)
                {
                    ref var otherShapeDraw = ref query.Get<ShapeDraw>(neighbor);
                    otherShapeDraw.Color = Color.RED;
                }
            }
        }));
    }

    static Entity SpawnBoid(Commands commands, CommonResources commonResources, Vec2f position, Vec2f velocity, BoidType group, Color color)
    {
        return commands.Spawn(Entity.With(
            GlobalTransform.Default,
            Transform2D.Default with
            {
                Position = position,
                Scale = Vec2f.One,
            },
            new Velocity() { Value = velocity, MaxSpeed = 100f },
            new Boid() { Group = group },
            CollisionShape.Circle(group switch
            {
                BoidType.Insect => 1f,
                BoidType.SmallBird => 2f,
                BoidType.LargeBird => 4f,
                _ => 1f,
            }),
            new Energy()
            {
                Current = 100f,
                Max = 100f,
            },
            new Hunger()
            {
                Current = 100f,
                Max = 100f,
            },
            new ShapeDraw()
            {
                MaterialHandle = commonResources.BoidMaterial,
                ShapeHandle = group switch
                {
                    BoidType.Insect => commonResources.InsectShape,
                    BoidType.SmallBird => commonResources.SmallBirdShape,
                    BoidType.LargeBird => commonResources.LargeBirdShape,
                    _ => commonResources.InsectShape,
                },
                Color = color,
            }
        ));
    }
}

struct DebugBoid : IComponent { }
struct BoidAvoid : IComponent
{
    public required float Force;
}

struct BoidCalc
{
    public Vec2f Sum;
    public int Count;
    public Vec2f Average => Sum / Count;
    public void Add(in Vec2f value)
    {
        Sum += value;
        Count++;
    }
}

struct Velocity : IComponent
{
    public required Vec2f Value;
    public required float MaxSpeed;
}

enum BoidType : uint
{
    Insect = 1u << 0,
    SmallBird = 1u << 1,
    LargeBird = 1u << 2,
}

struct Boid : IComponent
{
    public required BoidType Group;
}

struct Energy : IComponent
{
    public required float Current;
    public required float Max;
}

struct Hunger : IComponent
{
    public required float Current;
    public required float Max;
}

class BoidSettings
{
    public struct Settings
    {
        public required float MaxSpeed;
        public required float SeparationFactor;
        public required float CohesionFactor;
        public required float AlignmentFactor;
    }

    public required Settings Insect;
    public required Settings SmallBird;
    public required Settings LargeBird;
}