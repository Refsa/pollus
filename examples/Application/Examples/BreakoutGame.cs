namespace Pollus.Examples;

using Pollus.Coroutine;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Engine.Audio;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Input;
using Pollus.Engine.Physics;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Mathematics.Collision2D;
using Pollus.Collections;
using Pollus.Utils;
using Pollus.UI;
using Pollus.Engine.UI;
using Pollus.UI.Layout;

public partial class BreakoutGame : IExample
{
    public string Name => "breakout";
    IApplication? application;
    public void Stop() => application?.Shutdown();

    partial struct Player : IComponent;

    partial struct Disabled : IComponent;

    partial struct Paddle : IComponent;

    partial struct Brick : IComponent;

    partial struct Ball : IComponent;

    partial struct Velocity : IComponent
    {
        public required Vec2f Value;
    }

    partial struct MainMixer : IComponent;

    class Palette
    {
        public static Color PanelBackground = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        public static Color PanelTextColor = new Color(1f, 0.6f, 0.1f, 1f);
        public static Color ButtonNormal = new Color(0.8f, 0.4f, 0f, 1f);
        public static Color ButtonHover = new Color(1f, 0.5f, 0.05f, 1f);
        public static Color ButtonPressed = new Color(0.6f, 0.3f, 0f, 1f);
        public static Color ButtonDisabled = new Color(0.3f, 0.2f, 0.1f, 0.5f);
        public static Color ButtonTextColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        public static Color BorderColor = new Color(1f, 0.5f, 0f, 1f);
        public static Color AccentColor = new Color(1f, 0.65f, 0.15f, 1f);
    }

    struct Event
    {
        public struct BrickDestroyed;

        public struct Collision
        {
            public Entity EntityA;
            public Entity EntityB;
            public Intersection2D Intersection;
        }

        public struct RestartGame;

        public struct SpawnBall
        {
            public required int Count;
        }

        public struct ScoreChanged
        {
            public required int OldScore;
            public required int NewScore;
        }

        public struct LivesChanged;
    }


    enum State
    {
        NewGame,
        SpawnBall,
        Play,
        GameEnd,
    }

    class GameState
    {
        public int Lives;
        public int Score;
        public bool Won;

        public Handle<SpriteMaterial> spritesheet;
    }

    class UIState
    {
        public Handle Font;

        public Entity Root;
        public Entity GameUI;
        public Entity GameEndUI;
        public Entity GameEndTitleText;
        public Entity GameEndScoreText;
        public Entity GameEndRestartButton;
        public Entity GameUIScoreText;
        public Entity GameUILivesText;
    }

    public void Run() => (application = Application.Builder
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
                AssetPlugin.Default,
                new RenderingPlugin(),
                new InputPlugin(),
                new AudioPlugin(),
                new PerformanceTrackerPlugin(),
                new RandomPlugin(),
                new StatePlugin<State>(State.NewGame),
                new UIPlugin(),
            ])
            .AddResource(new GameState { Lives = 3, Score = 0 })
            .AddResource(new UIState())
            .AddSystems(CoreStage.Init, FnSystem.Create("LogSchedule",
                static (World world) =>
                {
                    Log.Info(world.Schedule.ToString());
                }))
            .AddSystems(CoreStage.PostInit, FnSystem.Create("SetupEntities",
                static (Commands commands, GameState gameState, IWindow window,
                    AssetServer assetServer, Assets<SpriteMaterial> materials, Assets<SamplerAsset> samplers) =>
                {
                    commands.Spawn(Camera2D.Bundle);

                    var spriteMaterial = materials.Add(new SpriteMaterial
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/breakout_sheet.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    });
                    gameState.spritesheet = spriteMaterial;

                    var paddleSize = new Vec2f(96f, 16f);
                    commands.Spawn(Entity.With(
                        new Player(),
                        Transform2D.Default with
                        {
                            Position = new Vec2f(window.Size.X / 2f - 32f, 32f),
                            Scale = new Vec2f(2f, 1f),
                            Rotation = 0f,
                        },
                        new Paddle(),
                        CollisionShape.Rectangle(Vec2f.Zero, paddleSize * 0.5f),
                        new Sprite
                        {
                            Material = spriteMaterial,
                            Slice = new Rect(16, 0, 48, 16),
                            Color = Color.WHITE,
                        }
                    ));
                }))
            .AddSystem(CoreStage.PostInit, FnSystem.Create(new("UISetupSystem"),
            static (AssetServer assetServer, UIState state, Commands commands) =>
            {
                state.Font = assetServer.LoadAsync<FontAsset>("builtin/fonts/SmoochSans-Light.ttf");

                state.Root = UI.Root(commands, 1, 1)
                    .AutoResize()
                    .Background(Color.TRANSPARENT)
                    .Spawn();

                state.GameUI = UI.Panel(commands)
                    .ChildOf(state.Root)
                    .DisplayNone()
                    .PositionAbsolute()
                    .MinSize(Length.Px(150), Length.Auto)
                    .Size(Length.Auto, Length.Auto)
                    .AlignSelf(AlignSelf.FlexStart)
                    .Margin(32).Padding(8)
                    .FlexColumn()
                    .Background(Palette.PanelBackground.WithAlpha(0.75f))
                    .BorderColor(Palette.BorderColor).BorderRadius(8)
                    .Children([
                        state.GameUIScoreText = UI.Text(commands, "Score: 0", state.Font).FontSize(24).LineHeight(1f).Color(Palette.PanelTextColor).Spawn(),
                        state.GameUILivesText = UI.Text(commands, "Lives: 3", state.Font).FontSize(24).LineHeight(1f).Color(Palette.AccentColor).Spawn(),
                    ])
                    .Spawn();

                state.GameEndUI = UI.Panel(commands)
                    .ChildOf(state.Root)
                    .DisplayNone()
                    .PositionAbsolute()
                    .Size(Length.Px(500), Length.Auto)
                    .FlexColumn().Gap(8)
                    .AlignItems(AlignItems.Center)
                    .AlignSelf(AlignSelf.Center).JustifySelf(JustifySelf.Center)
                    .Background(Palette.PanelBackground)
                    .BorderColor(Palette.BorderColor)
                    .BorderRadius(8)
                    .Padding(8)
                    .Children([
                        state.GameEndTitleText = UI.Text(commands, "", state.Font).FontSize(32).FlexGrow(1).LineHeight(1).Color(Palette.PanelTextColor).Spawn(),
                        state.GameEndScoreText = UI.Text(commands, "", state.Font).FontSize(32).LineHeight(1).Color(Palette.AccentColor).Spawn(),
                        state.GameEndRestartButton = UI.Button(commands)
                            .Colors(Palette.ButtonNormal, Palette.ButtonHover, Palette.ButtonPressed, Palette.ButtonDisabled)
                            .Padding(32)
                            .Children([
                                UI.Text(commands, "Restart", state.Font).FontSize(32).Color(Palette.ButtonTextColor).Spawn()
                            ])
                            .Spawn()
                    ])
                    .Spawn();
            }))
            .AddSystems(CoreStage.PreUpdate, FnSystem.Create(new("NewGameStateSystem")
            {
                RunCriteria = StateRunCriteria<State>.OnEnter(State.NewGame)
            }, static (GameState gameState, State<State> state, EventWriter<Event.RestartGame> eRestartGame) =>
            {
                gameState.Score = 0;
                gameState.Lives = 3;
                eRestartGame.Write(new Event.RestartGame());
            }))
            .AddSystems(CoreStage.PreUpdate, FnSystem.Create(new("SpawnBallStateSystem")
            {
                RunCriteria = StateRunCriteria<State>.OnEnter(State.SpawnBall)
            }, static (GameState gameState, State<State> state, EventWriter<Event.SpawnBall> eSpawnBall) =>
            {
                eSpawnBall.Write(new Event.SpawnBall { Count = 1 });
                state.Set(State.Play);
            }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("PlayStateSystem")
            {
                RunCriteria = StateRunCriteria<State>.OnCurrent(State.Play)
            }, static (Query<Brick> qBricks, State<State> state, GameState gameState) =>
            {
                if (qBricks.EntityCount() == 0)
                {
                    gameState.Won = true;
                    state.Set(State.GameEnd);
                }
            }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("GameEndEnter")
            {
                RunCriteria = StateRunCriteria<State>.OnEnter(State.GameEnd)
            }, static (Commands commands, Query<Ball> qBalls, View<UIStyle, UIText> view, UIState uiState, GameState gameState) =>
            {
                foreach (var ball in qBalls)
                {
                    commands.DespawnHierarchy(ball.Entity);
                }

                ref var gameUiStyle = ref view.GetTracked<UIStyle>(uiState.GameUI);
                gameUiStyle.Value = gameUiStyle.Value with { Display = Display.None };

                ref var panelStyle = ref view.GetTracked<UIStyle>(uiState.GameEndUI);
                panelStyle.Value = panelStyle.Value with { Display = Display.Flex };

                ref var titleText = ref view.GetTracked<UIText>(uiState.GameEndTitleText);
                titleText.Text = new NativeUtf8(gameState.Won ? "You Won!" : "Game Over");

                ref var scoreText = ref view.GetTracked<UIText>(uiState.GameEndScoreText);
                scoreText.Text = new NativeUtf8($"Score: {gameState.Score}");
            }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("GameEndExit")
            {
                RunCriteria = StateRunCriteria<State>.OnExit(State.GameEnd)
            }, static (View<UIStyle> view, UIState uiState) =>
            {
                ref var panelStyle = ref view.GetTracked<UIStyle>(uiState.GameEndUI);
                panelStyle.Value = panelStyle.Value with { Display = Display.None };
            }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("GameEndUI")
            {
                RunCriteria = WhenAll.Create([
                    StateRunCriteria<State>.OnCurrent(State.GameEnd),
                    EventRunCriteria<UIInteractionEvents.UIClickEvent>.Create,
                ]),
            }, static (State<State> state, View<UIButton> buttons, UIState uiState, EventReader<UIInteractionEvents.UIClickEvent> eUiClicks) =>
            {
                foreach (var ev in eUiClicks.Read())
                {
                    if (!buttons.Has<UIButton>(ev.Entity)) return;
                    if (ev.Entity != uiState.GameEndRestartButton) return;
                    state.Set(State.NewGame);
                }
            }))
            .AddSystems(CoreStage.First, Coroutine.Create(new("BrickSpawner"),
                static (Param<Commands, IWindow, GameState, State<State>, EventReader<Event.RestartGame>, Query<Transform2D, Brick>> param) =>
                {
                    return Routine(param);

                    static IEnumerable<Yield> Routine(Param<Commands, IWindow, GameState, State<State>, EventReader<Event.RestartGame>, Query<Transform2D, Brick>> param)
                    {
                        var (commands, window, gameState, state, eRestartGame, qBricks) = param;

                        while (!eRestartGame.HasAny) yield return Yield.Return;
                        eRestartGame.Consume();

                        qBricks.ForEach(delegate (in Entity brickEntity, ref Transform2D brickTransform, ref Brick brick)
                        {
                            commands.Despawn(brickEntity);
                        });

                        // spawn bricks that fits the window size
                        var spacing = 8f;
                        var bricksPerRow = 10;
                        var width = (window.Size.X - spacing * (bricksPerRow + 1)) / bricksPerRow;
                        var height = width * 0.33f;
                        for (int y = 0; y < 8; y++)
                        {
                            for (int x = 0; x < bricksPerRow; x++)
                            {
                                commands.Spawn(Entity.With(
                                    new Brick(),
                                    Transform2D.Default with
                                    {
                                        Position = new Vec2f(x * (width + spacing) + width * 0.5f + spacing,
                                            window.Size.Y - 32f - y * (height + spacing) - height * 0.5f),
                                        Scale = new Vec2f(width / 48f, height / 16f),
                                        Rotation = 0f,
                                    },
                                    CollisionShape.Rectangle(Vec2f.Zero, new Vec2f(width, height) * 0.5f),
                                    Sprite.Default with
                                    {
                                        Material = gameState.spritesheet,
                                        Slice = Rect.FromOriginSize(new Vec2f(16f, (x + y) % 3 * 16f), new Vec2f(48f, 16f)),
                                    }
                                ));
                            }

                            yield return Yield.WaitForSeconds(0.1f);
                        }

                        state.Set(State.SpawnBall);
                    }
                }))
            .AddSystems(CoreStage.First, FnSystem.Create("BallSpawner",
                static (Commands commands, GameState gameState, IWindow window, Random random, EventReader<Event.SpawnBall> eSpawnBall) =>
                {
                    if (!eSpawnBall.HasAny) return;
                    var spawns = eSpawnBall.Read();

                    for (int i = 0; i < spawns[0].Count; i++)
                        commands.Spawn(Entity.With(
                            new Ball(),
                            new Velocity { Value = 400f * new Vec2f((random.NextFloat() * 2f - 1f).Wrap(-0.25f, 0.25f), random.NextFloat()).Normalized() },
                            Transform2D.Default with
                            {
                                Position = (window.Size.X / 2f, 128f),
                                Rotation = 0f,
                            },
                            new Sprite
                            {
                                Material = gameState.spritesheet,
                                Slice = new Rect(0, 0, 16, 16),
                                Color = Color.WHITE,
                            },
                            CollisionShape.Circle(8f)
                        ));
                }))
            .AddSystem(CoreStage.Update, FnSystem.Create(new("PlayEnterUI")
            {
                RunCriteria = StateRunCriteria<State>.OnEnter(State.Play)
            },
            static (View<UIStyle, UIText> view, UIState uiState, GameState gameState) =>
            {
                ref var panelStyle = ref view.GetTracked<UIStyle>(uiState.GameUI);
                panelStyle.Value = panelStyle.Value with { Display = Display.Flex };
            }))
            .AddSystem(CoreStage.PostUpdate, FnSystem.Create(new("PlayUpdateUI")
            {
                RunCriteria = StateRunCriteria<State>.OnCurrent(State.Play)
            },
            static (View<UIText> view, UIState uiState, GameState gameState,
                EventReader<Event.LivesChanged> eLivesChanged, EventReader<Event.ScoreChanged> eScoreChanged) =>
            {
                if (eScoreChanged.Read() is { Length: > 0 })
                {
                    ref var scoreText = ref view.GetTracked<UIText>(uiState.GameUIScoreText);
                    scoreText.Text = new NativeUtf8($"Score: {gameState.Score}");
                }

                if (eLivesChanged.Read() is { Length: > 0 })
                {
                    ref var livesText = ref view.GetTracked<UIText>(uiState.GameUILivesText);
                    livesText.Text = new NativeUtf8($"Lives: {gameState.Lives}");
                }
            }))
            .AddSystems(CoreStage.Update, FnSystem.Create("PlayerUpdate",
                static (Commands commands, ButtonInput<Key> keys, AxisInput<GamepadAxis> gAxis,
                    Time time, IWindow window, Query query,
                    Query<Transform2D, CollisionShape>.Filter<All<Player>> qPlayer
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
                    var colliderShape = pCollider.GetShape<Bounds2D>();

                    pTransform.Position += movePaddle * 600f * (float)time.DeltaTime;
                    pTransform.Position = pTransform.Position.Clamp(
                        windowRect.Min - colliderShape.Min,
                        windowRect.Max - colliderShape.Max);

                    if (movePaddle.Length() > 0) query.SetChanged<Transform2D>(player.Entity);
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create("CollisionSystem",
                static (Commands commands, Time time, IWindow window, AssetServer assetServer,
                    EventWriter<Event.Collision> eCollision,
                    Query<Transform2D, Velocity, CollisionShape>.Filter<All<Ball>> qBalls,
                    Query<Transform2D, CollisionShape> qColliders
                ) =>
                {
                    foreach (var ball in qBalls)
                    {
                        ref var ballTransform = ref ball.Component0;
                        ref var ballVelocity = ref ball.Component1;
                        ref var ballCollider = ref ball.Component2;

                        var nextPos = ballTransform.Position + ballVelocity.Value * (float)time.DeltaTime;

                        foreach (var collider in qColliders)
                        {
                            ref var colliderTransform = ref collider.Component0;
                            ref var colliderShape = ref collider.Component1;
                            if (collider.Entity == ball.Entity) continue;

                            var intersection = colliderShape.GetIntersection(colliderTransform, ballCollider, ballTransform);

                            if (intersection.IsIntersecting)
                            {
                                eCollision.Write(new()
                                {
                                    EntityA = collider.Entity,
                                    EntityB = ball.Entity,
                                    Intersection = intersection,
                                });
                            }
                        }

                        if (nextPos.X < 8f || nextPos.X > window.Size.X - 8f)
                        {
                            eCollision.Write(new()
                            {
                                EntityA = Entity.Null,
                                EntityB = ball.Entity,
                                Intersection = new Intersection2D
                                {
                                    IsIntersecting = true,
                                    Normal = nextPos.X < 8f ? Vec2f.Right : Vec2f.Left,
                                }
                            });
                        }

                        if (nextPos.Y < 8f || nextPos.Y > window.Size.Y - 8f)
                        {
                            eCollision.Write(new()
                            {
                                EntityA = Entity.Null,
                                EntityB = ball.Entity,
                                Intersection = new Intersection2D
                                {
                                    IsIntersecting = true,
                                    Normal = nextPos.Y < 8f ? Vec2f.Up : Vec2f.Down,
                                }
                            });
                        }
                    }
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("CollisionResponseSystem")
            {
                RunsAfter = ["CollisionSystem"],
            },
                static (Commands commands, EventReader<Event.Collision> eCollision,
                    EventWriter<Event.BrickDestroyed> eBrickDestroyed,
                    Query query, Query<Transform2D, Velocity, CollisionShape> qBodies
                ) =>
                {
                    var collisions = eCollision.Read();

                    foreach (var body in qBodies)
                    {
                        ref var transform = ref body.Component0;
                        ref var velocity = ref body.Component1;
                        ref var shape = ref body.Component2;

                        var reflect = Vec2f.Zero;
                        var correctedPos = transform.Position;
                        foreach (var coll in collisions)
                        {
                            if (coll.EntityA == body.Entity || coll.EntityB == body.Entity)
                            {
                                reflect += coll.Intersection.Normal;
                                correctedPos += coll.Intersection.Normal * (coll.Intersection.Distance + shape.GetBoundingCircle(transform).Radius);
                            }
                        }

                        if (reflect.Length() > 0)
                        {
                            velocity.Value = velocity.Value.Reflect(reflect.Normalized());
                            transform.Position = correctedPos;
                        }
                    }

                    foreach (var coll in collisions)
                    {
                        if (!coll.EntityA.IsNull && query.Has<Brick>(coll.EntityA))
                        {
                            commands.Despawn(coll.EntityA);
                            eBrickDestroyed.Write(new Event.BrickDestroyed());
                        }

                        if (!coll.EntityB.IsNull && query.Has<Brick>(coll.EntityB))
                        {
                            commands.Despawn(coll.EntityB);
                            eBrickDestroyed.Write(new Event.BrickDestroyed());
                        }
                    }
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("VelocitySystem")
            {
                RunsAfter = ["CollisionResponseSystem"],
            },
                static (Query<Transform2D, Velocity> qTransforms, Time time) =>
                {
                    qTransforms.ForEach(time.DeltaTimeF,
                        static (in float deltaTime, ref Transform2D transform, ref Velocity velocity) =>
                        {
                            transform.Position += velocity.Value * deltaTime;
                        });
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new("BallOutOfBoundsSystem")
            {
                RunsAfter = ["VelocitySystem"],
            },
                static (Commands commands, GameState gameState,
                    State<State> state, EventWriter<Event.BrickDestroyed> eBrickDestroyed,
                    EventWriter<Event.LivesChanged> eLivesChanged,
                    Query<Transform2D, Ball> qBalls, IWindow window
                ) =>
                {
                    foreach (var ball in qBalls)
                    {
                        ref var ballTransform = ref ball.Component0;
                        if (ballTransform.Position.Y > 16f) continue;
                        commands.Despawn(ball.Entity);
                        eLivesChanged.Write(new());
                        if (--gameState.Lives <= 0)
                        {
                            gameState.Won = false;
                            state.Set(State.GameEnd);
                        }
                        else
                        {
                            state.Set(State.SpawnBall);
                        }
                    }
                }))
            .AddSystems(CoreStage.Last, FnSystem.Create("BrickEventsSystem",
                static (Commands commands, GameState gameState,
                    AssetServer assetServer, Random random,
                    EventReader<Event.BrickDestroyed> eBrickDestroyed,
                    EventReader<Event.Collision> eCollision,
                    EventWriter<Event.ScoreChanged> eScoreChanged
                ) =>
                {
                    if (eBrickDestroyed.HasAny)
                    {
                        var oldScore = gameState.Score;
                        gameState.Score += eBrickDestroyed.Count * 100;
                        eScoreChanged.Write(new()
                        {
                            NewScore = gameState.Score,
                            OldScore = oldScore,
                        });
                        eBrickDestroyed.Consume();
                    }

                    if (eCollision.HasAny)
                    {
                        eCollision.Consume();
                        commands.Spawn(Entity.With(
                            new MainMixer(),
                            new AudioSource
                            {
                                Gain = random.NextFloat().Wrap(0.8f, 1f),
                                Pitch = random.NextFloat().Wrap(0.8f, 1f),
                                Mode = PlaybackMode.Once
                            },
                            new AudioPlayback
                            {
                                Asset = assetServer.LoadAsync<AudioAsset>("sounds/bounce.wav")
                            }
                        ));
                    }
                }))
            .Build())
        .Run();
}