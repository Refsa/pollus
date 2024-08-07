using Pollus.Utils;

namespace Pollus.Graphics.WGPU;

unsafe public struct WGPURenderPassEncoder : IDisposable
{
    WGPUContext context;
    Silk.NET.WebGPU.RenderPassEncoder* native;
    public nint Native => (nint)native;

#if NET8_0_BROWSER
    public WGPURenderPassEncoder(WGPUContext context, Silk.NET.WebGPU.CommandEncoder* commandEncoder, WGPURenderPassDescriptor_Browser descriptor)
    {
        this.context = context;
        using var descriptorPtr = TemporaryPin.Pin(descriptor);
        native = context.wgpu.CommandEncoderBeginRenderPass(commandEncoder, (WGPURenderPassDescriptor_Browser*)descriptorPtr.Ptr);
    }
#else
    public WGPURenderPassEncoder(WGPUContext context, Silk.NET.WebGPU.CommandEncoder* commandEncoder, Silk.NET.WebGPU.RenderPassDescriptor descriptor)
    {
        this.context = context;
        native = context.wgpu.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }
#endif

    public void Dispose()
    {
        context.wgpu.RenderPassEncoderRelease(native);
    }


    public void End()
    {
        context.wgpu.RenderPassEncoderEnd(native);
    }

    public void SetPipeline(WGPURenderPipeline pipeline)
    {
        context.wgpu.RenderPassEncoderSetPipeline(native, (Silk.NET.WebGPU.RenderPipeline*)pipeline.Native);
    }

    public void SetBindGroup(WGPUBindGroup bindGroup, uint groupIndex, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
    {
        if (dynamicOffsetCount > 0)
        {
            context.wgpu.RenderPassEncoderSetBindGroup(native, groupIndex, (Silk.NET.WebGPU.BindGroup*)bindGroup.Native, dynamicOffsetCount, &dynamicOffsets);
        }
        else
        {
            context.wgpu.RenderPassEncoderSetBindGroup(native, groupIndex, (Silk.NET.WebGPU.BindGroup*)bindGroup.Native, 0, null);
        }
    }

    public void SetVertexBuffer(uint slot, WGPUBuffer buffer)
    {
        context.wgpu.RenderPassEncoderSetVertexBuffer(native, slot, (Silk.NET.WebGPU.Buffer*)buffer.Native, 0, buffer.Size);
    }

    public void SetIndexBuffer(WGPUBuffer indexBuffer, Silk.NET.WebGPU.IndexFormat format)
    {
        context.wgpu.RenderPassEncoderSetIndexBuffer(native, (Silk.NET.WebGPU.Buffer*)indexBuffer.Native, format, 0, indexBuffer.Size);
    }

    public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        context.wgpu.RenderPassEncoderDraw(native, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndirect(WGPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.wgpu.RenderPassEncoderDrawIndirect(native, (Silk.NET.WebGPU.Buffer*)indirectBuffer.Native, indirectOffset);
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        context.wgpu.RenderPassEncoderDrawIndexed(native, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void DrawIndexedIndirect(WGPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.wgpu.RenderPassEncoderDrawIndexedIndirect(native, (Silk.NET.WebGPU.Buffer*)indirectBuffer.Native, indirectOffset);
    }
}
