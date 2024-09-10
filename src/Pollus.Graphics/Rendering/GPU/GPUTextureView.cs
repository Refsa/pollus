namespace Pollus.Graphics.Rendering;

using ImGuiNET;
using Pollus.Collections;
using Pollus.Graphics.WGPU;

unsafe public struct GPUTextureView : IGPUResourceWrapper
{
    IWGPUContext context;
    GPUTexture? texture;
    Silk.NET.WebGPU.TextureView* textureView;
    TextureViewDescriptor descriptor;

    bool isRegistered;
    bool isDisposed;

    public nint Native => (nint)textureView;
    public readonly TextureViewDescriptor Descriptor => descriptor;
    public readonly TextureDescriptor TextureDescriptor => texture?.Descriptor ?? default;

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.Texture* texture, TextureViewDescriptor descriptor)
    {
        this.context = context;
        this.descriptor = descriptor;
        textureView = context.wgpu.TextureCreateView(texture, null);
    }

    public GPUTextureView(IWGPUContext context, GPUTexture texture)
    {
        this.context = context;
        this.texture = texture;
        this.descriptor = new()
        {
            MipLevelCount = texture.Descriptor.MipLevelCount,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Dimension = texture.Descriptor.Dimension switch
            {
                TextureDimension.Dimension1D => TextureViewDimension.Dimension1D,
                TextureDimension.Dimension2D => TextureViewDimension.Dimension2D,
                TextureDimension.Dimension3D => TextureViewDimension.Dimension3D,
                _ => throw new NotSupportedException("Unsupported texture dimension for view")
            },
            BaseMipLevel = 0,
            Format = texture.Descriptor.Format,
        };
        textureView = context.wgpu.TextureCreateView((Silk.NET.WebGPU.Texture*)texture.Native, null);
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

        this.texture = texture;
        textureView = context.wgpu.TextureCreateView((Silk.NET.WebGPU.Texture*)texture.Native, nativeDescriptor);
    }

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.TextureView* textureView, TextureViewDescriptor descriptor)
    {
        this.context = context;
        this.textureView = textureView;
        this.descriptor = descriptor;
    }

    public void Dispose()
    {
        if (isDisposed || texture?.Disposed is true) return;
        isDisposed = true;

        if (isRegistered) context.ReleaseResource(this);
        context.wgpu.TextureViewRelease(textureView);
    }

    public void RegisterResource()
    {
        context.RegisterResource(this);
        isRegistered = true;
    }
}