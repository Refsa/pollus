using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

using var graphicsContext = new GraphicsContext();

using var window = new Window(new());
using var windowContext = graphicsContext.CreateContext("main", window);

Span<WGPURenderPassColorAttachment> colorAttachments = stackalloc WGPURenderPassColorAttachment[1];
while (window.IsOpen)
{
    window.PollEvents();

    {
        using var surfaceTexture = windowContext.CreateSurfaceTexture();
        if (surfaceTexture.GetTextureView() is not WGPUTextureView surfaceTextureView)
        {
            continue;
        }

        using var commandEncoder = windowContext.CreateCommandEncoder("command-encoder");
        {
            colorAttachments[0] = new(
                textureView: surfaceTextureView.Native,
                resolveTarget: nint.Zero,
                clearValue: new(0.2f, 0.1f, 0.01f, 1.0f),
                loadOp: Silk.NET.WebGPU.LoadOp.Clear,
                storeOp: Silk.NET.WebGPU.StoreOp.Store
            );
            using var renderPass = commandEncoder.BeginRenderPass(new()
            {
                Label = "render-pass",
                ColorAttachments = colorAttachments,
            });
            renderPass.End();
        }
        using var commandBuffer = commandEncoder.Finish("command-buffer");
        commandBuffer.Submit();
        windowContext.Present();
    }

    Thread.Sleep(8);
}