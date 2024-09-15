namespace Pollus.Examples;

using System.Reflection.Metadata;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
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
        public Handle<GPUBindGroup> ComputeBindGroup = Handle<GPUBindGroup>.Null;
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
            ])
            .AddResource(new ComputeData())
            .AddSystem(CoreStage.Init, SystemBuilder.FnSystem("Setup",
            static (Commands commands, Random random, IWindow window,
                    ComputeData computeData, RenderAssets renderAssets, AssetServer assetServer,
                    Assets<ComputeShader> computeShaders, Assets<ParticleMaterial> particleMaterials,
                    Assets<Buffer> particleBuffers) =>
            {
                commands.Spawn(Camera2D.Bundle);

                var particleBuffer = Buffer.From<Particle>(1000);
                for (int i = 0; i < 1000; i++)
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
                        Buffer = particleBuffers.Add(particleBuffer),
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
                        [BindGroupLayoutEntry.BufferEntry<Particle>(0, ShaderStage.Compute, BufferBindingType.Storage)],
                    ]
                });
            }))
            .AddSystem(CoreStage.Render, SystemBuilder.FnSystem("Compute",
            static (RenderContext renderContext, RenderAssets renderAssets, AssetServer assetServer, Random random, IWindow window, ComputeData computeData) =>
            {
                if (!renderAssets.Has(computeData.Compute)) return;

                var compute = renderAssets.Get<ComputeRenderData>(computeData.Compute);
                var pipeline = renderAssets.Get<GPUComputePipeline>(compute.Pipeline);

                var particleRenderMaterial = renderAssets.Get<MaterialRenderData>(computeData.ParticleMaterial);
                var particleMaterial = assetServer.GetAssets<ParticleMaterial>().Get(computeData.ParticleMaterial);
                var particleHostBuffer = assetServer.GetAssets<Buffer>().Get(particleMaterial!.ParticleBuffer.Buffer);
                var particleBufferData = renderAssets.Get<BufferRenderData>(particleMaterial!.ParticleBuffer.Buffer);
                var particleBuffer = renderAssets.Get<GPUBuffer>(particleBufferData.Buffer);

                if (computeData.ComputeBindGroup == Handle<GPUBindGroup>.Null)
                {
                    particleHostBuffer!.WriteTo(particleBuffer, 0);

                    var computeBindGroup = renderContext.GPUContext.CreateBindGroup(new()
                    {
                        Label = "compute",
                        Layout = compute.BindGroupLayouts[0],
                        Entries = [
                            BindGroupEntry.BufferEntry<Particle>(0, particleBuffer, 0),
                        ]
                    });
                    computeData.ComputeBindGroup = renderAssets.Add(computeBindGroup);
                }

                var commandEncoder = renderContext.GetCurrentCommandEncoder();
                {
                    using var computeEncoder = commandEncoder.BeginComputePass("compute");
                    computeEncoder.SetPipeline(pipeline);
                    computeEncoder.SetBindGroup(0, renderAssets.Get<GPUBindGroup>(computeData.ComputeBindGroup));
                    computeEncoder.Dispatch(1000, 1, 1);
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
                    renderEncoder.SetBindGroup(0, renderAssets.Get<GPUBindGroup>(particleRenderMaterial.BindGroups[0]));
                    renderEncoder.SetVertexBuffer(0, particleBuffer, 0);
                    renderEncoder.Draw(4, 1000, 0, 0);
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
