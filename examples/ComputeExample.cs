namespace Pollus.Examples;

using System.Reflection.Metadata;
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

    class ComputeData
    {
        public Handle<ComputeShader> Compute;
        public Handle<ParticleMaterial> ParticleMaterial = Handle<ParticleMaterial>.Null;
    }

    class ParticleMaterial : IMaterial
    {
        public static string Name => "particle";

        public static VertexBufferLayout[] VertexLayouts => [
            VertexBufferLayout.Instance(0, [
                VertexFormat.Float32x2,
                VertexFormat.Float32x2,
            ])
        ];
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
            Color = new()
            {
                SrcFactor = BlendFactor.SrcAlpha,
                DstFactor = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add,
            },
            Alpha = new()
            {
                SrcFactor = BlendFactor.One,
                DstFactor = BlendFactor.Zero,
                Operation = BlendOperation.Add,
            },
        };

        public required Handle<ShaderAsset> ShaderSource { get; set; }
        public required BufferBinding<Particle> ParticleBuffer { get; set; }
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
            ])
            .AddResource(new ComputeData())
            .AddSystem(CoreStage.Init, SystemBuilder.FnSystem("Setup",
            static (Commands commands, Random random, IWindow window,
                    ComputeData computeData, RenderAssets renderAssets, AssetServer assetServer,
                    Assets<ComputeShader> computeShaders, Assets<ParticleMaterial> particleMaterials,
                    Assets<Buffer> particleBuffers) =>
            {
                commands.Spawn(Camera2D.Bundle);

                var particleBuffer = Buffer.From<Particle>(1_000_000);
                var particleBufferHandle = particleBuffers.Add(particleBuffer);
                for (int i = 0; i < particleBuffer.Capacity; i++)
                {
                    particleBuffer.Write<Particle>(i, new Particle()
                    {
                        Position = new Vec2f(random.NextFloat() * window.Size.X, random.NextFloat() * window.Size.Y),
                        Velocity = random.NextVec2f().Normalized(),
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
                    Bindings = [
                        [
                            new BufferBinding<Particle>()
                            {
                                Visibility = ShaderStage.Compute,
                                BufferType = BufferBindingType.Storage,
                                Buffer = particleBufferHandle,
                            }
                        ],
                    ]
                });
            }))
            .AddSystem(CoreStage.Render, SystemBuilder.FnSystem("Compute",
            static (Local<bool> initialized,
                    RenderContext renderContext, RenderAssets renderAssets, AssetServer assetServer,
                    Random random, IWindow window, ComputeData computeData) =>
            {
                if (!renderAssets.Has(computeData.Compute)) return;

                var compute = renderAssets.Get<ComputeRenderData>(computeData.Compute);
                var pipeline = renderAssets.Get<GPUComputePipeline>(compute.Pipeline);

                var particleRenderMaterial = renderAssets.Get<MaterialRenderData>(computeData.ParticleMaterial);
                var particleMaterial = assetServer.GetAssets<ParticleMaterial>().Get(computeData.ParticleMaterial);
                var particleHostBuffer = assetServer.GetAssets<Buffer>().Get(particleMaterial!.ParticleBuffer.Buffer);
                var particleBufferData = renderAssets.Get<BufferRenderData>(particleMaterial!.ParticleBuffer.Buffer);
                var particleBuffer = renderAssets.Get<GPUBuffer>(particleBufferData.Buffer);

                if (!initialized.Value)
                {
                    particleHostBuffer!.WriteTo(particleBuffer, 0);
                    initialized.Value = true;
                }

                var commandEncoder = renderContext.GetCurrentCommandEncoder();
                {
                    using var computeEncoder = commandEncoder.BeginComputePass("compute");
                    computeEncoder.SetPipeline(pipeline);
                    for (int i = 0; i < compute.BindGroups.Length; i++)
                    {
                        computeEncoder.SetBindGroup((uint)i, renderAssets.Get<GPUBindGroup>(compute.BindGroups[i]));
                    }
                    computeEncoder.Dispatch((uint)MathF.Ceiling(1_000_000 / 256f), 1, 1);
                }
                {
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
                        renderEncoder.SetBindGroup((uint)i, renderAssets.Get<GPUBindGroup>(particleRenderMaterial.BindGroups[i]));
                    }
                    renderEncoder.SetVertexBuffer(0, particleBuffer, 0);
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
