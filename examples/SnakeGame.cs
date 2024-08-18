namespace Pollus.Game;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Input;
using Pollus.Engine.Platform;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Pollus.ECS.SystemBuilder;

struct Player : IComponent { }

class SnakeRenderData
{
    public GPUBindGroupLayout? bindGroupLayout0 = null;
    public GPUBindGroup? bindGroup0 = null;
    public GPURenderPipeline? quadRenderPipeline = null;

    public GPUBuffer? sceneUniformBuffer = null;

    public GPUBuffer? quadVertexBuffer = null;
    public GPUBuffer? quadIndexBuffer = null;

    public GPUBuffer? instanceBuffer = null;
    public VertexData instanceData = VertexData.From(1024, [VertexFormat.Mat4x4]);

    public GPUTexture? texture = null;
    public GPUTextureView? textureView = null;
    public GPUSampler? textureSampler = null;
}

public class SnakeGame
{
    ~SnakeGame()
    {

    }

    public void Run() => Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
        ])
        .InitResource<SnakeRenderData>()
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
                    Position = Vec2f.Zero,
                    Scale = Vec2f.One * 16f,
                    Rotation = 0,
                },
                new Renderable
                {
                    Mesh = primitives.Quad,
                    Material = materialHandle,
                }
            );

            for (int x = 0; x < 1600 / 16; x++)
                for (int y = 0; y < 900 / 16; y++)
                {
                    world.Spawn(
                        new Transform2
                        {
                            Position = new Vec2f(x, y) * 16f + Vec2f.One * 16f,
                            Scale = Vec2f.One * 16f,
                            Rotation = 0,
                        },
                        new Renderable
                        {
                            Mesh = primitives.Quad,
                            Material = materialHandle,
                        }
                    );
                }

            world.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (InputManager input, Query<Transform2>.Filter<All<Player>> qPlayer) =>
        {
            var keyboard = input.GetDevice("keyboard") as Keyboard;
            var inputVec = keyboard!.GetAxis2D(Key.ArrowLeft, Key.ArrowRight, Key.ArrowUp, Key.ArrowDown);

            qPlayer.ForEach((ref Transform2 transform) =>
            {
                transform.Position += inputVec;
            });
        }))
        .Run();
}