namespace Pollus.Engine.Rendering;

using Pollus.Assets;
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
    public TypeID TargetType => TypeLookup.ID<StorageBuffer>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var buffer = assetServer.GetAssets<StorageBuffer>().Get(handle)
            ?? throw new InvalidOperationException("Buffer asset not found");

        var gpuBuffer = gpuContext.CreateBuffer(new()
        {
            Label = buffer.GetType().Name,
            Size = buffer.SizeInBytes,
            Usage = buffer.Usage,
        });
        var bufferData = new StorageBufferRenderData
        {
            Buffer = renderAssets.Add(gpuBuffer),
            Stride = buffer.Stride,
        };
        buffer.WriteTo(gpuBuffer, 0);

        renderAssets.Add(handle, bufferData);
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var buffer = renderAssets.Get<StorageBufferRenderData>(handle);
        renderAssets.Unload(buffer.Buffer);
    }
}