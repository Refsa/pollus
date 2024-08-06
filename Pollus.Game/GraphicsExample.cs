namespace Pollus.Game;

using System.Runtime.InteropServices.JavaScript;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

public partial class GraphicsExample
{
    public static void Run()
    {
        Console.WriteLine("Graphics Example");


        using var window = new Window(new());
        Console.WriteLine("Window Created");
        using var graphicsContext = new GraphicsContext();
        Console.WriteLine("Graphics Context Created");

        WGPUContext? windowContext = null;
        
        bool isSetup = false;
        WGPUShaderModule? shaderModule = null;
        WGPURenderPipeline? renderPipeline = null;

        var startupSw = System.Diagnostics.Stopwatch.StartNew();

        window.Run(() =>
        {
            Console.WriteLine("Main loop");
            if (windowContext == null)
            {
                windowContext = graphicsContext.CreateContext("main", window);
                Console.WriteLine("Window Context Created");
            }

            if (!windowContext.IsReady)
            {
                windowContext.Setup();
                return;
            }
            if (!isSetup)
            {
                Console.WriteLine("Window Context Ready");
                isSetup = true;
                Setup();
                return;
            }

            using var surfaceTexture = windowContext.CreateSurfaceTexture();
            if (surfaceTexture.GetTextureView() is not WGPUTextureView surfaceTextureView)
            {
                return;
            }

            Span<WGPURenderPassColorAttachment> colorAttachments = stackalloc WGPURenderPassColorAttachment[1];
            using var commandEncoder = windowContext.CreateCommandEncoder("command-encoder");
            {
                colorAttachments[0] = new(
                    textureView: surfaceTextureView.Native,
                    resolveTarget: nint.Zero,
                    clearValue: new(0.2f, 0.1f, 0.01f, 1.0f),
                    loadOp: Silk.NET.WebGPU.LoadOp.Clear,
                    storeOp: Silk.NET.WebGPU.StoreOp.Store
                );
                using var renderPass = commandEncoder.BeginRenderPass(new()
                {
                    Label = "render-pass",
                    ColorAttachments = colorAttachments,
                });

                renderPass.SetPipeline(renderPipeline!);
                renderPass.Draw(3, 1, 0, 0);

                renderPass.End();
            }
            using var commandBuffer = commandEncoder.Finish("command-buffer");
            commandBuffer.Submit();
            windowContext.Present();
        });

        void Setup()
        {
            shaderModule = windowContext.CreateShaderModule(new()
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
fn fs_main() -> @location(0) vec4f {
    return vec4f(0.0, 1.0, 0.0, 1.0);
}
"""
            });

            Console.WriteLine("Shader Module Created");

            renderPipeline = windowContext.CreateRenderPipeline(new()
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
                Format = windowContext.GetSurfaceFormat(),
            }
                    ]
                },
                MultisampleState = WGPUMultisampleState.Default,
                PrimitiveState = WGPUPrimitiveState.Default,
                PipelineLayout = null,
            });

            Console.WriteLine("Render Pipeline Created");
        }
    }

    /* public static void RunInternal()
    {
        using var window = new Window(new());
        Console.WriteLine("Window Created");
        using var graphicsContext = new GraphicsContext();
        Console.WriteLine("Graphics Context Created");
        using var windowContext = graphicsContext.CreateContext("main", window);
        Console.WriteLine("Window Context Created");

        Span<WGPURenderPassColorAttachment> colorAttachments = stackalloc WGPURenderPassColorAttachment[1];
        while (window.IsOpen)
        {
            window.PollEvents();

            {
                using var surfaceTexture = windowContext.CreateSurfaceTexture();
                if (surfaceTexture.GetTextureView() is not WGPUTextureView surfaceTextureView)
                {
                    continue;
                }

                using var commandEncoder = windowContext.CreateCommandEncoder("command-encoder");
                {
                    colorAttachments[0] = new(
                        textureView: surfaceTextureView.Native,
                        resolveTarget: nint.Zero,
                        clearValue: new(0.2f, 0.1f, 0.01f, 1.0f),
                        loadOp: Silk.NET.WebGPU.LoadOp.Clear,
                        storeOp: Silk.NET.WebGPU.StoreOp.Store
                    );
                    using var renderPass = commandEncoder.BeginRenderPass(new()
                    {
                        Label = "render-pass",
                        ColorAttachments = colorAttachments,
                    });

                    renderPass.SetPipeline(renderPipeline);
                    renderPass.Draw(3, 1, 0, 0);

                    renderPass.End();
                }
                using var commandBuffer = commandEncoder.Finish("command-buffer");
                commandBuffer.Submit();
                windowContext.Present();
            }

            Thread.Sleep(8);
        }
    } */
}