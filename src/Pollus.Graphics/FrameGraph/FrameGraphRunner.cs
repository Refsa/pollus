namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public ref struct FrameGraphRunner<TExecuteParam>
{
    internal ReadOnlySpan<int> order;
    FrameGraph<TExecuteParam> frameGraph;

    GPUCommandEncoder encoder;

    public FrameGraphRunner(FrameGraph<TExecuteParam> frameGraph, scoped in ReadOnlySpan<int> order)
    {
        this.frameGraph = frameGraph;
        this.order = order;
    }

    public void Execute(RenderContext renderContext, TExecuteParam param)
    {
        // BeginFrame(renderContext);
        foreach (var passIndex in order)
        {
            frameGraph.ExecutePass(passIndex, renderContext, param);
        }
        // EndFrame(renderContext);
    }

    void BeginFrame(RenderContext renderContext)
    {
        encoder = renderContext.GPUContext.CreateCommandEncoder("""frame-graph-command-encoder""");
    }

    void EndFrame(RenderContext renderContext)
    {
        using var renderingCommandBuffer = encoder.Finish("""rendering-command-encoder""");

        renderContext.GPUContext.Present();
        encoder.Dispose();
    }
}