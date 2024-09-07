namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class FrameGraph<TRenderAssets>
{
    public delegate void BuilderDelegate<TData>(FrameGraphBuilder<TRenderAssets> builder, TData data);
    public delegate void ExecuteDelegate<TData>(RenderContext context, TRenderAssets renderAssets, TData data);

    FrameGraphResources<BufferFrameResource> bufferResources = new();
    FrameGraphResources<TextureFrameResource> textureResources = new();
    List<PassNode> passes = new();

    public FrameGraphResources<BufferFrameResource> BufferResources => bufferResources;
    public FrameGraphResources<TextureFrameResource> TextureResources => textureResources;

    public FrameGraph()
    {

    }

    public void AddPass<TData>(string name, BuilderDelegate<TData> builder, ExecuteDelegate<TData> execute)
    {

    }

    public FrameGraph<TRenderAssets> Compile()
    {
        return this;
    }

    public void Execute(IWGPUContext gpuContext)
    {
        var runner = new FrameGraphRunner<TRenderAssets>(this, gpuContext);
        runner.Run();
    }

    public ResourceHandle<BufferFrameResource> AddBufferResource(BufferFrameResource resource)
    {
        return bufferResources.Add(resource);
    }

    public ResourceHandle<TextureFrameResource> AddTextureResource(TextureFrameResource resource)
    {
        return textureResources.Add(resource);
    }
}

public struct FrameGraphBuilder<TRenderAssets>
{
    FrameGraph<TRenderAssets> frameGraph;

    public FrameGraphBuilder(FrameGraph<TRenderAssets> frameGraph)
    {
        this.frameGraph = frameGraph;
    }

    public ResourceHandle<BufferFrameResource> LookupBuffer(string name)
    {
        return frameGraph.BufferResources.GetHandle(name);
    }

    public ResourceHandle<TextureFrameResource> LookupTexture(string name)
    {
        return frameGraph.TextureResources.GetHandle(name);
    }

    public ResourceHandle<BufferFrameResource> Creates(string name, BufferDescriptor descriptor)
    {
        return frameGraph.AddBufferResource(new BufferFrameResource(name, descriptor));
    }

    public ResourceHandle<BufferFrameResource> Writes(ResourceHandle<BufferFrameResource> resource)
    {
        return resource;
    }

    public ResourceHandle<BufferFrameResource> Reads(ResourceHandle<BufferFrameResource> resource)
    {
        return resource;
    }

    public ResourceHandle<TextureFrameResource> Creates(string name, TextureDescriptor descriptor)
    {
        return frameGraph.AddTextureResource(new TextureFrameResource(name, descriptor));
    }

    public ResourceHandle<TextureFrameResource> Writes(ResourceHandle<TextureFrameResource> resource)
    {
        return resource;
    }

    public ResourceHandle<TextureFrameResource> Reads(ResourceHandle<TextureFrameResource> resource)
    {
        return resource;
    }
}

public struct PassNode
{
    public IFramePass Pass;
}

public struct ResourceNode
{

}