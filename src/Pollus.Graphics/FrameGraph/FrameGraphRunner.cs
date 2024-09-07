namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class FrameGraphRunner<TRenderAssets>
{
    FrameGraph<TRenderAssets> frameGraph;
    IWGPUContext gpuContext;

    GPUCommandEncoder rendering;
    GPUCommandEncoder compute;

    public FrameGraphRunner(FrameGraph<TRenderAssets> frameGraph, IWGPUContext gpuContext)
    {
        this.frameGraph = frameGraph;
        this.gpuContext = gpuContext;
    }

    public void Run()
    {
        BeginFrame(gpuContext);
        {
            
        }
        EndFrame(gpuContext);
    }

    void BeginFrame(IWGPUContext gpuContext)
    {
        rendering = gpuContext.CreateCommandEncoder("""rendering-command-encoder""");
        compute = gpuContext.CreateCommandEncoder("""compute-command-encoder""");
    }

    void EndFrame(IWGPUContext gpuContext)
    {
        using var renderingCommandBuffer = rendering.Finish("""rendering-command-encoder""");
        using var computeCommandBuffer = compute.Finish("""compute-command-encoder""");

        renderingCommandBuffer.Submit();
        computeCommandBuffer.Submit();

        gpuContext.Present();

        rendering.Dispose();
        compute.Dispose();
    }
}