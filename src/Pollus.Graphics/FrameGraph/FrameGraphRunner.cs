namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public ref struct FrameGraphRunner<TParam>
{
    internal ReadOnlySpan<int> order;
    readonly FrameGraph<TParam> frameGraph;

    public FrameGraphRunner(FrameGraph<TParam> frameGraph, scoped in ReadOnlySpan<int> order)
    {
        this.frameGraph = frameGraph;
        this.order = order;
    }

    public void Execute(RenderContext renderContext, TParam param)
    {
        renderContext.PrepareResources(ref frameGraph.Resources);
        renderContext.CreateCommandEncoder("""frame-graph-command-encoder""");
        foreach (var passIndex in order)
        {
            frameGraph.ExecutePass(passIndex, renderContext, param);
        }
    }
}