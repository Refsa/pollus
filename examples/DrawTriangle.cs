namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using static Pollus.ECS.SystemBuilder;

class RenderData : IDisposable
{
    public required GPURenderPipeline RenderPipeline;

    public void Dispose()
    {
        RenderPipeline.Dispose();
    }
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
                PrimitiveState = PrimitiveState.Default with
                {
                    CullMode = Silk.NET.WebGPU.CullMode.None,
                    FrontFace = Silk.NET.WebGPU.FrontFace.Ccw,
                    Topology = Silk.NET.WebGPU.PrimitiveTopology.TriangleList,
                },
                PipelineLayout = null,
            });

            resources.Add(new RenderData { RenderPipeline = renderPipeline });

            Log.Info("Render Pipeline Created");
        }))
        .AddSystem(CoreStage.Last, FnSystem("Draw",
        static (IWGPUContext gpuContext, RenderData renderData) =>
        {
            var surfaceTexture = gpuContext.SurfaceGetCurrentTexture();
            using var textureView = gpuContext.CreateTextureView(surfaceTexture, new());

            using var commandEncoder = gpuContext.CreateCommandEncoder("""command-encoder""");
            {
                using var renderPass = commandEncoder.BeginRenderPass(new()
                {
                    Label = """render-pass""",
                    ColorAttachments = stackalloc RenderPassColorAttachment[1]
                    {
                        new(
                            textureView: textureView.Native,
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

            using var commandBuffer = commandEncoder.Finish("""command-buffer""");
            commandBuffer.Submit();
            commandBuffer.Dispose();

            gpuContext.Present();
            gpuContext.ReleaseSurfaceTexture(surfaceTexture);
        }))
        .Run();
}