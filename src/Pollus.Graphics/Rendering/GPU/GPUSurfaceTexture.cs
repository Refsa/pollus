namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.Platform;
using Pollus.Graphics.WGPU;

public struct GPUSurfaceTexture : IDisposable
{
    IWGPUContext context;

    NativeHandle<TextureTag> textureHandle;
    GPUTextureView? textureView;

    public GPUTextureView TextureView => textureView ?? throw new ApplicationException("TextureView is null");

    public GPUSurfaceTexture(IWGPUContext context)
    {
        this.context = context;
    }

    public void Dispose()
    {
        textureView?.Dispose();
        if (!textureHandle.IsNull)
        {
            context.Backend.TextureRelease(textureHandle);
        }
    }

    public bool Prepare()
    {
        var descriptor = new TextureViewDescriptor
        {
            Dimension = TextureViewDimension.Dimension2D,
            Format = context.GetSurfaceFormat(),
            MipLevelCount = 1,
            ArrayLayerCount = 1,
        };

        textureView?.Dispose();
        if (textureHandle.IsNull is false)
        {
            context.Backend.TextureRelease(textureHandle);
        }

        if (context.TryAcquireNextTextureView(descriptor, out var view, out var texture))
        {
            textureHandle = texture;
            textureView = view;
            return true;
        }

        return false;
    }
}