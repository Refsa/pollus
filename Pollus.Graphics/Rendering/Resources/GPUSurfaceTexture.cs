namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

unsafe public struct GPUSurfaceTexture : IDisposable
{
    IWGPUContext context;

#if BROWSER
    GPUTextureView? currentTextureView;
#else
    Silk.NET.WebGPU.SurfaceTexture? currentSurfaceTexture;
#endif

    public GPUSurfaceTexture(IWGPUContext context)
    {
        this.context = context;
    }

    public void Dispose()
    {
#if BROWSER
        if (currentTextureView != null)
        {
            currentTextureView.Value.Dispose();
            currentTextureView = null;
        }
#else
        if (currentSurfaceTexture is not null)
        {
            context.wgpu.TextureRelease(currentSurfaceTexture.Value.Texture);
            currentSurfaceTexture = null;
        }
#endif
    }

#if BROWSER
    public GPUTextureView? GetTextureView()
    {
        if (currentTextureView != null)
        {
            return currentTextureView;
        }

        var native = context.wgpu.SwapChainGetCurrentTextureView(context.SwapChain);
        if (native == null) throw new ApplicationException("Failed to get current texture view");
        
        currentTextureView = new GPUTextureView(context, native);
        return currentTextureView;
    }
#else
    Silk.NET.WebGPU.SurfaceTexture? GetSurfaceTexture()
    {
        if (currentSurfaceTexture is Silk.NET.WebGPU.SurfaceTexture current)
        {
            switch (current.Status)
            {
                case Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Success:
                    return current;
                case Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Timeout or
                     Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Outdated or
                     Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Lost:
                    {
                        if (current.Texture != null) Dispose();
                        return null;
                    }
                case Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.OutOfMemory or
                     Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.DeviceLost or
                     Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Force32:
                    {
                        throw new ApplicationException("SurfaceTexture Panic Status: " + current.Status);
                    }
            }
        }

        var surfaceTexture = new Silk.NET.WebGPU.SurfaceTexture();
        context.wgpu.SurfaceGetCurrentTexture(context.Surface, ref surfaceTexture);
        currentSurfaceTexture = surfaceTexture;
        return surfaceTexture;
    }

    public GPUTextureView? GetTextureView()
    {
        if (GetSurfaceTexture() is Silk.NET.WebGPU.SurfaceTexture surfaceTexture)
        {
            return new GPUTextureView(context, surfaceTexture.Texture);
        }
        return null;
    }
#endif
}