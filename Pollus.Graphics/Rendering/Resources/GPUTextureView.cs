namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

unsafe public struct GPUTextureView : IDisposable
{
    IWGPUContext context;
    Silk.NET.WebGPU.TextureView* textureView;

    public nint Native => (nint)textureView;

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.Texture* texture, Silk.NET.WebGPU.TextureViewDescriptor? descriptor = null)
    {
        this.context = context;
        if (descriptor is Silk.NET.WebGPU.TextureViewDescriptor descriptorValue)
        {
            textureView = context.wgpu.TextureCreateView(texture, descriptorValue);
        }
        else
        {
            textureView = context.wgpu.TextureCreateView(texture, null);
        }
    }

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.TextureView* textureView)
    {
        this.context = context;
        this.textureView = textureView;
    }

    public void Dispose()
    {
        context.wgpu.TextureViewRelease(textureView);
    }
}