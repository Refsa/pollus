namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class BufferRenderData
{
    public Handle<GPUBuffer> Buffer { get; set; }
    public uint Stride { get; set; }
}

public class BufferRenderDataLoader : IRenderDataLoader
{
    public int TargetType => TypeLookup.ID<Buffer>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var buffer = assetServer.GetAssets<Buffer>().Get(handle)
            ?? throw new InvalidOperationException("Buffer asset not found");

        var bufferData = new BufferRenderData
        {
            Buffer = renderAssets.Add(gpuContext.CreateBuffer(new()
            {
                Label = buffer.GetType().Name,
                Size = buffer.Capacity * buffer.Stride,
                Usage = BufferUsage.CopyDst | BufferUsage.Storage | BufferUsage.Vertex,
            })),
            Stride = buffer.Stride,
        };

        renderAssets.Add(handle, bufferData);
    }
}