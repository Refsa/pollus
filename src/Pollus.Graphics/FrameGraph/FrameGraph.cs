namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class FrameGraph
{
    IWGPUContext gpuContext;
    GPUCommandEncoder rendering;
    GPUCommandEncoder compute;

    Dictionary<int, IFrameResource> resources = new();
    Dictionary<int, IFramePass> passes = new();

    public FrameGraph(IWGPUContext gpuContext)
    {
        this.gpuContext = gpuContext;
    }

    public void BeginFrame()
    {
        rendering = gpuContext.CreateCommandEncoder("""rendering-command-encoder""");
        compute = gpuContext.CreateCommandEncoder("""compute-command-encoder""");
    }

    public void EndFrame()
    {
        using var renderingCommandBuffer = rendering.Finish("""rendering-command-encoder""");
        using var computeCommandBuffer = compute.Finish("""compute-command-encoder""");

        renderingCommandBuffer.Submit();
        computeCommandBuffer.Submit();

        gpuContext.Present();

        rendering.Dispose();
        compute.Dispose();

        passes.Clear();
    }
}

public interface IFramePass
{
    string Name { get; }

    List<FrameResource> Inputs { get; }
    List<FrameResource> Outputs { get; }
}

public class FramePass<TData> : IFramePass
{
    public delegate void Execute(TData passData);

    public string Name { get; }
    public TData Data { get; }
    public Execute? ExecutePass { get; private set; }

    public List<FrameResource> Inputs { get; } = new();
    public List<FrameResource> Outputs { get; } = new();

    public FramePass(string name, TData data)
    {
        Name = name;
        Data = data;
    }

    public FramePass<TData> AddTextureInput(TextureFrameGraphResource resource)
    {
        Inputs.Add(resource);
        return this;
    }

    public FramePass<TData> AddTextureOutput(TextureFrameGraphResource resource)
    {
        Outputs.Add(resource);
        return this;
    }

    public FramePass<TData> AddBufferInput(BufferFrameGraphResource resource)
    {
        Inputs.Add(resource);
        return this;
    }

    public FramePass<TData> AddBufferOutput(BufferFrameGraphResource resource)
    {
        Outputs.Add(resource);
        return this;
    }

    public FramePass<TData> SetExecute(Execute execute)
    {
        ExecutePass = execute;
        return this;
    }
}
