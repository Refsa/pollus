namespace Pollus.Graphics.Platform;

using System;
using Pollus.Collections;
using Pollus.Graphics.Rendering;

public interface IWgpuBackend : IDisposable
{
    NativeHandle<InstanceTag> CreateInstance();
    NativeHandle<BufferTag> DeviceCreateBuffer(in NativeHandle<DeviceTag> device, in BufferDescriptor descriptor, in NativeUtf8 label);
    void BufferDestroy(in NativeHandle<BufferTag> buffer);
    void BufferRelease(in NativeHandle<BufferTag> buffer);
    void QueueWriteBuffer(in NativeHandle<QueueTag> queue, in NativeHandle<BufferTag> buffer, nuint offset, ReadOnlySpan<byte> data, uint alignedSize);
    NativeHandle<TextureTag> DeviceCreateTexture(in NativeHandle<DeviceTag> device, in TextureDescriptor descriptor, in NativeUtf8 label);
    void TextureDestroy(in NativeHandle<TextureTag> texture);
    void TextureRelease(in NativeHandle<TextureTag> texture);

    void QueueWriteTexture(in NativeHandle<QueueTag> queue, in NativeHandle<TextureTag> texture, uint mipLevel, uint originX, uint originY, uint originZ, ReadOnlySpan<byte> data, uint bytesPerRow, uint rowsPerImage, uint writeWidth,
        uint writeHeight, uint writeDepth);

    NativeHandle<SamplerTag> DeviceCreateSampler(in NativeHandle<DeviceTag> device, in SamplerDescriptor descriptor, in NativeUtf8 label);
    void SamplerRelease(in NativeHandle<SamplerTag> sampler);
    NativeHandle<BindGroupLayoutTag> DeviceCreateBindGroupLayout(in NativeHandle<DeviceTag> device, in BindGroupLayoutDescriptor descriptor, in NativeUtf8 label);
    void BindGroupLayoutRelease(in NativeHandle<BindGroupLayoutTag> layout);
    NativeHandle<BindGroupTag> DeviceCreateBindGroup(in NativeHandle<DeviceTag> device, in BindGroupDescriptor descriptor, in NativeUtf8 label);
    void BindGroupRelease(in NativeHandle<BindGroupTag> bindGroup);
    NativeHandle<CommandEncoderTag> DeviceCreateCommandEncoder(in NativeHandle<DeviceTag> device, in NativeUtf8 label);
    void CommandEncoderRelease(in NativeHandle<CommandEncoderTag> encoder);
    NativeHandle<CommandBufferTag> CommandEncoderFinish(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label);
    void CommandBufferRelease(in NativeHandle<CommandBufferTag> buffer);
    void QueueSubmit(in NativeHandle<QueueTag> queue, ReadOnlySpan<NativeHandle<CommandBufferTag>> commandBuffers);
    NativeHandle<ShaderModuleTag> DeviceCreateShaderModule(in NativeHandle<DeviceTag> device, ShaderBackend backend, in NativeUtf8 label, in NativeUtf8 code);
    void ShaderModuleRelease(in NativeHandle<ShaderModuleTag> shaderModule);
    NativeHandle<PipelineLayoutTag> DeviceCreatePipelineLayout(in NativeHandle<DeviceTag> device, in PipelineLayoutDescriptor descriptor, in NativeUtf8 label);
    void PipelineLayoutRelease(in NativeHandle<PipelineLayoutTag> layout);
    NativeHandle<ComputePipelineTag> DeviceCreateComputePipeline(in NativeHandle<DeviceTag> device, in ComputePipelineDescriptor descriptor, in NativeUtf8 label);
    void ComputePipelineRelease(in NativeHandle<ComputePipelineTag> pipeline);
    NativeHandle<TextureViewTag> TextureCreateView(in NativeHandle<TextureTag> texture, in TextureViewDescriptor descriptor, in NativeUtf8 label);
    void TextureViewRelease(in NativeHandle<TextureViewTag> view);
    NativeHandle<RenderPipelineTag> DeviceCreateRenderPipeline(in NativeHandle<DeviceTag> device, in RenderPipelineDescriptor descriptor, in NativeUtf8 label);
    void RenderPipelineRelease(in NativeHandle<RenderPipelineTag> pipeline);
    NativeHandle<RenderPassEncoderTag> CommandEncoderBeginRenderPass(in NativeHandle<CommandEncoderTag> encoder, in RenderPassDescriptor descriptor);
    void RenderPassEncoderEnd(in NativeHandle<RenderPassEncoderTag> pass);
    void RenderPassEncoderRelease(in NativeHandle<RenderPassEncoderTag> pass);
    void RenderPassEncoderSetPipeline(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<RenderPipelineTag> pipeline);
    void RenderPassEncoderSetViewport(in NativeHandle<RenderPassEncoderTag> pass, float x, float y, float width, float height, float minDepth, float maxDepth);
    void RenderPassEncoderSetScissorRect(in NativeHandle<RenderPassEncoderTag> pass, uint x, uint y, uint width, uint height);
    void RenderPassEncoderSetBlendConstant(in NativeHandle<RenderPassEncoderTag> pass, double r, double g, double b, double a);
    void RenderPassEncoderSetBindGroup(in NativeHandle<RenderPassEncoderTag> pass, uint groupIndex, in NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets);
    void RenderPassEncoderSetVertexBuffer(in NativeHandle<RenderPassEncoderTag> pass, uint slot, in NativeHandle<BufferTag> buffer, ulong offset, ulong size);
    void RenderPassEncoderSetIndexBuffer(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, IndexFormat format, ulong offset, ulong size);
    void RenderPassEncoderDraw(in NativeHandle<RenderPassEncoderTag> pass, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    void RenderPassEncoderDrawIndirect(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, uint offset);
    void RenderPassEncoderDrawIndexed(in NativeHandle<RenderPassEncoderTag> pass, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    void RenderPassEncoderDrawIndexedIndirect(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, uint offset);
    void CommandEncoderCopyTextureToTexture(in NativeHandle<CommandEncoderTag> encoder, in NativeHandle<TextureTag> srcTexture, in NativeHandle<TextureTag> dstTexture, uint width, uint height, uint depthOrArrayLayers);
    NativeHandle<ComputePassEncoderTag> CommandEncoderBeginComputePass(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label);
    void ComputePassEncoderEnd(in NativeHandle<ComputePassEncoderTag> pass);
    void ComputePassEncoderSetPipeline(in NativeHandle<ComputePassEncoderTag> pass, in NativeHandle<ComputePipelineTag> pipeline);
    void ComputePassEncoderSetBindGroup(in NativeHandle<ComputePassEncoderTag> pass, uint groupIndex, in NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets);
    void ComputePassEncoderDispatchWorkgroups(in NativeHandle<ComputePassEncoderTag> pass, uint x, uint y, uint z);

    void CommandEncoderPushDebugGroup(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label);
    void CommandEncoderPopDebugGroup(in NativeHandle<CommandEncoderTag> encoder);

    void CommandEncoderInsertDebugMarker(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label);
}

