namespace Pollus.Examples;

using ImGuiNET;
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
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;
using static Pollus.ECS.SystemBuilder;

public class BreakoutGame
{
    struct Player : IComponent { }

    struct Disabled : IComponent { }

    struct Paddle : IComponent { }

    struct Brick : IComponent { }

    struct Ball : IComponent { }

    struct Velocity : IComponent
    {
        public required Vec2f Value;
    }

    struct Collider : IComponent
    {
        public required Rect Bounds;
    }

    struct MainMixer : IComponent
    {

    }

    struct Event
    {
        public struct BrickDestroyed { }
        public struct Collision
        {
            public Entity EntityA;
            public Entity EntityB;
            public Vec2f Normal;
        }
        public struct RestartGame { }
        public struct SpawnBall
        {
            public required int Count;
        }
    }


    enum State
    {
        NewGame,
        SpawnBall,
        Play,
        GameOver,
        Won,
    }

    class GameState
    {
        public State State;
        public int Lives;
        public int Score;

        public Handle<SpriteMaterial> spritesheet;
    }

    public void Run() => Application.Builder
        .WithWindowOptions(WindowOptions.Default with
        {
            Title = "Breakout Game",
            Width = 900,
            Height = 600,
        })
        .InitEvent<Event.BrickDestroyed>()
        .InitEvent<Event.Collision>()
        .InitEvent<Event.RestartGame>()
        .InitEvent<Event.SpawnBall>()
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
            new ImguiPlugin(),
            new AudioPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddResource(new GameState { State = State.NewGame, Lives = 3, Score = 0 })
        .AddSystem(CoreStage.PostInit, FnSystem("SetupEntities",
        static (Commands commands, GameState gameState, IWindow window,
            AssetServer assetServer, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
        {
            commands.Spawn(Camera2D.Bundle);

            var spriteMaterial = materials.Add(new SpriteMaterial
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                Texture = assetServer.Load<ImageAsset>("breakout/breakout_sheet.png"),
                Sampler = samplers.Add(SamplerDescriptor.Nearest),
            });
            gameState.spritesheet = spriteMaterial;

            commands.Spawn(Entity.With(
                new Player(),
                new Transform2
                {
                    Position = (window.Size.X / 2f - 32f, 32f),
                    Scale = (96f, 16f),
                    Rotation = 0f,
                },
                new Paddle(),
                new Collider { Bounds = Rect.FromCenterScale(Vec2f.Zero, new Vec2f(96f, 16f)) },
                new Sprite
                {
                    Material = spriteMaterial,
                    Slice = new Rect(16, 0, 48, 16),
                    Color = Color.WHITE,
                }
            ));
        }))
        .AddSystem(CoreStage.First, FnSystem("GameState",
        static (World world, GameState gameState, EventWriter<Event.RestartGame> eRestartGame,
                EventWriter<Event.SpawnBall> eSpawnBall, ButtonInput<Key> keys,
                Query<Brick> qBricks
        ) =>
        {
            if (gameState.State == State.NewGame)
            {
                gameState.Score = 0;
                gameState.Lives = 3;
                gameState.State = State.SpawnBall;
                eRestartGame.Write(new Event.RestartGame());
            }
            else if (gameState.State == State.SpawnBall)
            {
                eSpawnBall.Write(new Event.SpawnBall { Count = 1 });
                gameState.State = State.Play;
            }
            else if (gameState.State == State.Play)
            {
                if (qBricks.EntityCount() == 0) gameState.State = State.Won;
            }
            else if (gameState.State == State.GameOver)
            {
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(350, 250), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("Game Over", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings))
                {
                    ImGui.TextUnformatted("Game Over");
                    ImGui.TextUnformatted($"Final Score: {gameState.Score}");
                    if (ImGui.Button("Restart"))
                    {
                        gameState.State = State.NewGame;
                    }

                    ImGui.End();
                }
            }
            else if (gameState.State == State.Won)
            {
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(350, 250), ImGuiCond.FirstUseEver);
                if (ImGui.Begin("You Won!", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings))
                {
                    ImGui.TextUnformatted("You Won!");
                    ImGui.TextUnformatted($"Final Score: {gameState.Score}");
                    if (ImGui.Button("Restart"))
                    {
                        gameState.State = State.NewGame;
                    }

                    ImGui.End();
                }
            }
        }))
        .AddSystem(CoreStage.First, FnSystem("BrickSpawner",
        static (Commands commands, IWindow window, GameState gameState,
                EventReader<Event.RestartGame> eRestartGame,
                Query<Transform2, Brick, Collider> qBricks
        ) =>
        {
            if (!eRestartGame.HasAny) return;
            eRestartGame.Consume();

            qBricks.ForEach(delegate (in Entity brickEntity, ref Transform2 brickTransform, ref Brick brick, ref Collider brickCollider)
            {
                commands.Despawn(brickEntity);
            });

            // spawn bricks that fits the window size
            var spacing = 8f;
            var bricksPerRow = 10;
            var width = (window.Size.X - spacing * (bricksPerRow + 1)) / bricksPerRow;
            var height = width * 0.33f;
            for (int x = 0; x < bricksPerRow; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    commands.Spawn(Entity.With(
                        new Brick(),
                        new Transform2
                        {
                            Position = new Vec2f(x * (width + spacing) + width * 0.5f + spacing,
                                        window.Size.Y - 32f - y * (height + spacing) - height * 0.5f),
                            Scale = new Vec2f(width, height),
                            Rotation = 0f,
                        },
                        new Collider { Bounds = Rect.FromCenterScale(Vec2f.Zero, new Vec2f(width, height)) },
                        new Sprite
                        {
                            Material = gameState.spritesheet,
                            Slice = new Rect(16, (x + y) % 3 * 16, 48, 16),
                            Color = Color.WHITE,
                        }
                    ));
                }
            }
        }))
        .AddSystem(CoreStage.First, FnSystem("BallSpawner",
        static (Commands commands, GameState gameState, IWindow window, EventReader<Event.SpawnBall> eSpawnBall) =>
        {
            if (!eSpawnBall.HasAny) return;
            var spawns = eSpawnBall.Read();

            for (int i = 0; i < spawns[0].Count; i++)
                commands.Spawn(Entity.With(
                    new Ball(),
                    new Velocity { Value = 400f * new Vec2f(((float)Random.Shared.NextDouble() * 2f - 1f).Wrap(-0.25f, 0.25f), (float)Random.Shared.NextDouble()).Normalized() },
                    new Transform2
                    {
                        Position = (window.Size.X / 2f, 128f),
                        Scale = (16f, 16f),
                        Rotation = 0f,
                    },
                    new Sprite
                    {
                        Material = gameState.spritesheet,
                        Slice = new Rect(0, 0, 16, 16),
                        Color = Color.WHITE,
                    },
                    new Collider { Bounds = Rect.FromCenterScale(Vec2f.Zero, new Vec2f(16f, 16f)) }
                ));
        }))
        .AddSystem(CoreStage.Update, FnSystem("UI",
        static (GameState gameState) =>
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(100, 50), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(5, 5), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Breakout Game", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings))
            {
                ImGui.TextUnformatted($"Score: {gameState.Score}".AsSpan());
                ImGui.TextUnformatted($"Lives: {gameState.Lives}".AsSpan());

                ImGui.End();
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (Commands commands, ButtonInput<Key> keys, AxisInput<GamepadAxis> gAxis,
                Time time, IWindow window, Query query,
                Query<Transform2, Collider>.Filter<All<Player>> qPlayer
        ) =>
        {
            var keyboardInput = keys?.GetAxis(Key.ArrowLeft, Key.ArrowRight) ?? 0;
            var gamepadInput = gAxis?.GetAxis(GamepadAxis.LeftX) ?? 0;
            var paddleInput = (Math.Abs(gamepadInput) > Math.Abs(keyboardInput)) switch
            {
                true => gamepadInput,
                false => keyboardInput,
            };

            var movePaddle = Vec2f.Right * paddleInput;

            var windowRect = new Rect(Vec2f.Zero, (window.Size.X, window.Size.Y));

            if (qPlayer.EntityCount() == 0)
                return;

            var player = qPlayer.Single();
            ref var pTransform = ref player.Component0;
            ref var pCollider = ref player.Component1;

            pTransform.Position += movePaddle * 600f * (float)time.DeltaTime;
            pTransform.Position = pTransform.Position.Clamp(
                windowRect.Min - pCollider.Bounds.Min,
                windowRect.Max - pCollider.Bounds.Max);

            if (movePaddle.Length() > 0) query.SetChanged<Transform2>(player.Entity);
        }))
        .AddSystem(CoreStage.Update, FnSystem("CollisionSystem",
        static (Commands commands, Time time, IWindow window, AssetServer assetServer,
                EventWriter<Event.Collision> eCollision,
                Query<Transform2, Ball, Velocity, Collider> qBalls,
                Query<Transform2, Collider> qColliders
        ) =>
        {
            qBalls.ForEach(delegate (in Entity _ballEntity, ref Transform2 ballTransform, ref Ball ball, ref Velocity velocity, ref Collider ballCollider)
            {
                var nextPos = ballTransform.Position + velocity.Value * (float)time.DeltaTime;

                var ballBounds = ballCollider.Bounds.Move(nextPos);
                var ballEntity = _ballEntity;

                qColliders.ForEach(delegate (in Entity colliderEntity, ref Transform2 colliderTransform, ref Collider collider)
                {
                    if (colliderEntity == ballEntity) return;

                    var colliderBounds = collider.Bounds.Move(colliderTransform.Position);
                    if (ballBounds.Intersects(colliderBounds))
                    {
                        eCollision.Write(new()
                        {
                            EntityA = colliderEntity,
                            EntityB = ballEntity,
                            Normal = ballBounds.IntersectionNormal(colliderBounds),
                        });
                    }
                });

                if (nextPos.X < 8f || nextPos.X > window.Size.X - 8f)
                {
                    eCollision.Write(new()
                    {
                        EntityA = Entity.NULL,
                        EntityB = ballEntity,
                        Normal = nextPos.X < 8f ? Vec2f.Left : Vec2f.Right,
                    });
                }
                if (nextPos.Y < 8f || nextPos.Y > window.Size.Y - 8f)
                {
                    eCollision.Write(new()
                    {
                        EntityA = Entity.NULL,
                        EntityB = ballEntity,
                        Normal = nextPos.Y < 8f ? Vec2f.Down : Vec2f.Up,
                    });
                }
            });
        }))
        .AddSystem(CoreStage.Update, FnSystem("CollisionResponseSystem",
        static (Commands commands, EventReader<Event.Collision> eCollision,
                EventWriter<Event.BrickDestroyed> eBrickDestroyed,
                Query query, Query<Velocity> qVelocities
        ) =>
        {
            qVelocities.ForEach((in Entity entity, ref Velocity velocity) =>
            {
                var reflect = Vec2f.Zero;
                foreach (var coll in eCollision.Peek())
                {
                    if (coll.EntityA == entity || coll.EntityB == entity)
                    {
                        reflect += coll.Normal;
                    }
                }

                if (reflect.Length() > 0)
                {
                    velocity.Value = velocity.Value.Reflect(reflect.Normalized());
                }
            });

            foreach (var coll in eCollision.Read())
            {
                if (coll.EntityA != Entity.NULL && query.Has<Brick>(coll.EntityA))
                {
                    commands.Despawn(coll.EntityA);
                    eBrickDestroyed.Write(new Event.BrickDestroyed());
                }
                if (coll.EntityB != Entity.NULL && query.Has<Brick>(coll.EntityB))
                {
                    commands.Despawn(coll.EntityB);
                    eBrickDestroyed.Write(new Event.BrickDestroyed());
                }
            }
        }).After("CollisionSystem"))
        .AddSystem(CoreStage.Update, FnSystem("VelocitySystem",
        static (Query<Transform2, Velocity> qTransforms, Time time) =>
        {
            qTransforms.ForEach(delegate (ref Transform2 transform, ref Velocity velocity)
            {
                transform.Position += velocity.Value * (float)time.DeltaTime;
            });
        }).After("CollisionResponseSystem"))
        .AddSystem(CoreStage.Update, FnSystem("BallOutOfBoundsSystem",
        static (Commands commands, GameState gameState, EventWriter<Event.BrickDestroyed> eBrickDestroyed,
                Query<Transform2, Ball, Collider> qBalls, IWindow window
        ) =>
        {
            qBalls.ForEach(delegate (in Entity ballEntity, ref Transform2 ballTransform, ref Ball ball, ref Collider ballCollider)
            {
                if (ballTransform.Position.Y > 16f) return;
                commands.Despawn(ballEntity);
                gameState.State = --gameState.Lives switch
                {
                    0 => State.GameOver,
                    _ => State.SpawnBall,
                };
            });
        }).After("VelocitySystem"))
        .AddSystem(CoreStage.Last, FnSystem("BrickEventsSystem",
        static (Commands commands, GameState gameState, AssetServer assetServer,
                EventReader<Event.BrickDestroyed> eBrickDestroyed,
                EventReader<Event.Collision> eCollision
        ) =>
        {
            if (eBrickDestroyed.HasAny)
            {
                eBrickDestroyed.Consume();
                gameState.Score += eBrickDestroyed.Count * 100;
            }

            if (eCollision.HasAny)
            {
                eCollision.Consume();
                commands.Spawn(Entity.With(
                    new MainMixer(),
                    new AudioSource
                    {
                        Gain = (float)Random.Shared.NextDouble().Wrap(0.8, 1),
                        Pitch = (float)Random.Shared.NextDouble().Wrap(0.8, 1),
                        Mode = PlaybackMode.Once
                    },
                    new AudioPlayback
                    {
                        Asset = assetServer.Load<AudioAsset>("sounds/bounce.wav")
                    }
                ));
            }
        }))
        .Run();
}