namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

unsafe public struct WGPUTextureView : IDisposable
{
    WGPUContext context;
    TextureView* textureView;

    public nint Native => (nint)textureView;

    public WGPUTextureView(WGPUContext context, Texture* texture, TextureViewDescriptor? descriptor = null)
    {
        this.context = context;
        if (descriptor is TextureViewDescriptor descriptorValue)
        {
            textureView = context.wgpu.TextureCreateView(texture, descriptorValue);
        }
        else
        {
            textureView = context.wgpu.TextureCreateView(texture, null);
        }
    }

    public WGPUTextureView(WGPUContext context, TextureView* textureView)
    {
        this.context = context;
        this.textureView = textureView;
    }

    public void Dispose()
    {
        context.wgpu.TextureViewRelease(textureView);
    }
}