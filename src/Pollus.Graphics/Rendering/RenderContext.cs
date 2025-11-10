namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Debugging;
using Pollus.Graphics.Platform;

public class RenderContext
{
    List<GPUCommandEncoder> commandEncoders = new();
    List<GPUCommandBuffer> commandBuffers = new();
    RenderResourceCache resources = new();

    GPUSurfaceTexture? surfaceTexture;
    public GPUTextureView? SurfaceTextureView;

    public required IWGPUContext GPUContext { get; init; }
    public RenderResourceCache Resources => resources;
    public bool SkipFrame { get; private set; }

    public bool PrepareFrame()
    {
        var surfaceTexture = GPUContext.CreateSurfaceTexture();
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

    public void PrepareResources(ref ResourceContainers resourceContainers)
    {
        foreach (var resource in resourceContainers.Textures.Resources)
        {
            if (resources.Has(resource.Handle)) continue;

            var texture = GPUContext.CreateTexture(resource.Descriptor);
            var textureView = GPUContext.CreateTextureView(texture, TextureViewDescriptor.D2 with
            {
                Label = resource.Descriptor.Label,
                Format = resource.Descriptor.Format,
                MipLevelCount = resource.Descriptor.MipLevelCount,
                ArrayLayerCount = resource.Descriptor.Size.DepthOrArrayLayers,
                Dimension = resource.Descriptor.Dimension switch
                {
                    TextureDimension.Dimension1D => TextureViewDimension.Dimension1D,
                    TextureDimension.Dimension2D => TextureViewDimension.Dimension2D,
                    TextureDimension.Dimension3D => TextureViewDimension.Dimension3D,
                    _ => throw new NotImplementedException(),
                },
            });
            resources.SetTexture(resource.Handle, new TextureGPUResource(texture, textureView, resource.Descriptor));
        }

        foreach (var resource in resourceContainers.Buffers.Resources)
        {
            if (resources.Has(resource.Handle)) continue;

            var buffer = GPUContext.CreateBuffer(resource.Descriptor);
            resources.SetBuffer(resource.Handle, new BufferGPUResource(buffer, resource.Descriptor));
        }
    }

    public void CleanupFrame()
    {
        foreach (var buffer in commandBuffers) buffer.Dispose();
        foreach (var encoder in commandEncoders) encoder.Dispose();

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
            Span<NativeHandle<CommandBufferTag>> commandBufferHandles = stackalloc NativeHandle<CommandBufferTag>[commandEncoders.Count];
            for (int i = 0; i < commandEncoders.Count; i++)
            {
                var commandBuffer = commandEncoders[i].Finish("");
                commandBuffers.Add(commandBuffer);
                commandBufferHandles[i] = commandBuffer.Native;
            }
            GPUContext.Backend.QueueSubmit(GPUContext.QueueHandle, commandBufferHandles);
        }

        GPUContext.Present();
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
}