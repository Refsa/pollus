namespace Pollus.Examples;

using Pollus.Debugging;
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

        public static RenderPipelineDescriptor PipelineDescriptor => RenderPipelineDescriptor.Default with
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

        public IBinding[][] Bindings =>
        [
            [
                new UniformBinding<SceneUniform>(),
                ParticleBuffer
            ]
        ];
    }

    struct ComputePassData
    {
        public ResourceHandle<BufferResource> ParticleBuffer;
    }

    struct ParticlePassData
    {
        public ResourceHandle<TextureResource> ColorAttachment;
        public ResourceHandle<BufferResource> ParticleBuffer;
    }

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new AssetPlugin() { RootPath = "assets" },
                new RenderingPlugin(),
                new MaterialPlugin<ParticleMaterial>(),
                new RandomPlugin(),
                new PerformanceTrackerPlugin(),
                new UniformPlugin<SceneData, Param<Time, IWindow>>()
                {
                    Extract = static (in param, ref sceneData) =>
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
            .AddSystem(CoreStage.Init, FnSystem.Create("Setup",
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
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/particle.wgsl"),
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
                        Shader = assetServer.LoadAsync<ShaderAsset>("shaders/compute.wgsl"),
                        Bindings =
                        [
                            [
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
                            ]
                        ]
                    });
                }))
            .AddSystem(CoreStage.PreRender, FnSystem.Create(new("PrepareRender")
                {
                    RunsAfter = [FrameGraph2DPlugin.BeginFrame],
                },
                static (FrameGraph2D frameGraph) =>
                {
                    frameGraph.FrameGraph.AddBuffer(BufferDescriptor.Storage<Particle>("particles_buffer", 1_000_000));

                    frameGraph.AddPass(RenderStep2D.Main,
                        static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref ComputePassData data) => { data.ParticleBuffer = builder.Writes<BufferResource>("particles_buffer"); },
                        static (context, in param, in data) =>
                        {
                            var computeData = param.Resources.Get<ComputeData>();
                            var compute = param.RenderAssets.Get<ComputeRenderData>(computeData.Compute);
                            using var computeEncoder = context.GetCurrentCommandEncoder().BeginComputePass("""particles_compute""");

                            ComputeCommands.Builder
                                .SetPipeline(compute.Pipeline)
                                .SetBindGroups(0, compute.BindGroups)
                                .Dispatch((uint)MathF.Ceiling(1_000_000 / 256f), 1, 1)
                                .ApplyAndDispose(computeEncoder, param.RenderAssets);
                        });

                    frameGraph.AddPass(RenderStep2D.Main,
                        static (ref FrameGraph<FrameGraph2DParam>.Builder builder, in FrameGraph2DParam param, ref ParticlePassData data) =>
                        {
                            data.ColorAttachment = builder.Writes<TextureResource>(FrameGraph2D.Textures.ColorTarget);
                            data.ParticleBuffer = builder.Reads<BufferResource>("particles_buffer");
                        },
                        static (context, in param, in data) =>
                        {
                            var computeData = param.Resources.Get<ComputeData>();
                            var particleMaterial = param.RenderAssets.Get<MaterialRenderData>(computeData.ParticleMaterial);
                            using var renderEncoder = context.GetCurrentCommandEncoder().BeginRenderPass(new()
                            {
                                Label = """particles_render""",
                                ColorAttachments =
                                [
                                    new()
                                    {
                                        View = context.Resources.GetTexture(data.ColorAttachment).TextureView.Native,
                                        LoadOp = LoadOp.Load,
                                        StoreOp = StoreOp.Store,
                                    }
                                ]
                            });

                            RenderCommands.Builder
                                .SetPipeline(particleMaterial.Pipeline)
                                .SetBindGroups(0, particleMaterial.BindGroups)
                                .Draw(4, 1_000_000, 0, 0)
                                .ApplyAndDispose(renderEncoder, param.RenderAssets);
                        });
                }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}
