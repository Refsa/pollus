namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class UniformRenderData
{
    public required Handle<GPUBuffer> UniformBuffer { get; init; }
}

public class UniformRenderDataLoader<T> : IRenderDataLoader
    where T : unmanaged, IShaderType
{
    public TypeID TargetType => TypeLookup.ID<Uniform<T>>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var uniform = assetServer.GetAssets<Uniform<T>>().Get(handle)
            ?? throw new InvalidOperationException("Uniform asset not found");

        var uniformBuffer = gpuContext.CreateBuffer(BufferDescriptor.Uniform<T>(nameof(T)));

        renderAssets.Add(handle, new UniformRenderData
        {
            UniformBuffer = renderAssets.Add(uniformBuffer),
        });
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var uniform = renderAssets.Get<UniformRenderData>(handle);
        renderAssets.Unload(uniform.UniformBuffer);
    }
}