namespace Pollus.Game;

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
using static Pollus.ECS.SystemBuilder;

public class SnakeGame
{
    struct Player : IComponent { }

    public void Run() => Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
            new PerformanceTrackerPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem("SetupEntities",
        static (World world, AssetServer assetServer, PrimitiveMeshes primitives, Assets<Material> materials, Assets<ShaderAsset> shaders, Assets<SamplerAsset> samplers) =>
        {
            var materialHandle = materials.Add(new Material()
            {
                ShaderSource = assetServer.Load<ShaderAsset>("shaders/quad.wgsl"),
                Texture = assetServer.Load<ImageAsset>("snake/snake_head.png"),
                Sampler = samplers.Add(SamplerDescriptor.Nearest),
            });

            world.Spawn(
                new Player(),
                new Transform2
                {
                    Position = (128f, 128f),
                    Scale = (16f, 16f),
                    Rotation = 0f,
                },
                new Renderable<Material>
                {
                    Mesh = primitives.Quad,
                    Material = materialHandle,
                }
            );

            world.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (InputManager input, Time time,
            Query<Transform2, OrthographicProjection>.Filter<All<Camera2D>> qCamera, Query<Transform2>.Filter<All<Player>> qPlayer
        ) =>
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

            qPlayer.ForEach((ref Transform2 transform) =>
            {
                transform.Rotation = (float)(time.SecondsSinceStartup * 360f).Wrap(0f, 360f);
            });
        }))
        .Run();
}