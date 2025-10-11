namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

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
        native = context.wgpu.CommandEncoderBeginRenderPass(commandEncoder, in descriptor);
    }
#endif

    public void Dispose()
    {
        context.wgpu.RenderPassEncoderEnd(native);
        context.wgpu.RenderPassEncoderRelease(native);
    }

    public void SetPipeline(GPURenderPipeline pipeline)
    {
        context.wgpu.RenderPassEncoderSetPipeline(native, (Silk.NET.WebGPU.RenderPipeline*)pipeline.Native);
    }

    public void SetViewport(Vec2f pos, Vec2f size, float minDepth, float maxDepth)
    {
        context.wgpu.RenderPassEncoderSetViewport(native, pos.X, pos.Y, size.X, size.Y, minDepth, maxDepth);
    }

    public void SetScissorRect(uint x, uint y, uint width, uint height)
    {
        context.wgpu.RenderPassEncoderSetScissorRect(native, x, y, width, height);
    }

    public void SetBlendConstant(Vec4<double> color)
    {
        var c = new Silk.NET.WebGPU.Color
        {
            R = color.X,
            G = color.Y,
            B = color.Z,
            A = color.W
        };
        context.wgpu.RenderPassEncoderSetBlendConstant(native, in c);
    }

    public void SetBindGroup(uint groupIndex, GPUBindGroup bindGroup, uint dynamicOffsetCount = 0, uint dynamicOffsets = 0)
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

    public void SetVertexBuffer(uint slot, GPUBuffer buffer, ulong? offset = null, ulong? length = null)
    {
        context.wgpu.RenderPassEncoderSetVertexBuffer(native, slot, (Silk.NET.WebGPU.Buffer*)buffer.Native, offset ?? 0, length ?? buffer.Size);
    }

    public void SetIndexBuffer(GPUBuffer indexBuffer, IndexFormat format, ulong? offset = null, ulong? length = null)
    {
        context.wgpu.RenderPassEncoderSetIndexBuffer(native,
            (Silk.NET.WebGPU.Buffer*)indexBuffer.Native,
            (Silk.NET.WebGPU.IndexFormat)format,
            offset ?? 0, length ?? indexBuffer.Size);
    }

    public void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        context.wgpu.RenderPassEncoderDraw(native, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndirect(GPUBuffer indirectBuffer, uint indirectOffset)
    {
        context.wgpu.RenderPassEncoderDrawIndirect(native, (Silk.NET.WebGPU.Buffer*)indirectBuffer.Native, indirectOffset);
    }

    public void DrawIndirectMulti(GPUBuffer indirectBuffer, uint drawCount)
    {
        for (uint i = 0; i < drawCount; i++)
        {
            context.wgpu.RenderPassEncoderDrawIndirect(native, (Silk.NET.WebGPU.Buffer*)indirectBuffer.Native, i * IndirectBufferData.SizeOf);
        }
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
