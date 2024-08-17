namespace Pollus.Game;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Input;
using Pollus.Engine.Mesh;
using Pollus.Engine.Platform;
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
        .AddPlugin(new MeshPlugin { SharedPrimitives = PrimitiveType.Quad })
        .AddPlugin<InputPlugin>()
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
                    Scale = Vec2f.One * 32f,
                    Rotation = 0,
                },
                new Renderable()
            );

            world.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.PostInit, FnSystem("SetupRenderData",
        static (Resources resources, IWGPUContext gpuContext, IWindow window, PrimitiveMeshes primitives, Assets<MeshAsset> meshes) =>
        {
            var renderData = new SnakeRenderData();
            resources.Add(renderData);

            // Quad Mesh
            {
                var quadMesh = meshes.Get(primitives.Quad)!;

                // Vertex Buffer
                var vertexData = quadMesh.Mesh.GetVertexData([MeshAttributeType.Position, MeshAttributeType.UV0]);

                renderData.quadVertexBuffer = gpuContext.CreateBuffer(
                    BufferDescriptor.Vertex("""quad-vertex-buffer""", vertexData.SizeInBytes));
                renderData.quadVertexBuffer.Write<byte>(vertexData.AsSpan());

                var indices = quadMesh.Mesh.GetIndexData();
                renderData.quadIndexBuffer = gpuContext.CreateBuffer(
                    BufferDescriptor.Index("""quad-index-buffer""", (ulong)indices.Length));
                renderData.quadIndexBuffer.Write<byte>(indices);

                // Instance Buffer
                renderData.instanceBuffer = gpuContext.CreateBuffer(
                    BufferDescriptor.Vertex("""instance-buffer""", (ulong)Mat4f.SizeInBytes));
            }

            // Scene Uniform Buffer
            {
                renderData.sceneUniformBuffer = gpuContext.CreateBuffer(
                    BufferDescriptor.Uniform<SceneUniform>("""scene-uniform-buffer"""));
            }

            // Texture
            {
                renderData.texture = gpuContext.CreateTexture(TextureDescriptor.D2(
                    """texture""",
                    TextureUsage.TextureBinding | TextureUsage.CopyDst,
                    TextureFormat.Rgba8Unorm,
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
                    BindGroupLayoutEntry.Uniform<SceneUniform>(0, ShaderStage.Vertex, false),
                    BindGroupLayoutEntry.TextureEntry(1, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
                    BindGroupLayoutEntry.SamplerEntry(2, ShaderStage.Fragment, SamplerBindingType.Filtering),
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
                                VertexFormat.Float32x3,
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
            var inputVec = keyboard!.GetAxis2D(Key.ArrowLeft, Key.ArrowRight, Key.ArrowUp, Key.ArrowDown);

            qPlayer.ForEach((ref Transform2 transform) =>
            {
                transform.Position += inputVec;
            });
        }))
        .AddSystem(CoreStage.PreRender, FnSystem("RenderExtract",
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
        }).RunCriteria(new RunFixed(120)))
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
                            loadOp: LoadOp.Clear,
                            storeOp: StoreOp.Store
                        )
                    },
                });

                {
                    renderPass.SetPipeline(renderData.quadRenderPipeline!);
                    renderPass.SetBindGroup(renderData.bindGroup0!, 0);
                    renderPass.SetIndexBuffer(renderData.quadIndexBuffer!, IndexFormat.Uint16);
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