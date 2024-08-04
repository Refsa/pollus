namespace Pollus.Graphics.WGPU;

using Pollus.Mathematics;
using Silk.NET.WebGPU;

unsafe public struct WGPUSurfaceTexture : IDisposable
{
    WGPUContext context;
    SurfaceTexture? currentSurfaceTexture;

    public WGPUSurfaceTexture(WGPUContext context)
    {
        this.context = context;
    }

    public void Dispose()
    {
        if (currentSurfaceTexture is not null)
        {
            context.wgpu.TextureRelease(currentSurfaceTexture.Value.Texture);
            currentSurfaceTexture = null;
        }
    }

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
        context.wgpu.SurfaceGetCurrentTexture(context.surface, ref surfaceTexture);
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
}