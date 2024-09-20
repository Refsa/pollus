namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;


class RenderData : IDisposable
{
    public required GPURenderPipeline RenderPipeline;

    public void Dispose()
    {
        RenderPipeline.Dispose();
    }
}

public class DrawTriangleExample : IExample
{
    public string Name => "draw-triangle";
    IApplication? application;
    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .InitResource<RenderData>()
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
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
                    CullMode = CullMode.None,
                    FrontFace = FrontFace.Ccw,
                    Topology = PrimitiveTopology.TriangleList,
                },
                PipelineLayout = null,
            });

            resources.Add(new RenderData { RenderPipeline = renderPipeline });

            Log.Info("Render Pipeline Created");
        }))
        .AddSystem(CoreStage.Last, FnSystem.Create("Draw",
        static (IWGPUContext gpuContext, RenderData renderData) =>
        {
            using var surfaceTexture = new GPUSurfaceTexture(gpuContext);
            if (!surfaceTexture.Prepare())
            {
                Log.Error("Failed to prepare surface texture");
                surfaceTexture.Dispose();
                return;
            }

            using var commandEncoder = gpuContext.CreateCommandEncoder("""command-encoder""");
            {
                using var renderPass = commandEncoder.BeginRenderPass(new()
                {
                    Label = """render-pass""",
                    ColorAttachments = stackalloc RenderPassColorAttachment[1]
                    {
                        new()
                        {
                            View = surfaceTexture.TextureView.Native,
                            ResolveTarget = nint.Zero,
                            ClearValue = new(0.2f, 0.1f, 0.01f, 1.0f),
                            LoadOp = LoadOp.Clear,
                            StoreOp = StoreOp.Store
                        }
                    },
                });

                renderPass.SetPipeline(renderData.RenderPipeline);
                renderPass.Draw(3, 1, 0, 0);
            }

            using var commandBuffer = commandEncoder.Finish("""command-buffer""");
            commandBuffer.Submit();
            commandBuffer.Dispose();

            gpuContext.Present();
        }))
        .Build())
        .Run();
}