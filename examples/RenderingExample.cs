namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Input;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;
using static Pollus.ECS.SystemBuilder;

public class RenderingExample
{
    struct RotateMe : IComponent
    {
        public float Speed;
    }

    struct RotateMeForEach : IForEach<Transform2, RotateMe>
    {
        public required float SecondsSinceStartup;

        public void Execute(ref Transform2 transform, ref RotateMe rotateMe)
        {
            transform.Rotation = (SecondsSinceStartup * rotateMe.Speed).Wrap(0f, 360f);
        }
    }

    public void Run() => Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem("SetupEntities",
        static (World world, AssetServer assetServer, PrimitiveMeshes primitives, Assets<Material> materials, Assets<SamplerAsset> samplers) =>
        {
            Handle[] materialHandles = [
                materials.Add(new Material()
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/quad.wgsl"),
                    Texture = assetServer.Load<ImageAsset>("breakout/ball_1.png"),
                    Sampler = samplers.Add(SamplerDescriptor.Nearest),
                }),
                materials.Add(new Material()
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/quad.wgsl"),
                    Texture = assetServer.Load<ImageAsset>("breakout/ball_2.png"),
                    Sampler = samplers.Add(SamplerDescriptor.Nearest),
                }),
                materials.Add(new Material()
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/quad.wgsl"),
                    Texture = assetServer.Load<ImageAsset>("breakout/ball_3.png"),
                    Sampler = samplers.Add(SamplerDescriptor.Nearest),
                }),
                materials.Add(new Material()
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/quad.wgsl"),
                    Texture = assetServer.Load<ImageAsset>("breakout/ball_4.png"),
                    Sampler = samplers.Add(SamplerDescriptor.Nearest),
                }),
            ];

            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    world.Spawn(
                        new Transform2
                        {
                            Position = (x * 16f, y * 16f),
                            Scale = (16f, 16f),
                            Rotation = 0f,
                        },
                        new MeshDraw<Material>
                        {
                            Mesh = primitives.Quad,
                            Material = materialHandles[(x + y) % materialHandles.Length],
                        },
                        new RotateMe { Speed = (x * y * 5).Wrap(45, 720) }
                    );
                }

            world.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (InputManager input, Time time,
            Query<Transform2, OrthographicProjection>.Filter<All<Camera2D>> qCamera,
            Query<Transform2, RotateMe> qRotateMe) =>
        {
            var keyboard = input.GetDevice("keyboard") as Keyboard;
            var inputVec = keyboard!.GetAxis2D(Key.ArrowLeft, Key.ArrowRight, Key.ArrowUp, Key.ArrowDown);
            var controlHeld = keyboard.Pressed(Key.LeftControl);
            var zoomIn = keyboard.JustPressed(Key.ArrowUp);
            var zoomOut = keyboard.JustPressed(Key.ArrowDown);

            qCamera.ForEach((ref Transform2 transform, ref OrthographicProjection projection) =>
            {
                if (controlHeld)
                {
                    if (zoomIn || zoomOut)
                    {
                        projection.Scale += inputVec.Y * 0.25f;
                        projection.Scale = projection.Scale.Clamp(0.25f, 10f);
                    }
                }
                else
                {
                    transform.Position += inputVec * 400f * (float)time.DeltaTime;
                }
            });

            qRotateMe.ForEach(new RotateMeForEach { SecondsSinceStartup = (float)time.SecondsSinceStartup });
        }))
        .Run();
}