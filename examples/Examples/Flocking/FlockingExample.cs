namespace Pollus.Examples.Flocking;

using System.Diagnostics.CodeAnalysis;
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
                new GizmoPlugin(),
                new ShapePlugin(),
                new InputPlugin(),
                new ImguiPlugin(),
                new AudioPlugin(),
                new PerformanceTrackerPlugin(),
                new RandomPlugin(),
                // SpatialPlugin.Grid(64, 2048 / 64, 2048 / 64),
                SpatialPlugin.LooseGrid(64, 32, 4096),
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
        world.Resources.Add(new CommonResources());
        world.Resources.Add(new BoidSettings()
        {
            Insect = new BoidSettings.Settings()
            {
                MaxSpeed = 50f,
                VisionRange = 50f,
                SeparationFactor = 200f,
                CohesionFactor = 0.01f,
                AlignmentFactor = 0.01f,
                SteeringFactor = 2.0f,
            },
            SmallBird = new BoidSettings.Settings()
            {
                MaxSpeed = 100f,
                VisionRange = 200f,
                SeparationFactor = 200f,
                CohesionFactor = 0.1f,
                AlignmentFactor = 0.75f,
                SteeringFactor = 0.8f,
            },
            LargeBird = new BoidSettings.Settings()
            {
                MaxSpeed = 150f,
                VisionRange = 150f,
                SeparationFactor = 200f,
                CohesionFactor = 0.1f,
                AlignmentFactor = 0.1f,
                SteeringFactor = 0.6f,
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

            for (int i = 0; i < 1000; i++)
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
                var force = 25f;

                // Bottom
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(window.Size.X / 2f, window.Size.Y - 16f),
                    },
                    new AvoidArea() { Force = force, Target = BoidType.All },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(window.Size.X / 2f, 64f))
                ));

                // Top
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(window.Size.X / 2f, 0f),
                    },
                    new AvoidArea() { Force = force, Target = BoidType.All },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(window.Size.X / 2f, 64f))
                ));

                // Left
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(0f, window.Size.Y / 2f),
                    },
                    new AvoidArea() { Force = force, Target = BoidType.All },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(64f, window.Size.Y / 2f))
                ));

                // Right
                commands.Spawn(Entity.With(
                    GlobalTransform.Default,
                    Transform2D.Default with
                    {
                        Position = new Vec2f(window.Size.X - 16f, window.Size.Y / 2f),
                    },
                    new AvoidArea() { Force = force, Target = BoidType.All },
                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(64f, window.Size.Y / 2f))
                ));
            }

            var debugEntity = commands.Spawn(Entity.With(
                GlobalTransform.Default,
                Transform2D.Default,
                new DebugBoid(),
                new AvoidArea() { Force = 10f, Target = BoidType.All },
                CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(100f, 50f))
            ));
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("UpdateBoids")
        {
            Locals = [],
        },
        static (
            SpatialQuery spatialQuery,
            BoidSettings boidSettings,
            Query query, IWindow window, Time time,
            Query<Transform2D, Velocity, Boid> qBoids
        ) =>
        {
            qBoids.ForEach((spatialQuery, query, time.DeltaTimeF, boidSettings),
            static (in (SpatialQuery spatialQuery, Query query, float deltaTime, BoidSettings boidSettings) userData,
                in Entity entity, ref Transform2D transform, ref Velocity velocity, ref Boid boid) =>
            {
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
                    var count = userData.spatialQuery.Query(transform.Position, settings.VisionRange, boid.Group, neighbors);
                    foreach (var neighbor in neighbors[..count])
                    {
                        if (neighbor == entity) continue;

                        ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);
                        ref var otherVelocity = ref userData.query.Get<Velocity>(neighbor);

                        var direction = otherTransform.Position - transform.Position;
                        float distance = direction.Length();
                        if (distance < settings.VisionRange)
                        {
                            cohesion.Add(otherTransform.Position);
                            alignment.Add(otherVelocity.Value);
                        }

                        direction = direction.Normalized();
                        if (distance < 25f) separation.Add(-direction / distance);
                        if (distance < 4f) separation.Add(-direction / distance * 10f);
                    }
                }
                {
                    var count = userData.spatialQuery.Query(transform.Position, 25f, ~boid.Group, neighbors);
                    foreach (var neighbor in neighbors[..count])
                    {
                        if (neighbor == entity) continue;

                        ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);

                        Vec2f toNeighbor = otherTransform.Position - transform.Position;
                        float distance = toNeighbor.Length();
                        separation.Add(-toNeighbor.Normalized() / distance);
                        if (distance < 4f) separation.Add(-toNeighbor.Normalized() / distance * 10f);
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

                velocity.Value += steeringForce * settings.SteeringFactor * userData.deltaTime;
                velocity.Value = velocity.Value.ClampLength(0, settings.MaxSpeed);
            });
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("Avoid"),
        static (
            Time time, Query query, SpatialQuery spatial,
            Query<Transform2D, CollisionShape, AvoidArea> qAvoid,
            Query<Transform2D, CollisionShape, Velocity> qBoids
        ) =>
        {
            qAvoid.ForEach(
                (spatial, query, time.DeltaTimeF),
                static (
                    in (SpatialQuery spatial, Query query, float deltaTime) userData,
                    in Entity entity, ref Transform2D transform, ref CollisionShape shape, ref AvoidArea avoid
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
                            var magnitude = otherVelocity.Value.Length();
                            otherVelocity.Value += intersection.Normal * avoid.Force * intersection.Distance * userData.deltaTime;
                        }
                    }
                }
            );
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("AvoidTarget"),
        static (Time time, Query query, SpatialQuery spatial, Query<Transform2D, Velocity, AvoidTarget> qBoid) =>
        {
            qBoid.ForEach((time.DeltaTimeF, query, spatial),
            static (in (float deltaTime, Query query, SpatialQuery spatial) userData, ref Transform2D transform, ref Velocity velocity, ref AvoidTarget avoidTarget) =>
            {
                const float RANGE = 50f;
                Span<Entity> neighbors = stackalloc Entity[1024];

                var count = userData.spatial.Query(transform.Position, RANGE, avoidTarget.Target, neighbors);
                foreach (var neighbor in neighbors[..count])
                {
                    ref var otherTransform = ref userData.query.Get<Transform2D>(neighbor);
                    var direction = otherTransform.Position - transform.Position;
                    var distance = direction.Length();
                    direction = direction.Normalized();

                    var force = Math.Smoothstep(0f, avoidTarget.Force, distance / RANGE);
                    velocity.Value += -direction * force * userData.deltaTime;
                }
            });
        }));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create(new("ScreenBoundsRebound")
        {
            RunsAfter = ["ApplyVelocity"]
        },
        static (Query<Transform2D, Velocity>.Filter<All<Boid>> qBoids, IWindow window, Time time) =>
        {
            const float REBOUND_FORCE = 3f;
            qBoids.ForEach((window.Size, time.DeltaTimeF),
            static (in (Vec2<uint> size, float deltaTime) userData, ref Transform2D transform, ref Velocity velocity) =>
            {
                if (transform.Position.X > userData.size.X) velocity.Value += Vec2f.Left * velocity.Value.Length() * userData.deltaTime * REBOUND_FORCE;
                else if (transform.Position.X < 0) velocity.Value += Vec2f.Right * velocity.Value.Length() * userData.deltaTime * REBOUND_FORCE;
                if (transform.Position.Y > userData.size.Y) velocity.Value += Vec2f.Down * velocity.Value.Length() * userData.deltaTime * REBOUND_FORCE;
                else if (transform.Position.Y < 0) velocity.Value += Vec2f.Up * velocity.Value.Length() * userData.deltaTime * REBOUND_FORCE;
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

        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create("DebugDraw",
        static (Local<bool> active, ButtonInput<Key> keys, Gizmos gizmos,
            SpatialQuery spatialQuery,
            Query<Transform2D, Velocity, CollisionShape>.Filter<All<Boid>> qBoids,
            Query<Transform2D, AvoidArea, CollisionShape> qAvoids
        ) =>
        {
            if (keys.Pressed(Key.KeyO))
            {
                spatialQuery.Visualize(gizmos);
            }

            if (keys.Pressed(Key.KeyP))
            {
                foreach (var boid in qBoids)
                {
                    gizmos.DrawRay(boid.Component0.Position, boid.Component1.Value.Normalized(), Color.GREEN, 50f);
                    gizmos.DrawCircle(boid.Component0.Position, boid.Component2.GetBoundingCircle(boid.Component0).Radius, Color.RED);
                }

                foreach (var boid in qAvoids)
                {
                    if (boid.Component2.Type == CollisionShapeType.Rectangle)
                    {
                        var bounds = boid.Component2.GetShape<Bounds2D>();
                        gizmos.DrawRectFilled(boid.Component0.Position + bounds.Center, bounds.Extents, 0f, Color.RED.WithAlpha(0.1f));
                    }
                    else if (boid.Component2.Type == CollisionShapeType.Circle)
                    {
                        var circle = boid.Component2.GetShape<Circle2D>();
                        gizmos.DrawCircle(boid.Component0.Position + circle.Center, circle.Radius, Color.RED.WithAlpha(0.1f));
                    }
                }
            }
        }));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new("DebugBoids")
        {
            RunsAfter = ["UpdateSpatialQuery"],
            Locals = []
        },
        static (
            SpatialQuery spatialQuery,
            InputManager input, IWindow window,
            Query<Transform2D, DebugBoid, CollisionShape> qDebug) =>
        {
            if (input.GetDevice<Mouse>("mouse") is not { } mouse) return;

            var debugDraw = qDebug.Single();
            if (mouse.Pressed(MouseButton.Left))
            {
                var mousePos = mouse.Position;
                var mousePosWorld = new Vec2f(mousePos.X, window.Size.Y - mousePos.Y);
                debugDraw.Component0.Position = mousePosWorld;
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
            },
            new AvoidTarget()
            {
                Force = group switch
                {
                    BoidType.Insect => 250f,
                    BoidType.SmallBird => 500f,
                    _ => 0f,
                },
                Target = group switch
                {
                    BoidType.Insect => BoidType.SmallBird,
                    BoidType.SmallBird => BoidType.LargeBird,
                    _ => BoidType.None,
                }
            }
        ));
    }
}

struct DebugBoid : IComponent { }
struct AvoidArea : IComponent
{
    public required float Force;
    public required BoidType Target;
}

struct AvoidTarget : IComponent
{
    public required float Force;
    public required BoidType Target;
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
    None = 0u,
    All = ~0u,
    Insect = 1u << 0,
    SmallBird = 1u << 1,
    LargeBird = 1u << 2,
}

struct Boid : IComponent
{
    public required BoidType Group;
}

class BoidSettings
{
    public class Settings
    {
        public required float MaxSpeed;
        public required float SteeringFactor;
        public required float SeparationFactor;
        public required float CohesionFactor;
        public required float AlignmentFactor;
        public required float VisionRange;
    }

    public required Settings Insect;
    public required Settings SmallBird;
    public required Settings LargeBird;
}