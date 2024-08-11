namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Graphics.Rendering;

public class DrawTriangle
{
    GPUShader? shaderModule = null;
    GPURenderPipeline? renderPipeline = null;

    void Setup(IApplication app)
    {
        shaderModule = app.GPUContext.CreateShaderModule(new()
        {
            Label = "shader-module",
            Backend = ShaderBackend.WGSL,
            Content = File.ReadAllText("./assets/triangle.wgsl"),
        });
        Console.WriteLine("Shader Module Created");

        renderPipeline = app.GPUContext.CreateRenderPipeline(new()
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
        Console.WriteLine("Render Pipeline Created");
    }

    void Update(IApplication app)
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

            renderPass.SetPipeline(renderPipeline!);
            renderPass.Draw(3, 1, 0, 0);

            renderPass.End();
        }
        using var commandBuffer = commandEncoder.Finish("command-buffer");
        commandBuffer.Submit();
        app.GPUContext.Present();
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