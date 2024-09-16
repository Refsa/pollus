namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class StorageBufferRenderData
{
    public Handle<GPUBuffer> Buffer { get; set; }
    public uint Stride { get; set; }
}

public class StorageBufferRenderDataLoader : IRenderDataLoader
{
    public int TargetType => TypeLookup.ID<StorageBuffer>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var buffer = assetServer.GetAssets<StorageBuffer>().Get(handle)
            ?? throw new InvalidOperationException("Buffer asset not found");

        var bufferData = new StorageBufferRenderData
        {
            Buffer = renderAssets.Add(gpuContext.CreateBuffer(new()
            {
                Label = buffer.GetType().Name,
                Size = buffer.SizeInBytes,
                Usage = buffer.Usage,
            })),
            Stride = buffer.Stride,
        };

        renderAssets.Add(handle, bufferData);
    }
}