namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;

public partial class ComputeExample : IExample
{
    public string Name => "compute";

    [ShaderType]
    partial struct Particle
    {
        public Vec2f Position;
        public Vec2f Velocity;
    }

    [ShaderType]
    partial struct SceneData
    {
        public float Time;
        public float DeltaTime;
        public uint Width;
        public uint Height;
    }

    class ComputeData
    {
        public Handle<ComputeShader> Compute = Handle<ComputeShader>.Null;
        public Handle<ParticleMaterial> ParticleMaterial = Handle<ParticleMaterial>.Null;
    }

    class ParticleMaterial : IMaterial
    {
        public static string Name => "particle";

        public static VertexBufferLayout[] VertexLayouts => [];
        public static RenderPipelineDescriptor PipelineDescriptor => new()
        {
            Label = "particle",
            VertexState = new()
            {
                EntryPoint = "vs_main",
                Layouts = VertexLayouts,
            },
            FragmentState = new()
            {
                EntryPoint = "fs_main",
            },
            PrimitiveState = PrimitiveState.Default with
            {
                Topology = PrimitiveTopology.TriangleStrip,
                CullMode = CullMode.None,
                FrontFace = FrontFace.CW,
            },
        };
        public static BlendState? Blend => BlendState.Default with
        {
            Alpha = new()
            {
                SrcFactor = BlendFactor.SrcAlpha,
                DstFactor = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add,
            },
        };

        public required Handle<ShaderAsset> ShaderSource { get; set; }
        public required StorageBufferBinding<Particle> ParticleBuffer { get; set; }
        public IBinding[][] Bindings => [[
            new UniformBinding<SceneUniform>(),
            ParticleBuffer
        ]];
    }

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new AssetPlugin() {RootPath = "assets"},
                new RenderingPlugin(),
                new ComputePlugin<ComputeShader>(),
                new MaterialPlugin<ParticleMaterial>(),
                new RandomPlugin(),
                new PerformanceTrackerPlugin(),
                new UniformPlugin<SceneData, Param<Time, IWindow>>()
                {
                    Extract = static (in Param<Time, IWindow> param, ref SceneData sceneData) =>
                    {
                        var (time, window) = param;
                        sceneData.Time = (float)time.SecondsSinceStartup;
                        sceneData.DeltaTime = (float)time.DeltaTime;
                        sceneData.Width = (uint)window.Size.X;
                        sceneData.Height = (uint)window.Size.Y;
                    }
                },
            ])
            .AddResource(new ComputeData())
            .AddSystem(CoreStage.Init, SystemBuilder.FnSystem("Setup",
            static (Commands commands, Random random, IWindow window,
                    ComputeData computeData, RenderAssets renderAssets, AssetServer assetServer,
                    Assets<ComputeShader> computeShaders, Assets<ParticleMaterial> particleMaterials,
                    Assets<StorageBuffer> particleBuffers) =>
            {
                commands.Spawn(Camera2D.Bundle);

                var particleBuffer = StorageBuffer.From<Particle>(1_000_000, BufferUsage.CopyDst);
                var particleBufferHandle = particleBuffers.Add(particleBuffer);
                for (int i = 0; i < particleBuffer.Capacity; i++)
                {
                    particleBuffer.Write<Particle>(i, new Particle()
                    {
                        Position = new Vec2f(random.NextFloat(0, window.Size.X), random.NextFloat(0, window.Size.Y)),
                        Velocity = random.NextVec2f().Normalized() * random.NextFloat(5f, 50f),
                    });
                }

                computeData.ParticleMaterial = particleMaterials.Add(new()
                {
                    ShaderSource = assetServer.Load<ShaderAsset>("shaders/particle.wgsl"),
                    ParticleBuffer = new()
                    {
                        Buffer = particleBufferHandle,
                        BufferType = BufferBindingType.ReadOnlyStorage,
                        Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
                    },
                });

                computeData.Compute = computeShaders.Add(new()
                {
                    Label = "compute",
                    EntryPoint = "main",
                    Shader = assetServer.Load<ShaderAsset>("shaders/compute.wgsl"),
                    Bindings = [[
                        new StorageBufferBinding<Particle>()
                        {
                            Visibility = ShaderStage.Compute,
                            BufferType = BufferBindingType.Storage,
                            Buffer = particleBufferHandle,
                        },
                        new UniformBinding<SceneData>()
                        {
                            Visibility = ShaderStage.Compute,
                        }
                    ]]
                });
            }))
            .AddSystem(CoreStage.Render, SystemBuilder.FnSystem("Compute",
            static (RenderContext renderContext, RenderAssets renderAssets, AssetServer assetServer,
                    Random random, IWindow window, ComputeData computeData) =>
            {
                if (!renderAssets.Has(computeData.Compute)) return;

                var compute = renderAssets.Get<ComputeRenderData>(computeData.Compute);
                var pipeline = renderAssets.Get(compute.Pipeline);

                var particleMaterial = assetServer.GetAssets<ParticleMaterial>().Get(computeData.ParticleMaterial)!;
                var particleHostBuffer = assetServer.GetAssets<StorageBuffer>().Get(particleMaterial.ParticleBuffer.Buffer);
                var particleBufferData = renderAssets.Get<StorageBufferRenderData>(particleMaterial.ParticleBuffer.Buffer);
                var particleBuffer = renderAssets.Get(particleBufferData.Buffer);

                var commandEncoder = renderContext.GetCurrentCommandEncoder();
                {
                    using var computeEncoder = commandEncoder.BeginComputePass("compute");
                    computeEncoder.SetPipeline(pipeline);
                    for (int i = 0; i < compute.BindGroups.Length; i++)
                    {
                        computeEncoder.SetBindGroup((uint)i, renderAssets.Get(compute.BindGroups[i]));
                    }
                    computeEncoder.Dispatch((uint)MathF.Ceiling(1_000_000 / 256f), 1, 1);
                }
                {
                    var particleRenderMaterial = renderAssets.Get<MaterialRenderData>(computeData.ParticleMaterial);
                    using var renderEncoder = commandEncoder.BeginRenderPass(new()
                    {
                        ColorAttachments = [
                            new()
                            {
                                View = renderContext.SurfaceTextureView!.Value.Native,
                                LoadOp = LoadOp.Clear,
                                StoreOp = StoreOp.Store,
                                ClearValue = new(0.1f, 0.1f, 0.1f, 1.0f),
                            }
                        ]
                    });
                    renderEncoder.SetPipeline(renderAssets.Get(particleRenderMaterial.Pipeline));
                    for (int i = 0; i < particleRenderMaterial.BindGroups.Length; i++)
                    {
                        renderEncoder.SetBindGroup((uint)i, renderAssets.Get(particleRenderMaterial.BindGroups[i]));
                    }
                    renderEncoder.Draw(4, 1_000_000, 0, 0);
                }
            }).After(FrameGraph2DPlugin.Render))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}
