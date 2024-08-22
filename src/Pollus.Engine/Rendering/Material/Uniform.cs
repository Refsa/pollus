namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class UniformAsset<T>
    where T : unmanaged
{
    public T Value { get; set; }

    public UniformAsset(T value)
    {
        Value = value;
    }
}

public class UniformRenderData : IRenderData
{
    public required GPUBuffer UniformBuffer { get; init; }

    public void Dispose()
    {
        UniformBuffer.Dispose();
    }

    public void WriteBuffer<T>(T value)
        where T : unmanaged
    {
        UniformBuffer.Write(value, 0);
    }
}

public class UniformRenderDataLoader<T> : IRenderDataLoader
    where T : unmanaged
{
    public int TargetType => AssetLookup.ID<UniformAsset<T>>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var uniformAsset = assetServer.GetAssets<UniformAsset<T>>().Get(handle)
            ?? throw new InvalidOperationException("Uniform asset not found");

        var uniformBuffer = gpuContext.CreateBuffer(BufferDescriptor.Uniform<T>(nameof(T)));

        renderAssets.Add(handle, new UniformRenderData
        {
            UniformBuffer = uniformBuffer,
        });
    }
}