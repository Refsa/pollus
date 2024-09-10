namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class UniformAsset<T>
    where T : unmanaged, IShaderType
{
    T value;

    public ref T Value => ref value;

    public UniformAsset(T value)
    {
        this.value = value;
    }
}

public class UniformRenderData
{
    public required Handle<GPUBuffer> UniformBuffer { get; init; }
}

public class UniformRenderDataLoader<T> : IRenderDataLoader
    where T : unmanaged, IShaderType
{
    public int TargetType => TypeLookup.ID<UniformAsset<T>>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var uniformAsset = assetServer.GetAssets<UniformAsset<T>>().Get(handle)
            ?? throw new InvalidOperationException("Uniform asset not found");

        var uniformBuffer = gpuContext.CreateBuffer(BufferDescriptor.Uniform<T>(nameof(T)));

        renderAssets.Add(handle, new UniformRenderData
        {
            UniformBuffer = renderAssets.Add(uniformBuffer),
        });
    }
}