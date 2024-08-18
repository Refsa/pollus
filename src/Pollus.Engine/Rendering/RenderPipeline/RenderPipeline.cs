namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;

public class RenderPipelineAsset
{
    public required RenderPipelineDescriptor Descriptor { get; init; }

    public static implicit operator RenderPipelineAsset(RenderPipelineDescriptor descriptor) => new() { Descriptor = descriptor };
}

public class RenderPipelineRenderData : IRenderData
{
    public required GPURenderPipeline Pipeline { get; init; }

    public void Dispose()
    {
        Pipeline.Dispose();
    }
}