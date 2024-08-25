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
    List<RenderStep2D> order = [RenderStep2D.Main, RenderStep2D.PostProcess, RenderStep2D.UI];
    Dictionary<RenderStep2D, List<IRenderStepDraw>> stages = new();

    public IReadOnlyDictionary<RenderStep2D, List<IRenderStepDraw>> Stages => stages;
    public IReadOnlyList<RenderStep2D> Order => order;

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