namespace Pollus.Examples;

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
        })
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new ImguiPlugin(),
            new InputPlugin(),
            new AudioPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddResource(new GameState { State = State.SpawnBall, Lives = 3, Score = 0 })
        .AddSystem(CoreStage.PostInit, FnSystem("SetupEntities",
        static (World world, GameState gameState, IWindow window, AssetServer assetServer, PrimitiveMeshes primitives, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
        {
            world.Spawn(Camera2D.Bundle);

            var spriteMaterial = materials.Add(new SpriteMaterial
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/sprite.wgsl"),
                Texture = assetServer.Load<ImageAsset>("breakout/breakout_sheet.png"),
                Sampler = samplers.Add(SamplerDescriptor.Nearest),
            });
            gameState.spritesheet = spriteMaterial;

            world.Spawn(
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
            );

            var spacing = 8f;
            for (int x = 0; x < window.Size.X / (48f + spacing + 2f); x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    world.Spawn(
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
                    );
                }
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("TestImgui",
        static () =>
        {
            ImGuiNET.ImGui.Begin("Test Window");
            // ImGuiNET.ImGui.Text("Hello, world!");
            ImGuiNET.ImGui.End();
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (InputManager input, Time time, IWindow window, Query<Transform2, Collider>.Filter<All<Player>> qPlayer) =>
        {
            var keyboard = input.GetDevice("keyboard") as Keyboard;
            var movePaddle = Vec2f.Right * keyboard!.GetAxis(Key.ArrowLeft, Key.ArrowRight);

            var windowRect = new Rect(Vec2f.Zero, (window.Size.X, window.Size.Y));

            qPlayer.ForEach((ref Transform2 transform, ref Collider collider) =>
            {
                transform.Position += movePaddle * 1000f * (float)time.DeltaTime;
                transform.Position = transform.Position.Clamp(windowRect.Min - collider.Bounds.Min, windowRect.Max - collider.Bounds.Max);
            });
        }))
        .AddSystem(CoreStage.Update, FnSystem("BallUpdate",
        static (World world, Time time, IWindow window, AssetServer assetServer,
            Query<Transform2, Ball, Collider> qBall,
            Query<Transform2, Collider>.Filter<None<Ball>> qColliders,
            Query<AudioSource>.Filter<All<MainMixer>> qAudioSources
        ) =>
        {
            var collisions = new List<Entity>();

            qBall.ForEach((ref Transform2 ballTransform, ref Ball ball, ref Collider ballCollider) =>
            {
                var nextPos = ballTransform.Position + ball.Velocity * ball.Speed * (float)time.DeltaTime;

                var ballBounds = ballCollider.Bounds.Move(nextPos);
                Vec2f? collisionNormal = null;
                qColliders.ForEach((in Entity entity, ref Transform2 colliderTransform, ref Collider collider) =>
                {
                    var colliderBounds = collider.Bounds.Move(colliderTransform.Position);
                    if (ballBounds.Intersects(colliderBounds))
                    {
                        collisionNormal = ballBounds.IntersectionNormal(colliderBounds);
                        collisions.Add(entity);
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

                    world.Spawn(
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
                    );
                }

                ball.Velocity = ball.Velocity.Clamp(-Vec2f.One, Vec2f.One);
                ballTransform.Position += ball.Velocity * ball.Speed * (float)time.DeltaTime;
            });

            foreach (var entity in collisions)
            {
                world.Despawn(entity);
            }
        }))
        .AddSystem(CoreStage.First, FnSystem("GameState",
        static (World world, GameState gameState) =>
        {
            if (gameState.State == State.SpawnBall)
            {
                world.Spawn(
                    new Ball { Speed = 800f, Velocity = new Vec2f(((float)Random.Shared.NextDouble() * 2f - 1f).Wrap(-0.5f, 0.5f), (float)Random.Shared.NextDouble()).Normalized() },
                    // new Ball { Speed = 800f, Velocity = new Vec2f(1f, 0f).Normalized() },
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