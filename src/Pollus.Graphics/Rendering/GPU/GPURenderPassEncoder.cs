namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Graphics.Platform;

unsafe public readonly struct GPURenderPassEncoder : IDisposable
{
    readonly IWGPUContext context;
    readonly NativeHandle<RenderPassEncoderTag> native;
    public NativeHandle<RenderPassEncoderTag> Native => native;

    public GPURenderPassEncoder(IWGPUContext context, in GPUCommandEncoder encoder, in RenderPassDescriptor descriptor)
    {
        this.context = context;
        native = context.Backend.CommandEncoderBeginRenderPass(encoder.Native, in descriptor);
    }

    public void Dispose()
    {
        context.Backend.RenderPassEncoderEnd(native);
    }

    public void SetPipeline(GPURenderPipeline pipeline)
    {
        context.Backend.RenderPassEncoderSetPipeline(native, pipeline.Native);
    }

    public void SetViewport(Vec2f pos, Vec2f size, float minDepth, float maxDepth)
    {
        context.Backend.RenderPassEncoderSetViewport(native, pos.X, pos.Y, size.X, size.Y, minDepth, maxDepth);
    }

    public void SetScissorRect(uint x, uint y, uint width, uint height)
    {
        context.Backend.RenderPassEncoderSetScissorRect(native, x, y, width, height);
    }

    public void SetBlendConstant(Vec4<double> color)
    {
        context.Backend.RenderPassEncoderSetBlendConstant(native, color.X, color.Y, color.Z, color.W);
    }

    public void SetBindGroup(uint groupIndex, GPUBindGroup bindGroup, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
    {
        if (dynamicOffsetCount > 0)
        {
            var dynamicOffsetsSpan = new ReadOnlySpan<uint>(&dynamicOffsets, (int)dynamicOffsetCount);
            context.Backend.RenderPassEncoderSetBindGroup(native, groupIndex, bindGroup.Native, dynamicOffsetsSpan);
        }
        else
        {
            context.Backend.RenderPassEncoderSetBindGroup(native, groupIndex, bindGroup.Native, ReadOnlySpan<uint>.Empty);
        }
    }

    public void SetVertexBuffer(uint slot, GPUBuffer buffer, ulong? offset = null, ulong? length = null)
    {
        context.Backend.RenderPassEncoderSetVertexBuffer(native, slot, buffer.Native, offset ?? 0, length ?? buffer.Size);
    }

    public void SetIndexBuffer(GPUBuffer indexBuffer, IndexFormat format, ulong? offset = null, ulong? length = null)
    {
        context.Backend.RenderPassEncoderSetIndexBuffer(native, indexBuffer.Native, format, offset ?? 0, length ?? indexBuffer.Size);
    }

    public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        context.Backend.RenderPassEncoderDraw(native, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndirect(GPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.Backend.RenderPassEncoderDrawIndirect(native, indirectBuffer.Native, indirectOffset);
    }

    public void DrawIndirectMulti(GPUBuffer indirectBuffer, uint drawCount)
    {
        for (uint i = 0; i < drawCount; i++)
        {
            context.Backend.RenderPassEncoderDrawIndirect(native, indirectBuffer.Native, i * IndirectBufferData.SizeOf);
        }
    }

    public void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        context.Backend.RenderPassEncoderDrawIndexed(native, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void DrawIndexedIndirect(GPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.Backend.RenderPassEncoderDrawIndexedIndirect(native, indirectBuffer.Native, indirectOffset);
    }
}
