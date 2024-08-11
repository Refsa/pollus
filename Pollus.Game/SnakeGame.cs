using Pollus.Engine;
using Pollus.Engine.Input;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

namespace Pollus.Game;

struct SceneUniform
{
    public Mat4<float> View;
    public Mat4<float> Projection;
}

public class SnakeGame
{
    Keyboard? keyboard;
    GPURenderPipeline? quadRenderPipeline = null;
    GPUBindGroupLayout? sceneUniformBindGroupLayout = null;
    GPUBindGroup? sceneUniformBindGroup = null;
    GPUBuffer? quadVertexBuffer = null;
    GPUBuffer? quadIndexBuffer = null;
    GPUBuffer? sceneUniformBuffer = null;
    GPUBuffer? instanceBuffer = null;
    Mat4<float> player = Mat4<float>.Identity();

    Vec2<float>[] quadVertices = [
        (-50f, -50f),
        (+50f, -50f),
        (+50f, +50f),
        (-50f, +50f),
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

        // Quad Vertex Buffer
        {
            quadVertexBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = "quad-vertex-buffer",
                Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = (ulong)(quadVertices.Length * Vec3<float>.SizeInBytes),
                MappedAtCreation = false,
            });
            quadVertexBuffer.Write<Vec2<float>>(quadVertices);

            quadIndexBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = "quad-index-buffer",
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
                Label = "instance-buffer",
                Usage = Silk.NET.WebGPU.BufferUsage.Vertex | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = Alignment.GetAlignedSize<Mat4<float>>(1),
                MappedAtCreation = false,
            });
            player = Mat4<float>.Translation(new Vec3<float>(200f, 200f, 0f));
            instanceBuffer.Write(player, 0);
        }

        // Scene Uniform Buffer
        {
            sceneUniformBuffer = app.GPUContext.CreateBuffer(new()
            {
                Label = "scene-uniform-buffer",
                Usage = Silk.NET.WebGPU.BufferUsage.Uniform | Silk.NET.WebGPU.BufferUsage.CopyDst,
                Size = Alignment.GetAlignedSize<SceneUniform>(),
                MappedAtCreation = false,
            });
            var SceneUniform = new SceneUniform
            {
                View = Mat4<float>.Identity(),
                Projection = Mat4<float>.OrthographicRightHanded(0, 1600, 0, 900, 0, 1),
            };
            sceneUniformBuffer.Write(SceneUniform, 0);
        }

        using var quadShaderModule = app.GPUContext.CreateShaderModule(new()
        {
            Label = "quad-shader-module",
            Backend = ShaderBackend.WGSL,
            Content = File.ReadAllText("./assets/snake/quad.wgsl"),
        });

        sceneUniformBindGroupLayout = app.GPUContext.CreateBindGroupLayout(new()
        {
            Label = "scene-uniform-bind-group-layout",
            Entries = [
                BindGroupLayoutEntry.BufferEntry<SceneUniform>(0, Silk.NET.WebGPU.ShaderStage.Vertex, Silk.NET.WebGPU.BufferBindingType.Uniform, false)
            ]
        });

        sceneUniformBindGroup = app.GPUContext.CreateBindGroup(new()
        {
            Label = "scene-uniform-bind-group",
            Layout = sceneUniformBindGroupLayout,
            Entries = [
                BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer!, 0)
            ]
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
                    ]),
                    VertexBufferLayout.Instance(5, [
                        VertexFormat.Mat4x4
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
            PipelineLayout = app.GPUContext.CreatePipelineLayout(new()
            {
                Label = "quad-pipeline-layout",
                Layouts = [
                    sceneUniformBindGroupLayout
                ]
            }),
        });
    }
 
    public void Update(IApplication app)
    {
        if (keyboard!.Pressed(Key.ArrowLeft))
        {
            player.Translate(new Vec3<float>(+1f, 0f, 0f));
        }
        if (keyboard!.Pressed(Key.ArrowRight))
        {
            player.Translate(new Vec3<float>(-1f, 0f, 0f));
        }
        if (keyboard!.Pressed(Key.ArrowUp))
        {
            player.Translate(new Vec3<float>(0f, +1f, 0f));
        }
        if (keyboard!.Pressed(Key.ArrowDown))
        {
            player.Translate(new Vec3<float>(0f, -1f, 0f));
        }
        instanceBuffer!.Write(player, 0);

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
            renderPass.SetVertexBuffer(1, instanceBuffer!);
            renderPass.SetIndexBuffer(quadIndexBuffer!, Silk.NET.WebGPU.IndexFormat.Uint32);
            renderPass.SetBindGroup(sceneUniformBindGroup!, 0);
            renderPass.DrawIndexed((uint)quadIndices.Length, 1, 0, 0, 0);

            renderPass.End();
        }
        using var commandBuffer = commandEncoder.Finish("command-buffer");
        commandBuffer.Submit();
        app.GPUContext.Present();
    }
}