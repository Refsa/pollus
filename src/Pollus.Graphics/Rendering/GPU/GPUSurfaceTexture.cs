namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

unsafe public struct GPUSurfaceTexture : IDisposable
{
    IWGPUContext context;

    GPUTextureView? textureView;
#if !BROWSER
    Silk.NET.WebGPU.SurfaceTexture? surfaceTexture;
#endif

    public GPUTextureView TextureView => textureView ?? throw new ApplicationException("TextureView is null");

    public GPUSurfaceTexture(IWGPUContext context)
    {
        this.context = context;
    }

    public void Dispose()
    {
#if !BROWSER
        if (surfaceTexture.HasValue)
        {
            context.ReleaseSurfaceTexture(surfaceTexture.Value);
        }
#endif

        textureView?.Dispose();
    }

    public bool Prepare()
    {
#if BROWSER
        var native = context.wgpu.SwapChainGetCurrentTextureView(context.SwapChain);
        if (native == null) throw new ApplicationException("Failed to get current texture view");
        textureView = new GPUTextureView(context, native);
        return true;
#else
        surfaceTexture = context.SurfaceGetCurrentTexture();
        textureView = context.CreateTextureView(surfaceTexture.Value, new());
        return CheckSurface(surfaceTexture.Value.Status);
#endif
    }

    bool CheckSurface(Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus status)
    {
        switch (status)
        {
            case Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Success:
                return true;
            case Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Timeout or
                 Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Outdated or
                 Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Lost:
                return false;
            case Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.OutOfMemory or
                 Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.DeviceLost or
                 Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Force32:
                {
                    throw new ApplicationException("SurfaceTexture Panic Status: " + status);
                }
        }

        return true;
    }
}