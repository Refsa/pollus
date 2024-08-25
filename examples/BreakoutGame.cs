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

    struct Ball : IComponent
    {
        public required float Speed;
        public required Vec2f Velocity;
    }

    struct Collider : IComponent
    {
        public required Rect Bounds;
    }

    struct MainMixer : IComponent
    {

    }

    enum State
    {
        SpawnBall,
        Play,
        GameOver,
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
            Width = 1600,
            Height = 900,
        })
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
            new ImguiPlugin(),
            new AudioPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddResource(new GameState { State = State.SpawnBall, Lives = 3, Score = 0 })
        .AddSystem(CoreStage.PostInit, FnSystem("SetupEntities",
        static (Commands commands, GameState gameState, IWindow window,
            AssetServer assetServer, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
        {
            commands.Spawn(Camera2D.Bundle);

            var spriteMaterial = materials.Add(new SpriteMaterial
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/sprite.wgsl"),
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

            var spacing = 8f;
            for (int x = 0; x < window.Size.X / (48f + spacing + 2f); x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    commands.Spawn(Entity.With(
                        new Brick(),
                        new Transform2
                        {
                            Position = (x * (48f + spacing) + 32f, window.Size.Y - 32f - y * (16f + spacing)),
                            Scale = (48f, 16f),
                            Rotation = 0f,
                        },
                        new Collider { Bounds = Rect.FromCenterScale(Vec2f.Zero, new Vec2f(48f, 16f)) },
                        new Sprite
                        {
                            Material = spriteMaterial,
                            Slice = new Rect(16, (x + y) % 3 * 16, 48, 16),
                            Color = Color.WHITE,
                        }
                    ));
                }
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("TestImgui",
        static () =>
        {
            ImGui.ShowDemoWindow();
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (Commands commands, ButtonInput<Key> keys, AxisInput<GamepadAxis> gAxis, Time time, IWindow window, Query query, Query<Transform2, Collider>.Filter<All<Player>> qPlayer) =>
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

            if (keys?.JustPressed(Key.KeyZ) is true)
            {
                commands.AddComponent<Disabled>(player.Entity, default);
            }
            else if (keys?.JustPressed(Key.KeyX) is true)
            {
                commands.RemoveComponent<Disabled>(player.Entity);
            }
            else if (!query.Has<Disabled>(player.Entity))
            {
                pTransform.Position += movePaddle * 1000f * (float)time.DeltaTime;
                pTransform.Position = pTransform.Position.Clamp(
                    windowRect.Min - pCollider.Bounds.Min,
                    windowRect.Max - pCollider.Bounds.Max);
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("BallUpdate",
        static (Commands commands, Time time, IWindow window, AssetServer assetServer,
            Query<Transform2, Ball, Collider> qBall,
            Query<Transform2, Collider>.Filter<All<Brick>> qBricks,
            Query<Transform2, Collider>.Filter<All<Paddle>> qPaddles
        ) =>
        {
            var collisions = new List<Entity>();
            bool spawnSound = false;

            qBall.ForEach((ref Transform2 ballTransform, ref Ball ball, ref Collider ballCollider) =>
            {
                var nextPos = ballTransform.Position + ball.Velocity * ball.Speed * (float)time.DeltaTime;

                var ballBounds = ballCollider.Bounds.Move(nextPos);
                Vec2f? collisionNormal = null;
                qBricks.ForEach((in Entity entity, ref Transform2 colliderTransform, ref Collider collider) =>
                {
                    var colliderBounds = collider.Bounds.Move(colliderTransform.Position);
                    if (ballBounds.Intersects(colliderBounds))
                    {
                        collisionNormal = ballBounds.IntersectionNormal(colliderBounds);
                        collisions.Add(entity);
                    }
                });
                qPaddles.ForEach((ref Transform2 colliderTransform, ref Collider collider) =>
                {
                    var colliderBounds = collider.Bounds.Move(colliderTransform.Position);
                    if (ballBounds.Intersects(colliderBounds))
                    {
                        collisionNormal = ballBounds.IntersectionNormal(colliderBounds);
                    }
                });

                if (nextPos.X < 8f || nextPos.X > window.Size.X - 8f)
                {
                    collisionNormal = nextPos.X < 8f ? Vec2f.Left : Vec2f.Right;
                }
                if (nextPos.Y < 8f || nextPos.Y > window.Size.Y - 8f)
                {
                    collisionNormal = nextPos.Y < 8f ? Vec2f.Down : Vec2f.Up;
                }

                if (collisionNormal.HasValue)
                {
                    var normal = collisionNormal.Value.Normalized();
                    ballTransform.Position -= normal * 2;
                    ball.Velocity = ball.Velocity.Reflect(normal);

                    spawnSound = true;
                }

                ball.Velocity = ball.Velocity.Clamp(-Vec2f.One, Vec2f.One);
                ballTransform.Position += ball.Velocity * ball.Speed * (float)time.DeltaTime;
            });

            foreach (var entity in collisions)
            {
                commands.Despawn(entity);
            }

            if (spawnSound)
            {
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
        .AddSystem(CoreStage.First, FnSystem("GameState",
        static (World world, GameState gameState) =>
        {
            if (gameState.State == State.SpawnBall)
            {
                for (int i = 0; i < 1; i++)
                    world.Spawn(
                        new Ball { Speed = 800f, Velocity = new Vec2f(((float)Random.Shared.NextDouble() * 2f - 1f).Wrap(-0.5f, 0.5f), (float)Random.Shared.NextDouble()).Normalized() },
                        new Transform2
                        {
                            Position = (world.Resources.Get<IWindow>().Size.X / 2f, 128f),
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
                    );

                gameState.State = State.Play;
            }
            else if (gameState.State == State.Play)
            {

            }
        }))
        .Run();
}