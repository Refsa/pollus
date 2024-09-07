namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public ref struct FrameGraphRunner<TExecuteParam>
{
    internal ReadOnlySpan<int> order;
    FrameGraph<TExecuteParam> frameGraph;

    GPUCommandEncoder rendering;
    GPUCommandEncoder compute;

    public FrameGraphRunner(FrameGraph<TExecuteParam> frameGraph, scoped in ReadOnlySpan<int> order)
    {
        this.frameGraph = frameGraph;
        this.order = order;
    }

    public void Execute(IWGPUContext gpuContext, TExecuteParam param)
    {
        BeginFrame(gpuContext);
        foreach (var passIndex in order)
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