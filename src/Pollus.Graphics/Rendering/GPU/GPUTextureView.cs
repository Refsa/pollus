namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public struct GPUTextureView : IGPUResourceWrapper
{
    IWGPUContext context;
    Silk.NET.WebGPU.TextureView* textureView;
    bool isRegistered;

    public nint Native => (nint)textureView;

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.Texture* texture)
    {
        this.context = context;
        textureView = context.wgpu.TextureCreateView(texture, null);
    }

    public GPUTextureView(IWGPUContext context, GPUTexture texture, TextureViewDescriptor descriptor)
    {
        using var labelData = new NativeUtf8(descriptor.Label);

        this.context = context;
        var nativeDescriptor = new Silk.NET.WebGPU.TextureViewDescriptor(
            label: labelData.Pointer,
            format: (Silk.NET.WebGPU.TextureFormat)descriptor.Format,
            dimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Dimension,
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
        isRegistered = true;
    }

    public void Dispose()
    {
        if (isRegistered) context.ReleaseResource(this);
        context.wgpu.TextureViewRelease(textureView);
    }
}