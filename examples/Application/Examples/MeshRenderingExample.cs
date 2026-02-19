namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public partial class MeshRenderingExample : IExample
{
    public string Name => "mesh-rendering";

    partial struct RotateMe : IComponent
    {
        public float Speed;
    }

    struct RotateMeForEach : IForEach<Transform2D, RotateMe>
    {
        public required float SecondsSinceStartup;

        public void Execute(scoped in Entity entity, scoped ref Transform2D transform, scoped ref RotateMe rotateMe)
        {
            transform.Rotation = (SecondsSinceStartup * rotateMe.Speed).Wrap(0f, 360f);
        }
    }

    IApplication? app;

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new MeshDrawPlugin<Material>(),
            new InputPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystems(CoreStage.PostInit, FnSystem.Create("SetupEntities",
            static (Commands commands, AssetServer assetServer, PrimitiveMeshes primitives, Assets<Material> materials, Assets<SamplerAsset> samplers) =>
            {
                Handle[] materialHandles =
                [
                    materials.Add(new Material()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/quad.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/ball_1.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    }),
                    materials.Add(new Material()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/quad.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/ball_2.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    }),
                    materials.Add(new Material()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/quad.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/ball_3.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    }),
                    materials.Add(new Material()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/quad.wgsl"),
                        Texture = assetServer.LoadAsync<Texture2D>("breakout/ball_4.png"),
                        Sampler = samplers.Add(SamplerDescriptor.Nearest),
                    }),
                ];

                for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    commands.Spawn(Entity.With(
                        new Transform2D
                        {
                            Position = (x * 24f + 128f, y * 24f + 128f),
                            Scale = (16f, 16f),
                            Rotation = 0f,
                        },
                        GlobalTransform.Default,
                        new MeshDraw<Material>
                        {
                            Mesh = primitives.Quad,
                            Material = materialHandles[(x + y) % materialHandles.Length],
                        },
                        new RotateMe { Speed = (x * y * 5).Wrap(45, 720) }
                    ));
                }

                commands.Spawn(Camera2D.Bundle);
            }))
        .AddSystems(CoreStage.Update, FnSystem.Create("PlayerUpdate",
            static (InputManager input, Time time,
                Query<Transform2D, OrthographicProjection>.Filter<All<Camera2D>> qCamera,
                Query<Transform2D, RotateMe> qRotateMe) =>
            {
                var keyboard = input.GetDevice("keyboard") as Keyboard;
                var inputVec = keyboard!.GetAxis2D(Key.ArrowLeft, Key.ArrowRight, Key.ArrowUp, Key.ArrowDown);
                var controlHeld = keyboard.Pressed(Key.LeftControl);
                var zoomIn = keyboard.JustPressed(Key.ArrowUp);
                var zoomOut = keyboard.JustPressed(Key.ArrowDown);

                foreach (var camera in qCamera)
                {
                    ref var transform = ref camera.Component0;
                    ref var projection = ref camera.Component1;

                    if (controlHeld)
                    {
                        if (zoomIn || zoomOut)
                        {
                            projection.Scale += inputVec.Y * -0.25f;
                            projection.Scale = projection.Scale.Clamp(0.25f, 10f);
                        }
                    }
                    else
                    {
                        transform.Position += inputVec * -400f * (float)time.DeltaTime;
                    }
                }

                qRotateMe.ForEach(new RotateMeForEach { SecondsSinceStartup = (float)time.SecondsSinceStartup });
            })).Build()).Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}