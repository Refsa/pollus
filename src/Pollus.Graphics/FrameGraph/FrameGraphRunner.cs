namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;

public ref struct FrameGraphRunner<TParam>
{
    internal ReadOnlySpan<int> order;
    readonly ref readonly FrameGraph<TParam> frameGraph;

    public FrameGraphRunner(ref FrameGraph<TParam> frameGraph, ReadOnlySpan<int> order)
    {
        this.frameGraph = ref frameGraph;
        this.order = order;
    }

    public void Execute(RenderContext renderContext, in TParam param)
    {
        renderContext.PrepareResources(ref frameGraph.Resources);
        renderContext.CreateCommandEncoder("""frame-graph-command-encoder""");
        foreach (var passIndex in order)
        {
            frameGraph.ExecutePass(passIndex, renderContext, param);
        }
    }
}