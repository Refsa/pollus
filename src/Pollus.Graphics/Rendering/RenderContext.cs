namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Debugging;

public class RenderContext
{
    List<GPUCommandEncoder> commandEncoders = new();
    List<GPUCommandBuffer> commandBuffers = new();

    GPUSurfaceTexture? surfaceTexture;
    public GPUTextureView? SurfaceTextureView;

    public required IWGPUContext GPUContext { get; init; }
    public bool SkipFrame { get; private set; }

    public bool PrepareFrame()
    {
        var surfaceTexture = new GPUSurfaceTexture(GPUContext);
        if (!surfaceTexture.Prepare())
        {
            Log.Error("Failed to prepare surface texture");
            surfaceTexture.Dispose();
            SkipFrame = true;
            return false;
        }

        this.surfaceTexture = surfaceTexture;
        SurfaceTextureView = surfaceTexture.TextureView;
        return true;
    }

    public void PrepareResources(ResourceContainers resourceContainers)
    {

    }

    public void CleanupFrame()
    {
        foreach (var buffer in commandBuffers)
        {
            buffer.Dispose();
        }
        foreach (var encoder in commandEncoders)
        {
            encoder.Dispose();
        }
        surfaceTexture?.Dispose();

        commandEncoders.Clear();
        commandBuffers.Clear();
        surfaceTexture = null;
        SurfaceTextureView = null;
    }

    unsafe public void EndFrame()
    {
        Guard.IsNotNull(surfaceTexture, "SurfaceTexture is null");
        Guard.IsNotNull(SurfaceTextureView, "SurfaceTexture is null");

        {
            var commandBuffers = stackalloc Silk.NET.WebGPU.CommandBuffer*[commandEncoders.Count];
            for (int i = 0; i < commandEncoders.Count; i++)
            {
                var commandBuffer = commandEncoders[i].Finish(commandEncoders[i].Label + "-CommandBuffer");
                this.commandBuffers.Add(commandBuffer);
                commandBuffers[i] = (Silk.NET.WebGPU.CommandBuffer*)commandBuffer.Native;
            }
            GPUContext.wgpu.QueueSubmit(GPUContext.Queue, (uint)commandEncoders.Count, commandBuffers);
        }

        GPUContext.Present();
        CleanupFrame();
    }

    public GPUCommandEncoder CreateCommandEncoder(string label)
    {
        var encoder = GPUContext.CreateCommandEncoder(label);
        commandEncoders.Add(encoder);
        return encoder;
    }

    public GPUCommandEncoder GetCommandEncoder(string label)
    {
        for (int i = 0; i < commandEncoders.Count; i++)
        {
            if (commandEncoders[i].Label == label)
            {
                return commandEncoders[i];
            }
        }
        return CreateCommandEncoder(label);
    }
    
    public GPUCommandEncoder GetCurrentCommandEncoder()
    {
        return commandEncoders[^1];
    }

    unsafe public void SubmitCommandBuffers(in GPUCommandBuffer buffer0)
    {
        var buffers = stackalloc Silk.NET.WebGPU.CommandBuffer*[]
        {
            (Silk.NET.WebGPU.CommandBuffer*)buffer0.Native,
        };
        GPUContext.wgpu.QueueSubmit(GPUContext.Queue, 1, buffers);
    }
}