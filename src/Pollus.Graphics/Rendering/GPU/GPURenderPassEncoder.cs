namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;

unsafe public struct GPURenderPassEncoder : IDisposable
{
    IWGPUContext context;
    Silk.NET.WebGPU.RenderPassEncoder* native;
    public nint Native => (nint)native;

#if BROWSER
    public GPURenderPassEncoder(IWGPUContext context, Silk.NET.WebGPU.CommandEncoder* commandEncoder, Emscripten.WGPURenderPassDescriptor_Browser descriptor)
    {
        this.context = context;
        native = context.wgpu.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }
#else
    public GPURenderPassEncoder(IWGPUContext context, Silk.NET.WebGPU.CommandEncoder* commandEncoder, Silk.NET.WebGPU.RenderPassDescriptor descriptor)
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

    public void SetPipeline(GPURenderPipeline pipeline)
    {
        context.wgpu.RenderPassEncoderSetPipeline(native, (Silk.NET.WebGPU.RenderPipeline*)pipeline.Native);
    }

    public void SetBindGroup(GPUBindGroup bindGroup, uint groupIndex, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
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

    public void SetVertexBuffer(uint slot, GPUBuffer buffer)
    {
        context.wgpu.RenderPassEncoderSetVertexBuffer(native, slot, (Silk.NET.WebGPU.Buffer*)buffer.Native, 0, buffer.Size);
    }

    public void SetIndexBuffer(GPUBuffer indexBuffer, IndexFormat format)
    {
        context.wgpu.RenderPassEncoderSetIndexBuffer(native, (Silk.NET.WebGPU.Buffer*)indexBuffer.Native, (Silk.NET.WebGPU.IndexFormat)format, 0, indexBuffer.Size);
    }

    public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        context.wgpu.RenderPassEncoderDraw(native, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndirect(GPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.wgpu.RenderPassEncoderDrawIndirect(native, (Silk.NET.WebGPU.Buffer*)indirectBuffer.Native, indirectOffset);
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        context.wgpu.RenderPassEncoderDrawIndexed(native, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void DrawIndexedIndirect(GPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.wgpu.RenderPassEncoderDrawIndexedIndirect(native, (Silk.NET.WebGPU.Buffer*)indirectBuffer.Native, indirectOffset);
    }
}
