namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU;

unsafe public struct GPUTextureView : IGPUResourceWrapper
{
    IWGPUContext context;
    Silk.NET.WebGPU.TextureView* textureView;

    public nint Native => (nint)textureView;

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.Texture* texture)
    {
        this.context = context;
        textureView = context.wgpu.TextureCreateView(texture, null);
    }

    public GPUTextureView(IWGPUContext context, GPUTexture texture, TextureViewDescriptor descriptor)
    {
        this.context = context;
        var nativeDescriptor = new Silk.NET.WebGPU.TextureViewDescriptor(
            label: (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Label)),
            format: descriptor.Format,
            dimension: descriptor.Dimension,
            baseMipLevel: descriptor.BaseMipLevel,
            mipLevelCount: descriptor.MipLevelCount,
            baseArrayLayer: descriptor.BaseArrayLayer,
            arrayLayerCount: descriptor.ArrayLayerCount,
            aspect: descriptor.Aspect
        );

        textureView = context.wgpu.TextureCreateView((Silk.NET.WebGPU.Texture*)texture.Native, nativeDescriptor);
    }

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.TextureView* textureView)
    {
        this.context = context;
        this.textureView = textureView;
    }

    public void RegisterResource()
    {
        context.RegisterResource(this);
    }

    public void Dispose()
    {
        context.ReleaseResource(this);
        context.wgpu.TextureViewRelease(textureView);
    }
}