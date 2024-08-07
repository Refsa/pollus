namespace Pollus.Graphics.WGPU;

using System.Diagnostics;
using Silk.NET.WebGPU;

unsafe public struct WGPUSurfaceTexture : IDisposable
{
    IWGPUContext context;

#if BROWSER
    WGPUTextureView? currentTextureView;
#else
    SurfaceTexture? currentSurfaceTexture;
#endif

    public WGPUSurfaceTexture(IWGPUContext context)
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
    public WGPUTextureView? GetTextureView()
    {
        if (currentTextureView != null)
        {
            return currentTextureView;
        }

        var native = context.wgpu.SwapChainGetCurrentTextureView(context.SwapChain);
        if (native == null) throw new ApplicationException("Failed to get current texture view");
        
        currentTextureView = new WGPUTextureView(context, native);
        return currentTextureView;
    }
#else
    SurfaceTexture? GetSurfaceTexture()
    {
        if (currentSurfaceTexture is SurfaceTexture current)
        {
            switch (current.Status)
            {
                case SurfaceGetCurrentTextureStatus.Success:
                    return current;
                case SurfaceGetCurrentTextureStatus.Timeout or
                     SurfaceGetCurrentTextureStatus.Outdated or
                     SurfaceGetCurrentTextureStatus.Lost:
                    {
                        if (current.Texture != null) Dispose();
                        return null;
                    }
                case SurfaceGetCurrentTextureStatus.OutOfMemory or
                     SurfaceGetCurrentTextureStatus.DeviceLost or
                     SurfaceGetCurrentTextureStatus.Force32:
                    {
                        throw new ApplicationException("SurfaceTexture Panic Status: " + current.Status);
                    }
            }
        }

        var surfaceTexture = new SurfaceTexture();
        context.wgpu.SurfaceGetCurrentTexture(context.Surface, ref surfaceTexture);
        currentSurfaceTexture = surfaceTexture;
        return surfaceTexture;
    }

    public WGPUTextureView? GetTextureView()
    {
        if (GetSurfaceTexture() is SurfaceTexture surfaceTexture)
        {
            return new WGPUTextureView(context, surfaceTexture.Texture);
        }
        return null;
    }
#endif
}