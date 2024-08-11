using Pollus.Engine;
using Pollus.Engine.Input;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

namespace Pollus.Game;

public class SnakeGame
{
    Keyboard? keyboard;
    GPURenderPipeline? quadRenderPipeline = null;
    GPUBuffer? quadVertexBuffer = null;
    GPUBuffer? quadIndexBuffer = null;

    Vector2<float>[] quadVertices = [
        (-0.5f, -0.5f),
        (+0.5f, -0.5f),
        (+0.5f, +0.5f),
        (-0.5f, +0.5f),
    ];
    int[] quadIndices = [0, 1, 2, 0, 2, 3];

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

        using var quadShaderModule = app.GPUContext.CreateShaderModule(new()
        {
            Label = "quad-shader-module",
            Backend = ShaderBackend.WGSL,
            Content = File.ReadAllText("./assets/snake/quad.wgsl"),
        });

        quadRenderPipeline = app.GPUContext.CreateRenderPipeline(new()
        {
            Label = "quad-render-pipeline",
            VertexState = new()
            {
                ShaderModule = quadShaderModule,
                EntryPoint = "vs_main",
                Layouts = [
                    VertexBufferLayout.Vertex(0, [
                        VertexFormat.Float32x2
                    ])
                ]
            },
            FragmentState = new()
            {
                ShaderModule = quadShaderModule,
                EntryPoint = "fs_main",
                ColorTargets = [
                    ColorTargetState.Default with
                    {
                        Format = app.GPUContext.GetSurfaceFormat(),
                    }
                ]
            },
            MultisampleState = MultisampleState.Default,
            PrimitiveState = PrimitiveState.Default,
            PipelineLayout = null,
        });

        quadVertexBuffer = app.GPUContext.CreateBuffer(new()
        {
            Label = "quad-vertex-buffer",
            Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
            Size = (ulong)(quadVertices.Length * Vector3<float>.SizeInBytes),
            MappedAtCreation = false,
        });
        quadVertexBuffer.Write<Vector2<float>>(quadVertices);

        quadIndexBuffer = app.GPUContext.CreateBuffer(new()
        {
            Label = "quad-index-buffer",
            Usage = Silk.NET.WebGPU.BufferUsage.Index | Silk.NET.WebGPU.BufferUsage.CopyDst,
            Size = (ulong)(quadIndices.Length * sizeof(int)),
            MappedAtCreation = false,
        });
        quadIndexBuffer.Write<int>(quadIndices);
    }

    public void Update(IApplication app)
    {

        using var surfaceTexture = app.GPUContext.CreateSurfaceTexture();
        if (surfaceTexture.GetTextureView() is not GPUTextureView surfaceTextureView)
        {
            Console.WriteLine("Surface texture view is null");
            return;
        }

        using var commandEncoder = app.GPUContext.CreateCommandEncoder("command-encoder");
        {
            using var renderPass = commandEncoder.BeginRenderPass(new()
            {
                Label = "render-pass",
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

            renderPass.SetPipeline(quadRenderPipeline!);
            renderPass.SetVertexBuffer(0, quadVertexBuffer!);
            renderPass.SetIndexBuffer(quadIndexBuffer!, Silk.NET.WebGPU.IndexFormat.Uint32);
            renderPass.DrawIndexed((uint)quadIndices.Length, 1, 0, 0, 0);

            renderPass.End();
        }
        using var commandBuffer = commandEncoder.Finish("command-buffer");
        commandBuffer.Submit();
        app.GPUContext.Present();
    }
}