namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Debugging;
using Pollus.Utils;

public class RenderContext
{
    public GPUSurfaceTexture? SurfaceTexture;
    public GPUTextureView? SurfaceTextureView;
    public GPUCommandEncoder? CommandEncoder;
    public GPURenderPassEncoder? CurrentRenderPass;

    public bool SkipFrame { get; private set; }

    public void Begin(IWGPUContext gpuContext)
    {
        var surfaceTexture = new GPUSurfaceTexture(gpuContext);
        if (!surfaceTexture.Prepare())
        {
            Log.Error("Failed to prepare surface texture");
            surfaceTexture.Dispose();
            SkipFrame = true;
            return;
        }
        
        SurfaceTexture = surfaceTexture;
        SurfaceTextureView = surfaceTexture.TextureView;
        CommandEncoder = gpuContext.CreateCommandEncoder("""command-encoder""");
    }

    public void End(IWGPUContext gpuContext)
    {
        Guard.IsNotNull(CommandEncoder, "CommandEncoder is null");
        Guard.IsNotNull(SurfaceTexture, "SurfaceTexture is null");
        Guard.IsNotNull(SurfaceTextureView, "SurfaceTexture is null");

        {
            using var commandBuffer = CommandEncoder!.Value.Finish("""command-buffer""");
            commandBuffer.Submit();
            gpuContext.Present();
        }

        CommandEncoder.Value.Dispose();
        
        SurfaceTexture?.Dispose();
        SurfaceTexture = null;
        SurfaceTextureView = null;

        CommandEncoder = null;
    }

    public GPURenderPassEncoder BeginRenderPass(LoadOp loadOp = LoadOp.Clear, StoreOp storeOp = StoreOp.Store, Color? clearColor = null)
    {
        Guard.IsNull(CurrentRenderPass, "CurrentRenderPass is not null");
        Guard.IsNotNull(SurfaceTextureView, "SurfaceTextureView is null");
        Guard.IsNotNull(CommandEncoder, "CommandEncoder is null");

        CurrentRenderPass = CommandEncoder.Value.BeginRenderPass(new()
        {
            Label = """RenderPass""",
            ColorAttachments = stackalloc RenderPassColorAttachment[1]
            {
                new RenderPassColorAttachment()
                {
                    View = SurfaceTextureView.Value.Native,
                    LoadOp = loadOp,
                    StoreOp = storeOp,
                    ClearValue = clearColor ?? new Color.HSV(0.1f, 1f, 0.1f),
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
