namespace Pollus.Game;

using Pollus.ECS;
using Pollus.Engine;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using static Pollus.ECS.SystemBuilder;

class RenderData
{
    public required GPURenderPipeline RenderPipeline { get; set; }
}

public class DrawTriangle
{
    public void Run() => Application.Builder
        .InitResource<RenderData>()
        .AddSystem(CoreStage.PostInit, FnSystem("Setup",
        static (IWGPUContext gpuContext, Resources resources) =>
        {
            using var shaderModule = gpuContext.CreateShaderModule(new()
            {
                Label = "shader-module",
                Backend = ShaderBackend.WGSL,
                Content = File.ReadAllText("./assets/shaders/triangle.wgsl"),
            });

            var renderPipeline = gpuContext.CreateRenderPipeline(new()
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
                            Format = gpuContext.GetSurfaceFormat(),
                        }
                    ]
                },
                MultisampleState = MultisampleState.Default,
                PrimitiveState = PrimitiveState.Default,
                PipelineLayout = null,
            });

            resources.Add(new RenderData { RenderPipeline = renderPipeline });

            Console.WriteLine("Render Pipeline Created");
        }))
        .AddSystem(CoreStage.Last, FnSystem("Draw",
        static (IWGPUContext gpuContext, RenderData renderData) =>
        {
            using var surfaceTexture = gpuContext.CreateSurfaceTexture();
            if (surfaceTexture.GetTextureView() is not GPUTextureView surfaceTextureView)
            {
                Console.WriteLine("Surface texture view is null");
                return;
            }

            using var commandEncoder = gpuContext.CreateCommandEncoder("command-encoder");
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
                            loadOp: LoadOp.Clear,
                            storeOp: StoreOp.Store
                        )
                    },
                });

                renderPass.SetPipeline(renderData.RenderPipeline);
                renderPass.Draw(3, 1, 0, 0);

                renderPass.End();
            }
            using var commandBuffer = commandEncoder.Finish("command-buffer");
            commandBuffer.Submit();
            gpuContext.Present();
        }))
        .Run();
}