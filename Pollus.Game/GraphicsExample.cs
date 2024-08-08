namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Graphics.WGPU;

public class GraphicsExample
{
    WGPUShaderModule? shaderModule = null;
    WGPURenderPipeline? renderPipeline = null;

    void Setup(IApplication app)
    {
        shaderModule = app.WindowContext.CreateShaderModule(new()
        {
            Label = "shader-module",
            Backend = ShaderBackend.WGSL,
            Content =
                        """
@vertex
fn vs_main(@builtin(vertex_index) in_vertex_index: u32) -> @builtin(position) vec4f {
    var p = vec2f(0.0, 0.0);
    if (in_vertex_index == 0u) {
        p = vec2f(-0.5, -0.5);
    } else if (in_vertex_index == 1u) {
        p = vec2f(0.5, -0.5);
    } else {
        p = vec2f(0.0, 0.5);
    }
    return vec4f(p, 0.0, 1.0);
}

@fragment
fn fs_main(@builtin(position) in_position: vec4f) -> @location(0) vec4f {
    return vec4f(1.0, 1.0, 0.0, 1.0);
}
"""
        });
        Console.WriteLine("Shader Module Created");

        renderPipeline = app.WindowContext.CreateRenderPipeline(new()
        {
            Label = "render-pipeline",
            VertexState = new()
            {
                ShaderModule = shaderModule,
                EntryPoint = "vs_main"
            },
            FragmentState = new()
            {
                ShaderModule = shaderModule,
                EntryPoint = "fs_main",
                ColorTargets = [
                    WGPUColorTargetState.Default with
                        {
                            Format = app.WindowContext.GetSurfaceFormat(),
                        }
                ]
            },
            MultisampleState = WGPUMultisampleState.Default,
            PrimitiveState = WGPUPrimitiveState.Default,
            PipelineLayout = null,
        });
        Console.WriteLine("Render Pipeline Created");
    }

    void Update(IApplication app)
    {
        using var surfaceTexture = app.WindowContext.CreateSurfaceTexture();
        if (surfaceTexture.GetTextureView() is not WGPUTextureView surfaceTextureView)
        {
            Console.WriteLine("Surface texture view is null");
            return;
        }

        using var commandEncoder = app.WindowContext.CreateCommandEncoder("command-encoder");
        {
            using var renderPass = commandEncoder.BeginRenderPass(new()
            {
                Label = "render-pass",
                ColorAttachments = stackalloc WGPURenderPassColorAttachment[1]
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

            renderPass.SetPipeline(renderPipeline!);
            renderPass.Draw(3, 1, 0, 0);

            renderPass.End();
        }
        using var commandBuffer = commandEncoder.Finish("command-buffer");
        commandBuffer.Submit();
        app.WindowContext.Present();
    }

    public void Run()
    {
        Console.WriteLine("Graphics Example");

        (ApplicationBuilder.Default with
        {
            OnSetup = Setup,
            OnUpdate = Update,
        })
        .Build().Run();
    }
}