namespace Pollus.Game;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Input;
using Pollus.Engine.Transform;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static Pollus.ECS.SystemBuilder;


struct SceneUniform
{
    public Mat4f View;
    public Mat4f Projection;
}

struct Player : IComponent { }

struct Renderable : IComponent { }

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
        .AddPlugin(new AssetPlugin { RootPath = "assets" })
        .AddPlugin<InputPlugin>()
        .AddPlugin<TimePlugin>()
        .AddPlugin<CameraPlugin>()
        .InitResource<SnakeRenderData>()
        .AddSystem(CoreStage.PostInit, FnSystem("SetupEntities",
        static (World world) =>
        {
            world.Spawn(
                new Player(),
                new Transform2
                {
                    Position = Vec2f.Zero,
                    Scale = Vec2f.One,
                    Rotation = 0,
                },
                new Renderable()
            );

            world.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.PostInit, FnSystem("SetupRenderData",
        static (Resources resources, IWGPUContext gpuContext, IWindow window) =>
        {
            var renderData = new SnakeRenderData();
            resources.Add(renderData);

            // Quad Mesh
            {
                // Vertex Buffer
                var vertexData = VertexData.From(4, stackalloc VertexFormat[] { VertexFormat.Float32x2, VertexFormat.Float32x2 });
                vertexData.Write(0, [
                    ((-16f, -16f), (0f, 0f)),
                ((+16f, -16f), (1f, 0f)),
                ((+16f, +16f), (1f, 1f)),
                ((-16f, +16f), (0f, 1f)),
            ]);

                renderData.quadVertexBuffer = gpuContext.CreateBuffer(new()
                {
                    Label = """quad-vertex-buffer""",
                    Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
                    Size = vertexData.SizeInBytes,
                    MappedAtCreation = false,
                });
                renderData.quadVertexBuffer.Write<byte>(vertexData.AsSpan());

                Span<int> quadIndices = stackalloc int[] { 0, 1, 2, 0, 2, 3 };
                renderData.quadIndexBuffer = gpuContext.CreateBuffer(new()
                {
                    Label = """quad-index-buffer""",
                    Usage = Silk.NET.WebGPU.BufferUsage.Index | Silk.NET.WebGPU.BufferUsage.CopyDst,
                    Size = (ulong)(quadIndices.Length * sizeof(int)),
                    MappedAtCreation = false,
                });
                renderData.quadIndexBuffer.Write<int>(quadIndices);

                // Index Buffer
                renderData.instanceBuffer = gpuContext.CreateBuffer(new()
                {
                    Label = """instance-buffer""",
                    Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
                    Size = Alignment.GetAlignedSize<Mat4f>(1),
                    MappedAtCreation = false,
                });
            }

            // Scene Uniform Buffer
            {
                renderData.sceneUniformBuffer = gpuContext.CreateBuffer(new()
                {
                    Label = """scene-uniform-buffer""",
                    Usage = Silk.NET.WebGPU.BufferUsage.Uniform | Silk.NET.WebGPU.BufferUsage.CopyDst,
                    Size = Alignment.GetAlignedSize<SceneUniform>(),
                    MappedAtCreation = false,
                });
            }

            // Texture
            {
                renderData.texture = gpuContext.CreateTexture(TextureDescriptor.D2(
                    """texture""",
                    Silk.NET.WebGPU.TextureUsage.TextureBinding | Silk.NET.WebGPU.TextureUsage.CopyDst,
                    Silk.NET.WebGPU.TextureFormat.Rgba8Unorm,
                    (16, 16)
                ));

                using var imgFile = File.OpenRead("./assets/snake/snake_head.png");
                using var img = Image.Load<Rgba32>(imgFile);
                img.ProcessPixelRows((accessor) =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        var row = accessor.GetRowSpan(y);
                        renderData.texture.Write<Rgba32>(row, 0, new Vec3<uint>(0, (uint)y, 0), new Vec3<uint>((uint)row.Length, 1, 1));
                    }
                });

                renderData.textureSampler = gpuContext.CreateSampler(SamplerDescriptor.Nearest);
            }

            renderData.bindGroupLayout0 = gpuContext.CreateBindGroupLayout(new()
            {
                Label = """bind-group-layout-0""",
                Entries = [
                    BindGroupLayoutEntry.Uniform<SceneUniform>(0, Silk.NET.WebGPU.ShaderStage.Vertex, false),
                BindGroupLayoutEntry.TextureEntry(1, Silk.NET.WebGPU.ShaderStage.Fragment, Silk.NET.WebGPU.TextureSampleType.Float, Silk.NET.WebGPU.TextureViewDimension.Dimension2D),
                BindGroupLayoutEntry.SamplerEntry(2, Silk.NET.WebGPU.ShaderStage.Fragment, Silk.NET.WebGPU.SamplerBindingType.Filtering),
            ]
            });

            // Pipeline
            {
                using var quadShaderModule = gpuContext.CreateShaderModule(new()
                {
                    Label = """quad-shader-module""",
                    Backend = ShaderBackend.WGSL,
                    Content = File.ReadAllText("./assets/shaders/quad.wgsl"),
                });

                renderData.quadRenderPipeline = gpuContext.CreateRenderPipeline(new()
                {
                    Label = """quad-render-pipeline""",
                    VertexState = new()
                    {
                        ShaderModule = quadShaderModule,
                        EntryPoint = """vs_main""",
                        Layouts = [
                            VertexBufferLayout.Vertex(0, [
                            VertexFormat.Float32x2,
                            VertexFormat.Float32x2,
                        ]),
                        VertexBufferLayout.Instance(5, [
                            VertexFormat.Mat4x4,
                        ])
                        ]
                    },
                    FragmentState = new()
                    {
                        ShaderModule = quadShaderModule,
                        EntryPoint = """fs_main""",
                        ColorTargets = [
                            ColorTargetState.Default with
                        {
                            Format = gpuContext.GetSurfaceFormat(),
                        }
                        ]
                    },
                    MultisampleState = MultisampleState.Default,
                    PrimitiveState = PrimitiveState.Default,
                    PipelineLayout = gpuContext.CreatePipelineLayout(new()
                    {
                        Label = """quad-pipeline-layout""",
                        Layouts = [
                            renderData.bindGroupLayout0
                        ]
                    }),
                });
            }

            // Bind Group
            {
                renderData.textureView = renderData.texture.GetTextureView();
                renderData.textureView.Value.RegisterResource();
                renderData.bindGroup0 = gpuContext.CreateBindGroup(new()
                {
                    Label = """bind-group-0""",
                    Layout = renderData.bindGroupLayout0,
                    Entries = [
                        BindGroupEntry.BufferEntry<SceneUniform>(0, renderData.sceneUniformBuffer!, 0),
                    BindGroupEntry.TextureEntry(1, renderData.textureView.Value),
                    BindGroupEntry.SamplerEntry(2, renderData.textureSampler),
                ]
                });
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("PlayerUpdate",
        static (InputManager input, Query<Transform2>.Filter<All<Player>> qPlayer) =>
        {
            var keyboard = input.GetDevice("keyboard") as Keyboard;
            var inputVec = keyboard!.GetAxis2D(Key.ArrowRight, Key.ArrowLeft, Key.ArrowDown, Key.ArrowUp);

            qPlayer.ForEach((ref Transform2 transform) =>
            {
                transform.Position += inputVec;
            });
        }))
        .AddSystem(CoreStage.Last, FnSystem("RenderExtract",
        static (SnakeRenderData renderData, Query<Transform2>.Filter<All<Renderable>> qTransforms, Query<Projection, Transform2>.Filter<All<Camera2D>> qCamera) =>
        {
            int index = 0;
            qTransforms.ForEach((ref Transform2 transform) =>
            {
                renderData.instanceData.Write(index++, transform.ToMatrix());
            });
            renderData.instanceBuffer!.Write<byte>(renderData.instanceData.Slice(0, index));

            var SceneUniform = new SceneUniform();
            qCamera.ForEach((ref Projection projection, ref Transform2 transform) =>
            {
                SceneUniform.View = transform.ToMatrix();
                SceneUniform.Projection = projection.GetProjection();
            });
            renderData.sceneUniformBuffer!.Write(SceneUniform, 0);
        }))
        .AddSystem(CoreStage.Render, FnSystem("Render",
        static (IWGPUContext gpuContext, SnakeRenderData renderData) =>
        {
            using var surfaceTexture = gpuContext.CreateSurfaceTexture();
            if (surfaceTexture.GetTextureView() is not GPUTextureView surfaceTextureView)
            {
                Console.WriteLine("Surface texture view is null");
                return;
            }

            using var commandEncoder = gpuContext.CreateCommandEncoder("""command-encoder""");
            {
                using var renderPass = commandEncoder.BeginRenderPass(new()
                {
                    Label = """render-pass""",
                    ColorAttachments = stackalloc RenderPassColorAttachment[1]
                    {
                    new(
                        textureView: surfaceTextureView.Native,
                        resolveTarget: nint.Zero,
                        clearValue: new(0.2f, 0.1f, 0.01f, 1.0f),
                        loadOp: Silk.NET.WebGPU.LoadOp.Clear,
                        storeOp: Silk.NET.WebGPU.StoreOp.Store
                    )
                },
                });

                {
                    renderPass.SetPipeline(renderData.quadRenderPipeline!);
                    renderPass.SetBindGroup(renderData.bindGroup0!, 0);
                    renderPass.SetIndexBuffer(renderData.quadIndexBuffer!, Silk.NET.WebGPU.IndexFormat.Uint32);
                    renderPass.SetVertexBuffer(0, renderData.quadVertexBuffer!);
                    renderPass.SetVertexBuffer(1, renderData.instanceBuffer!);
                    renderPass.DrawIndexed(6, 1, 0, 0, 0);
                }

                renderPass.End();
            }
            using var commandBuffer = commandEncoder.Finish("""command-buffer""");
            commandBuffer.Submit();
            gpuContext.Present();
        }))
        .Run();
}