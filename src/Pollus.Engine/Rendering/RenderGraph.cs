namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics.Rendering;

public enum RenderPassStage2D
{
    Main,
    PostProcess,
    UI,
}

public class RenderGraph
{
    Dictionary<RenderPassStage2D, List<IRenderPassStageDraw>> stages = new();

    public Dictionary<RenderPassStage2D, List<IRenderPassStageDraw>> Stages => stages;

    public void Add(IRenderPassStageDraw stage)
    {
        if (!stages.TryGetValue(stage.Stage, out var list))
        {
            list = [];
            stages.Add(stage.Stage, list);
        }
        list.Add(stage);
    }
}

public interface IRenderPassStageDraw
{
    public RenderPassStage2D Stage { get; }
    void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets);
}