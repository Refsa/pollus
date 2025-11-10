namespace Pollus.Graphics.Platform;

using System;
using Pollus.Collections;
using Pollus.Graphics.Rendering;

public interface IWgpuBackend : IDisposable
{
    NativeHandle<InstanceTag> CreateInstance();
    NativeHandle<SurfaceTag> CreateSurface(NativeHandle<InstanceTag> instance, SurfaceSource source);
    void RequestAdapter(NativeHandle<InstanceTag> instance, in AdapterOptions options, Action<AdapterResult> callback);
    void RequestDevice(NativeHandle<AdapterTag> adapter, in DeviceOptions options, Action<DeviceResult> callback);
    NativeHandle<QueueTag> GetQueue(NativeHandle<DeviceTag> device);
    NativeHandle<SwapChainTag> CreateSwapChain(NativeHandle<DeviceTag> device, NativeHandle<SurfaceTag> surface, in SwapChainOptions descriptor);
    NativeHandle<BufferTag> DeviceCreateBuffer(NativeHandle<DeviceTag> device, in BufferDescriptor descriptor, NativeUtf8 label);
    void BufferDestroy(NativeHandle<BufferTag> buffer);
    void BufferRelease(NativeHandle<BufferTag> buffer);
    void QueueWriteBuffer(NativeHandle<QueueTag> queue, NativeHandle<BufferTag> buffer, nuint offset, ReadOnlySpan<byte> data, uint alignedSize);
    NativeHandle<TextureTag> DeviceCreateTexture(NativeHandle<DeviceTag> device, in TextureDescriptor descriptor, NativeUtf8 label);
    void TextureDestroy(NativeHandle<TextureTag> texture);
    void TextureRelease(NativeHandle<TextureTag> texture);
    void QueueWriteTexture(NativeHandle<QueueTag> queue, NativeHandle<TextureTag> texture, uint mipLevel, uint originX, uint originY, uint originZ, ReadOnlySpan<byte> data, uint bytesPerRow, uint rowsPerImage, uint writeWidth, uint writeHeight, uint writeDepth);
    NativeHandle<SamplerTag> DeviceCreateSampler(NativeHandle<DeviceTag> device, in SamplerDescriptor descriptor, NativeUtf8 label);
    void SamplerRelease(NativeHandle<SamplerTag> sampler);
    NativeHandle<BindGroupLayoutTag> DeviceCreateBindGroupLayout(NativeHandle<DeviceTag> device, in BindGroupLayoutDescriptor descriptor, NativeUtf8 label);
    void BindGroupLayoutRelease(NativeHandle<BindGroupLayoutTag> layout);
    NativeHandle<BindGroupTag> DeviceCreateBindGroup(NativeHandle<DeviceTag> device, in BindGroupDescriptor descriptor, NativeUtf8 label);
    void BindGroupRelease(NativeHandle<BindGroupTag> bindGroup);
    NativeHandle<CommandEncoderTag> DeviceCreateCommandEncoder(NativeHandle<DeviceTag> device, NativeUtf8 label);
    void CommandEncoderRelease(NativeHandle<CommandEncoderTag> encoder);
    NativeHandle<CommandBufferTag> CommandEncoderFinish(NativeHandle<CommandEncoderTag> encoder, NativeUtf8 label);
    void CommandBufferRelease(NativeHandle<CommandBufferTag> buffer);
    void QueueSubmit(NativeHandle<QueueTag> queue, ReadOnlySpan<NativeHandle<CommandBufferTag>> commandBuffers);
    NativeHandle<ShaderModuleTag> DeviceCreateShaderModule(NativeHandle<DeviceTag> device, ShaderBackend backend, NativeUtf8 label, NativeUtf8 code);
    void ShaderModuleRelease(NativeHandle<ShaderModuleTag> shaderModule);
    NativeHandle<PipelineLayoutTag> DeviceCreatePipelineLayout(NativeHandle<DeviceTag> device, in PipelineLayoutDescriptor descriptor, NativeUtf8 label);
    void PipelineLayoutRelease(NativeHandle<PipelineLayoutTag> layout);
    NativeHandle<ComputePipelineTag> DeviceCreateComputePipeline(NativeHandle<DeviceTag> device, in ComputePipelineDescriptor descriptor, NativeUtf8 label);
    void ComputePipelineRelease(NativeHandle<ComputePipelineTag> pipeline);
    NativeHandle<TextureViewTag> TextureCreateView(NativeHandle<TextureTag> texture, in TextureViewDescriptor descriptor, NativeUtf8 label);
    void TextureViewRelease(NativeHandle<TextureViewTag> view);
    NativeHandle<RenderPipelineTag> DeviceCreateRenderPipeline(NativeHandle<DeviceTag> device, in RenderPipelineDescriptor descriptor, NativeUtf8 label);
    void RenderPipelineRelease(NativeHandle<RenderPipelineTag> pipeline);
    NativeHandle<RenderPassEncoderTag> CommandEncoderBeginRenderPass(NativeHandle<CommandEncoderTag> encoder, in RenderPassDescriptor descriptor);
    void RenderPassEncoderEnd(NativeHandle<RenderPassEncoderTag> pass);
    void RenderPassEncoderSetPipeline(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<RenderPipelineTag> pipeline);
    void RenderPassEncoderSetViewport(NativeHandle<RenderPassEncoderTag> pass, float x, float y, float width, float height, float minDepth, float maxDepth);
    void RenderPassEncoderSetScissorRect(NativeHandle<RenderPassEncoderTag> pass, uint x, uint y, uint width, uint height);
    void RenderPassEncoderSetBlendConstant(NativeHandle<RenderPassEncoderTag> pass, double r, double g, double b, double a);
    void RenderPassEncoderSetBindGroup(NativeHandle<RenderPassEncoderTag> pass, uint groupIndex, NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets);
    void RenderPassEncoderSetVertexBuffer(NativeHandle<RenderPassEncoderTag> pass, uint slot, NativeHandle<BufferTag> buffer, ulong offset, ulong size);
    void RenderPassEncoderSetIndexBuffer(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<BufferTag> buffer, IndexFormat format, ulong offset, ulong size);
    void RenderPassEncoderDraw(NativeHandle<RenderPassEncoderTag> pass, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    void RenderPassEncoderDrawIndirect(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<BufferTag> buffer, uint offset);
    void RenderPassEncoderDrawIndexed(NativeHandle<RenderPassEncoderTag> pass, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    void RenderPassEncoderDrawIndexedIndirect(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<BufferTag> buffer, uint offset);
    void CommandEncoderCopyTextureToTexture(NativeHandle<CommandEncoderTag> encoder, NativeHandle<TextureTag> srcTexture, NativeHandle<TextureTag> dstTexture, uint width, uint height, uint depthOrArrayLayers);
    NativeHandle<ComputePassEncoderTag> CommandEncoderBeginComputePass(NativeHandle<CommandEncoderTag> encoder, NativeUtf8 label);
    void ComputePassEncoderEnd(NativeHandle<ComputePassEncoderTag> pass);
    void ComputePassEncoderSetPipeline(NativeHandle<ComputePassEncoderTag> pass, NativeHandle<ComputePipelineTag> pipeline);
    void ComputePassEncoderSetBindGroup(NativeHandle<ComputePassEncoderTag> pass, uint groupIndex, NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets);
    void ComputePassEncoderDispatchWorkgroups(NativeHandle<ComputePassEncoderTag> pass, uint x, uint y, uint z);
}

