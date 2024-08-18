namespace Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Debug;

public class RenderContext
{
    public GPUSurfaceTexture? SurfaceTexture;
    public GPUTextureView? SurfaceTextureView;
    public GPUCommandEncoder? CommandEncoder;

    public void Begin(IWGPUContext gpuContext)
    {
        SurfaceTexture = gpuContext.CreateSurfaceTexture();
        if (SurfaceTexture?.GetTextureView() is not GPUTextureView surfaceTextureView)
        {
            Log.Error("Surface texture view is null");
            SurfaceTexture = null;
            return;
        }

        SurfaceTextureView = surfaceTextureView;
        CommandEncoder = gpuContext.CreateCommandEncoder("""command-encoder""");
    }

    public void End(IWGPUContext gpuContext)
    {
        {
            using var commandBuffer = CommandEncoder!.Value.Finish("""command-buffer""");
            commandBuffer.Submit();
            gpuContext.Present();
        }

        CommandEncoder?.Dispose();
        SurfaceTexture?.Dispose();

        CommandEncoder = null;
        SurfaceTexture = null;
    }
}
