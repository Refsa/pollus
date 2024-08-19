namespace Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Debug;

public class RenderContext
{
    public GPUSurfaceTexture? SurfaceTexture;
    public GPUTextureView? SurfaceTextureView;
    public GPUCommandEncoder? CommandEncoder;
    public GPURenderPassEncoder? CurrentRenderPass;

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

    public GPURenderPassEncoder BeginRenderPass()
    {
        Guard.IsNull(CurrentRenderPass, "CurrentRenderPass is not null");
        Guard.IsNotNull(SurfaceTextureView, "SurfaceTextureView is null");
        Guard.IsNotNull(CommandEncoder, "CommandEncoder is null");

        CurrentRenderPass = CommandEncoder.Value.BeginRenderPass(new()
        {
            Label = """RenderPass""",
            ColorAttachments = new[]
            {
                new RenderPassColorAttachment()
                {
                    View = SurfaceTextureView.Value.Native,
                    LoadOp = LoadOp.Clear,
                    StoreOp = StoreOp.Store,
                    ClearValue = new(0.15f, 0.125f, 0.1f, 1.0f),
                },
            },
        });
        
        return CurrentRenderPass.Value;
    }

    public void EndRenderPass()
    {
        Guard.IsNotNull(CurrentRenderPass, "CurrentRenderPass is null");
        CurrentRenderPass.Value.End();
        CurrentRenderPass.Value.Dispose();
        CurrentRenderPass = null;
    }
}
