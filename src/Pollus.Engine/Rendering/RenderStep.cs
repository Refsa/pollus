namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics.Rendering;

public enum RenderStep2D
{
    Main,
    PostProcess,
    UI,
}

public class RenderSteps
{
    Dictionary<RenderStep2D, List<IRenderStepDraw>> stages = new();

    public Dictionary<RenderStep2D, List<IRenderStepDraw>> Stages => stages;

    public void Add(IRenderStepDraw stage)
    {
        if (!stages.TryGetValue(stage.Stage, out var list))
        {
            list = [];
            stages.Add(stage.Stage, list);
        }
        list.Add(stage);
    }
}

public interface IRenderStepDraw
{
    public RenderStep2D Stage { get; }
    void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets);
}