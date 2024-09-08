namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public ref struct FrameGraphRunner<TExecuteParam>
{
    internal ReadOnlySpan<int> order;
    FrameGraph<TExecuteParam> frameGraph;

    public FrameGraphRunner(FrameGraph<TExecuteParam> frameGraph, scoped in ReadOnlySpan<int> order)
    {
        this.frameGraph = frameGraph;
        this.order = order;
    }

    public void Execute(RenderContext renderContext, TExecuteParam param)
    {
        renderContext.PrepareResources(frameGraph.Resources);
        renderContext.CreateCommandEncoder("""frame-graph-command-encoder""");
        foreach (var passIndex in order)
        {
            frameGraph.ExecutePass(passIndex, renderContext, param);
        }
    }
}