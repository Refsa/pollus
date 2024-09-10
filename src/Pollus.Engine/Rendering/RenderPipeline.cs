namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;

public struct FrameGraphParam
{
    public required RenderAssets RenderAssets { get; init; }
    public required DrawGroups2D RenderSteps;
    public required Resources Resources;
    public required TextureFormat BackbufferFormat;
    public required Vec2<uint> BackbufferSize;
}

public class RenderPipeline
{
    FrameGraph<FrameGraphParam>? frameGraph;

    public void Begin()
    {
        frameGraph = new FrameGraph<FrameGraphParam>();
    }
}