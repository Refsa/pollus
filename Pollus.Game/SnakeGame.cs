using System.Runtime.InteropServices;
using System.Text;
using Pollus.Engine;
using Pollus.Engine.Input;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Pollus.Game;

struct SceneUniform
{
    public Mat4f View;
    public Mat4f Projection;
}

public class SnakeGame
{
    Keyboard? keyboard;
    GPURenderPipeline? quadRenderPipeline = null;
    GPUBindGroupLayout? bindGroupLayout0 = null;
    GPUBindGroup? bindGroup0 = null;
    GPUBuffer? quadVertexBuffer = null;
    GPUBuffer? quadIndexBuffer = null;
    GPUBuffer? sceneUniformBuffer = null;
    GPUBuffer? instanceBuffer = null;
    GPUTexture? texture = null;
    GPUTextureView? textureView = null;
    GPUSampler? textureSampler = null;
    Mat4f player = Mat4f.Identity();

    VertexData instanceData = VertexData.From(1024, [VertexFormat.Mat4x4]);

    ~SnakeGame()
    {

    }

    public void Run()
    {
        (ApplicationBuilder.Default with
        {
            OnSetup = Setup,
            OnUpdate = Update,
        }).Build().Run();
    }

    public void Setup(IApplication app)
    {
        keyboard = app.Input.GetDevice<Keyboard>("keyboard");

        // Quad Vertex Buffer
        {
            var vertexData = VertexData.From(4, stackalloc VertexFormat[] { VertexFormat.Float32x2, VertexFormat.Float32x2 });
            vertexData.Write(0, [
                ((-16f, -16f), (0f, 0f)),
                ((+16f, -16f), (1f, 0f)),
                ((+16f, +16f), (1f, 1f)),
                ((-16f, +16f), (0f, 1f)),
            ]);

            quadVertexBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = """quad-vertex-buffer""",
                Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = vertexData.SizeInBytes,
                MappedAtCreation = false,
            });
            quadVertexBuffer.Write<byte>(vertexData.AsSpan());

            Span<int> quadIndices = stackalloc int[] { 0, 1, 2, 0, 2, 3 };
            quadIndexBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = """quad-index-buffer""",
                Usage = Silk.NET.WebGPU.BufferUsage.Index | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = (ulong)(quadIndices.Length * sizeof(int)),
                MappedAtCreation = false,
            });
            quadIndexBuffer.Write<int>(quadIndices);
        }

        // Instance Buffer
        {
            instanceBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = """instance-buffer""",
                Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = Alignment.GetAlignedSize<Mat4f>(1),
                MappedAtCreation = false,
            });
            player = Mat4f.Translation(new Vec3f(200f, 200f, 0f));
        }

        // Scene Uniform Buffer
        {
            sceneUniformBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = """scene-uniform-buffer""",
                Usage = Silk.NET.WebGPU.BufferUsage.Uniform | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = Alignment.GetAlignedSize<SceneUniform>(),
                MappedAtCreation = false,
            });
            var SceneUniform = new SceneUniform
            {
                View = Mat4f.Identity(),
                Projection = Mat4f.OrthographicRightHanded(0, app.Window.Size.X, 0, app.Window.Size.Y, 0, 1),
            };
            sceneUniformBuffer.Write(SceneUniform, 0);
        }

        // Texture
        {
            texture = app.GPUContext.CreateTexture(TextureDescriptor.D2(
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
                    texture.Write<Rgba32>(row, 0, new Vec3<uint>(0, (uint)y, 0), new Vec3<uint>((uint)row.Length, 1, 1));
                }
            });

            textureSampler = app.GPUContext.CreateSampler(SamplerDescriptor.Nearest);
        }

        bindGroupLayout0 = app.GPUContext.CreateBindGroupLayout(new()
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
            using var quadShaderModule = app.GPUContext.CreateShaderModule(new()
            {
                Label = """quad-shader-module""",
                Backend = ShaderBackend.WGSL,
                Content = File.ReadAllText("./assets/snake/quad.wgsl"),
            });

            quadRenderPipeline = app.GPUContext.CreateRenderPipeline(new()
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
                            Format = app.GPUContext.GetSurfaceFormat(),
                        }
                    ]
                },
                MultisampleState = MultisampleState.Default,
                PrimitiveState = PrimitiveState.Default,
                PipelineLayout = app.GPUContext.CreatePipelineLayout(new()
                {
                    Label = """quad-pipeline-layout""",
                    Layouts = [
                        bindGroupLayout0
                    ]
                }),
            });
        }

        // Bind Group
        {
            textureView = texture.GetTextureView();
            textureView.Value.RegisterResource();
            bindGroup0 = app.GPUContext.CreateBindGroup(new()
            {
                Label = """bind-group-0""",
                Layout = bindGroupLayout0,
                Entries = [
                    BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer!, 0),
                    BindGroupEntry.TextureEntry(1, textureView.Value),
                    BindGroupEntry.SamplerEntry(2, textureSampler),
                ]
            });
        }
    }

    public void Update(IApplication app)
    {
        var input = Vec2f.Zero;
        if (keyboard!.Pressed(Key.ArrowLeft))
        {
            input += Vec2f.Left;
        }
        if (keyboard!.Pressed(Key.ArrowRight))
        {
            input += Vec2f.Right;
        }
        if (keyboard!.Pressed(Key.ArrowUp))
        {
            input += Vec2f.Down;
        }
        if (keyboard!.Pressed(Key.ArrowDown))
        {
            input += Vec2f.Up;
        }
        player.Translate(new Vec3f(input.Normalized(), 0));

        RenderExtract(app);
        Render(app);
    }

    void RenderExtract(IApplication app)
    {
        instanceData.Write(0, player);
        instanceBuffer!.Write<byte>(instanceData.Slice(0, 1));
    }

    void Render(IApplication app)
    {
        using var surfaceTexture = app.GPUContext.CreateSurfaceTexture();
        if (surfaceTexture.GetTextureView() is not GPUTextureView surfaceTextureView)
        {
            Console.WriteLine("Surface texture view is null");
            return;
        }

        using var commandEncoder = app.GPUContext.CreateCommandEncoder("""command-encoder""");
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
                renderPass.SetPipeline(quadRenderPipeline!);
                renderPass.SetVertexBuffer(0, quadVertexBuffer!);
                renderPass.SetVertexBuffer(1, instanceBuffer!);
                renderPass.SetIndexBuffer(quadIndexBuffer!, Silk.NET.WebGPU.IndexFormat.Uint32);
                renderPass.SetBindGroup(bindGroup0!, 0);
                renderPass.DrawIndexed(6, 1, 0, 0, 0);
            }

            renderPass.End();
        }
        using var commandBuffer = commandEncoder.Finish("""command-buffer""");
        commandBuffer.Submit();
        app.GPUContext.Present();
    }
}