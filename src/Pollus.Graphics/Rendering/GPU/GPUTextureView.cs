using Pollus.Debugging;

namespace Pollus.Graphics.Rendering;

using Pollus.Collections;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Platform;

unsafe public struct GPUTextureView : IGPUResourceWrapper
{
    IWGPUContext? context;
    GPUTexture? texture;
    NativeHandle<TextureViewTag> native;
    TextureViewDescriptor descriptor;

    bool isRegistered;
    bool isDisposed;

    public NativeHandle<TextureViewTag> Native => native;
    public readonly TextureViewDescriptor Descriptor => descriptor;
    public readonly TextureDescriptor TextureDescriptor => texture?.Descriptor ?? default;

    public GPUTextureView()
    {
        native = NativeHandle<TextureViewTag>.Null;
    }

    public GPUTextureView(IWGPUContext context, NativeHandle<TextureTag> texture, in TextureViewDescriptor descriptor)
    {
        this.context = context;
        this.descriptor = descriptor;
        native = context.Backend.TextureCreateView(texture, in descriptor, NativeUtf8.Null);
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
        native = context.Backend.TextureCreateView(texture.Native, in this.descriptor, NativeUtf8.Null);
    }

    public GPUTextureView(IWGPUContext context, GPUTexture texture, in TextureViewDescriptor descriptor)
    {
        using var labelData = new NativeUtf8(descriptor.Label);

        this.context = context;
        this.texture = texture;
        this.descriptor = descriptor;
        native = context.Backend.TextureCreateView(texture.Native, in descriptor, labelData);
    }

    public GPUTextureView(IWGPUContext context, Silk.NET.WebGPU.TextureView* textureView, in TextureViewDescriptor descriptor)
    {
        this.context = context;
        this.native = new NativeHandle<TextureViewTag>((nint)textureView);
        this.descriptor = descriptor;
    }

    public GPUTextureView(IWGPUContext context, nint textureView, in TextureViewDescriptor descriptor)
    {
        this.context = context;
        this.native = new NativeHandle<TextureViewTag>(textureView);
        this.descriptor = descriptor;
    }

    public void Dispose()
    {
        Guard.IsNotNull(context, "GPUTextureView::Dispose: context is null");
        if (isDisposed || texture?.Disposed is true) return;
        isDisposed = true;

        if (isRegistered) context.ReleaseResource(this);
        context.Backend.TextureViewRelease(native);
    }

    public void RegisterResource()
    {
        Guard.IsNotNull(context, "GPUTextureView::RegisterResource: context is null");
        context.RegisterResource(this);
        isRegistered = true;
    }
}