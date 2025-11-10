namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

unsafe public struct GPUSurfaceTexture : IDisposable
{
    IWGPUContext context;

    GPUTextureView? textureView;

    public GPUTextureView TextureView => textureView ?? throw new ApplicationException("TextureView is null");

    public GPUSurfaceTexture(IWGPUContext context)
    {
        this.context = context;
    }

    public void Dispose()
    {
        textureView?.Dispose();
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

        if (context.TryAcquireNextTextureView(out var view, descriptor))
        {
            textureView?.Dispose();
            textureView = view;
            return true;
        }
        return false;
    }
}