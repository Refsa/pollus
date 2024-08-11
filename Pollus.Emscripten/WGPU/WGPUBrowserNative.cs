namespace Pollus.Emscripten;

using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

public static class WGPUBrowserNative
{
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCreateInstance")]
    extern public static unsafe Instance* CreateInstance(InstanceDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCreateInstance")]
    extern public static unsafe Instance* CreateInstance(in InstanceDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe PfnProc GetProcAddress(Device* device, byte* procName);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe PfnProc GetProcAddress(Device* device, in byte procName);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe PfnProc GetProcAddress(Device* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string procName);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterEnumerateFeatures")]
    extern public static unsafe nuint AdapterEnumerateFeatures(Adapter* adapter, FeatureName* features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterEnumerateFeatures")]
    extern public static unsafe nuint AdapterEnumerateFeatures(Adapter* adapter, ref FeatureName features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetLimits")]
    extern public static unsafe bool AdapterGetLimits(Adapter* adapter, SupportedLimits* limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetLimits")]
    extern public static unsafe bool AdapterGetLimits(Adapter* adapter, ref SupportedLimits limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetProperties")]
    extern public static unsafe void AdapterGetProperties(Adapter* adapter, AdapterProperties* properties);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetProperties")]
    extern public static unsafe void AdapterGetProperties(Adapter* adapter, ref AdapterProperties properties);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterHasFeature")]
    extern public static unsafe bool AdapterHasFeature(Adapter* adapter, FeatureName feature);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterRequestDevice")]
    extern public static unsafe void AdapterRequestDevice(Adapter* adapter, WGPUDeviceDescriptor_Browser* descriptor, nint callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterRequestDevice")]
    extern public static unsafe void AdapterRequestDevice(Adapter* adapter, in WGPUDeviceDescriptor_Browser descriptor, nint callback, void* userdata);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterReference")]
    extern public static unsafe void AdapterReference(Adapter* adapter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterRelease")]
    extern public static unsafe void AdapterRelease(Adapter* adapter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(BindGroup* bindGroup, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(BindGroup* bindGroup, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(BindGroup* bindGroup, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupReference")]
    extern public static unsafe void BindGroupReference(BindGroup* bindGroup);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupRelease")]
    extern public static unsafe void BindGroupRelease(BindGroup* bindGroup);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutReference")]
    extern public static unsafe void BindGroupLayoutReference(BindGroupLayout* bindGroupLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutRelease")]
    extern public static unsafe void BindGroupLayoutRelease(BindGroupLayout* bindGroupLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferDestroy")]
    extern public static unsafe void BufferDestroy(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetConstMappedRange")]
    extern public static unsafe void* BufferGetConstMappedRange(Buffer* buffer, nuint offset, nuint size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetMapState")]
    extern public static unsafe BufferMapState BufferGetMapState(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetMappedRange")]
    extern public static unsafe void* BufferGetMappedRange(Buffer* buffer, nuint offset, nuint size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetSize")]
    extern public static unsafe ulong BufferGetSize(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetUsage")]
    extern public static unsafe BufferUsage BufferGetUsage(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferMapAsync")]
    extern public static unsafe void BufferMapAsync(Buffer* buffer, MapMode mode, nuint offset, nuint size, PfnBufferMapCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(Buffer* buffer, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(Buffer* buffer, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(Buffer* buffer, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferUnmap")]
    extern public static unsafe void BufferUnmap(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferReference")]
    extern public static unsafe void BufferReference(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferRelease")]
    extern public static unsafe void BufferRelease(Buffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferReference")]
    extern public static unsafe void CommandBufferReference(CommandBuffer* commandBuffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferRelease")]
    extern public static unsafe void CommandBufferRelease(CommandBuffer* commandBuffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginComputePass")]
    extern public static unsafe ComputePassEncoder* CommandEncoderBeginComputePass(CommandEncoder* commandEncoder, ComputePassDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginComputePass")]
    extern public static unsafe ComputePassEncoder* CommandEncoderBeginComputePass(CommandEncoder* commandEncoder, in ComputePassDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginRenderPass")]
    extern public static unsafe RenderPassEncoder* CommandEncoderBeginRenderPass(CommandEncoder* commandEncoder, WGPURenderPassDescriptor_Browser* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginRenderPass")]
    extern public static unsafe RenderPassEncoder* CommandEncoderBeginRenderPass(CommandEncoder* commandEncoder, in WGPURenderPassDescriptor_Browser descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderClearBuffer")]
    extern public static unsafe void CommandEncoderClearBuffer(CommandEncoder* commandEncoder, Buffer* buffer, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToBuffer")]
    extern public static unsafe void CommandEncoderCopyBufferToBuffer(CommandEncoder* commandEncoder, Buffer* source, ulong sourceOffset, Buffer* destination, ulong destinationOffset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, ImageCopyTexture* destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, ImageCopyTexture* destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, in ImageCopyTexture destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, in ImageCopyTexture destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, ImageCopyTexture* destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, ImageCopyTexture* destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, in ImageCopyTexture destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, in ImageCopyTexture destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyBuffer* destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyBuffer* destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyBuffer destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyBuffer destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyBuffer* destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyBuffer* destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyBuffer destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyBuffer destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyTexture* destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyTexture* destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyTexture destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyTexture destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyTexture* destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyTexture* destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyTexture destination, Extent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyTexture destination, in Extent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderFinish")]
    extern public static unsafe CommandBuffer* CommandEncoderFinish(CommandEncoder* commandEncoder, CommandBufferDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderFinish")]
    extern public static unsafe CommandBuffer* CommandEncoderFinish(CommandEncoder* commandEncoder, in CommandBufferDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPopDebugGroup")]
    extern public static unsafe void CommandEncoderPopDebugGroup(CommandEncoder* commandEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderResolveQuerySet")]
    extern public static unsafe void CommandEncoderResolveQuerySet(CommandEncoder* commandEncoder, QuerySet* querySet, uint firstQuery, uint queryCount, Buffer* destination, ulong destinationOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderWriteTimestamp")]
    extern public static unsafe void CommandEncoderWriteTimestamp(CommandEncoder* commandEncoder, QuerySet* querySet, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderReference")]
    extern public static unsafe void CommandEncoderReference(CommandEncoder* commandEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderRelease")]
    extern public static unsafe void CommandEncoderRelease(CommandEncoder* commandEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderBeginPipelineStatisticsQuery")]
    extern public static unsafe void ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder* computePassEncoder, QuerySet* querySet, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderDispatchWorkgroups")]
    extern public static unsafe void ComputePassEncoderDispatchWorkgroups(ComputePassEncoder* computePassEncoder, uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderDispatchWorkgroupsIndirect")]
    extern public static unsafe void ComputePassEncoderDispatchWorkgroupsIndirect(ComputePassEncoder* computePassEncoder, Buffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderEnd")]
    extern public static unsafe void ComputePassEncoderEnd(ComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderEndPipelineStatisticsQuery")]
    extern public static unsafe void ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPopDebugGroup")]
    extern public static unsafe void ComputePassEncoderPopDebugGroup(ComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetBindGroup")]
    extern public static unsafe void ComputePassEncoderSetBindGroup(ComputePassEncoder* computePassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetBindGroup")]
    extern public static unsafe void ComputePassEncoderSetBindGroup(ComputePassEncoder* computePassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetPipeline")]
    extern public static unsafe void ComputePassEncoderSetPipeline(ComputePassEncoder* computePassEncoder, ComputePipeline* pipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderReference")]
    extern public static unsafe void ComputePassEncoderReference(ComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderRelease")]
    extern public static unsafe void ComputePassEncoderRelease(ComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineGetBindGroupLayout")]
    extern public static unsafe BindGroupLayout* ComputePipelineGetBindGroupLayout(ComputePipeline* computePipeline, uint groupIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineReference")]
    extern public static unsafe void ComputePipelineReference(ComputePipeline* computePipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineRelease")]
    extern public static unsafe void ComputePipelineRelease(ComputePipeline* computePipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroup")]
    extern public static unsafe BindGroup* DeviceCreateBindGroup(Device* device, BindGroupDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroup")]
    extern public static unsafe BindGroup* DeviceCreateBindGroup(Device* device, in BindGroupDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroupLayout")]
    extern public static unsafe BindGroupLayout* DeviceCreateBindGroupLayout(Device* device, BindGroupLayoutDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroupLayout")]
    extern public static unsafe BindGroupLayout* DeviceCreateBindGroupLayout(Device* device, in BindGroupLayoutDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBuffer")]
    extern public static unsafe Buffer* DeviceCreateBuffer(Device* device, BufferDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBuffer")]
    extern public static unsafe Buffer* DeviceCreateBuffer(Device* device, in BufferDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateCommandEncoder")]
    extern public static unsafe CommandEncoder* DeviceCreateCommandEncoder(Device* device, CommandEncoderDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateCommandEncoder")]
    extern public static unsafe CommandEncoder* DeviceCreateCommandEncoder(Device* device, in CommandEncoderDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipeline")]
    extern public static unsafe ComputePipeline* DeviceCreateComputePipeline(Device* device, ComputePipelineDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipeline")]
    extern public static unsafe ComputePipeline* DeviceCreateComputePipeline(Device* device, in ComputePipelineDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipelineAsync")]
    extern public static unsafe void DeviceCreateComputePipelineAsync(Device* device, ComputePipelineDescriptor* descriptor, PfnCreateComputePipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipelineAsync")]
    extern public static unsafe void DeviceCreateComputePipelineAsync(Device* device, in ComputePipelineDescriptor descriptor, PfnCreateComputePipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreatePipelineLayout")]
    extern public static unsafe PipelineLayout* DeviceCreatePipelineLayout(Device* device, PipelineLayoutDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreatePipelineLayout")]
    extern public static unsafe PipelineLayout* DeviceCreatePipelineLayout(Device* device, in PipelineLayoutDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateQuerySet")]
    extern public static unsafe QuerySet* DeviceCreateQuerySet(Device* device, QuerySetDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateQuerySet")]
    extern public static unsafe QuerySet* DeviceCreateQuerySet(Device* device, in QuerySetDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderBundleEncoder")]
    extern public static unsafe RenderBundleEncoder* DeviceCreateRenderBundleEncoder(Device* device, RenderBundleEncoderDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderBundleEncoder")]
    extern public static unsafe RenderBundleEncoder* DeviceCreateRenderBundleEncoder(Device* device, in RenderBundleEncoderDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipeline")]
    extern public static unsafe RenderPipeline* DeviceCreateRenderPipeline(Device* device, RenderPipelineDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipeline")]
    extern public static unsafe RenderPipeline* DeviceCreateRenderPipeline(Device* device, in RenderPipelineDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipelineAsync")]
    extern public static unsafe void DeviceCreateRenderPipelineAsync(Device* device, RenderPipelineDescriptor* descriptor, PfnCreateRenderPipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipelineAsync")]
    extern public static unsafe void DeviceCreateRenderPipelineAsync(Device* device, in RenderPipelineDescriptor descriptor, PfnCreateRenderPipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateSampler")]
    extern public static unsafe Sampler* DeviceCreateSampler(Device* device, SamplerDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateSampler")]
    extern public static unsafe Sampler* DeviceCreateSampler(Device* device, in SamplerDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateShaderModule")]
    extern public static unsafe ShaderModule* DeviceCreateShaderModule(Device* device, ShaderModuleDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateShaderModule")]
    extern public static unsafe ShaderModule* DeviceCreateShaderModule(Device* device, in ShaderModuleDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateTexture")]
    extern public static unsafe Texture* DeviceCreateTexture(Device* device, TextureDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateTexture")]
    extern public static unsafe Texture* DeviceCreateTexture(Device* device, in TextureDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceDestroy")]
    extern public static unsafe void DeviceDestroy(Device* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceEnumerateFeatures")]
    extern public static unsafe nuint DeviceEnumerateFeatures(Device* device, FeatureName* features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceEnumerateFeatures")]
    extern public static unsafe nuint DeviceEnumerateFeatures(Device* device, ref FeatureName features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceGetLimits")]
    extern public static unsafe bool DeviceGetLimits(Device* device, SupportedLimits* limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceGetLimits")]
    extern public static unsafe bool DeviceGetLimits(Device* device, ref SupportedLimits limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceGetQueue")]
    extern public static unsafe Queue* DeviceGetQueue(Device* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceHasFeature")]
    extern public static unsafe bool DeviceHasFeature(Device* device, FeatureName feature);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDevicePopErrorScope")]
    extern public static unsafe void DevicePopErrorScope(Device* device, PfnErrorCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDevicePushErrorScope")]
    extern public static unsafe void DevicePushErrorScope(Device* device, ErrorFilter filter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(Device* device, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(Device* device, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(Device* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetUncapturedErrorCallback")]
    extern public static unsafe void DeviceSetUncapturedErrorCallback(Device* device, PfnErrorCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceReference")]
    extern public static unsafe void DeviceReference(Device* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceRelease")]
    extern public static unsafe void DeviceRelease(Device* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceCreateSurface")]
    extern public static unsafe Surface* InstanceCreateSurface(Instance* instance, SurfaceDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceCreateSurface")]
    extern public static unsafe Surface* InstanceCreateSurface(Instance* instance, in SurfaceDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceProcessEvents")]
    extern public static unsafe void InstanceProcessEvents(Instance* instance);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceRequestAdapter")]
    extern public static unsafe void InstanceRequestAdapter(Instance* instance, RequestAdapterOptions* options, nint callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceRequestAdapter")]
    extern public static unsafe void InstanceRequestAdapter(Instance* instance, in RequestAdapterOptions options, nint callback, void* userdata);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceReference")]
    extern public static unsafe void InstanceReference(Instance* instance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceRelease")]
    extern public static unsafe void InstanceRelease(Instance* instance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutReference")]
    extern public static unsafe void PipelineLayoutReference(PipelineLayout* pipelineLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutRelease")]
    extern public static unsafe void PipelineLayoutRelease(PipelineLayout* pipelineLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetDestroy")]
    extern public static unsafe void QuerySetDestroy(QuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetGetCount")]
    extern public static unsafe uint QuerySetGetCount(QuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetGetType")]
    extern public static unsafe QueryType QuerySetGetType(QuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(QuerySet* querySet, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(QuerySet* querySet, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(QuerySet* querySet, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetReference")]
    extern public static unsafe void QuerySetReference(QuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetRelease")]
    extern public static unsafe void QuerySetRelease(QuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueOnSubmittedWorkDone")]
    extern public static unsafe void QueueOnSubmittedWorkDone(Queue* queue, PfnQueueWorkDoneCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(Queue* queue, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(Queue* queue, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(Queue* queue, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSubmit")]
    extern public static unsafe void QueueSubmit(Queue* queue, nuint commandCount, CommandBuffer** commands);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSubmit")]
    extern public static unsafe void QueueSubmit(Queue* queue, nuint commandCount, ref CommandBuffer* commands);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteBuffer")]
    extern public static unsafe void QueueWriteBuffer(Queue* queue, Buffer* buffer, ulong bufferOffset, void* data, nuint size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, Extent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, in Extent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, Extent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, in Extent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, Extent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, in Extent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, Extent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, in Extent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueReference")]
    extern public static unsafe void QueueReference(Queue* queue);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueRelease")]
    extern public static unsafe void QueueRelease(Queue* queue);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleReference")]
    extern public static unsafe void RenderBundleReference(RenderBundle* renderBundle);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleRelease")]
    extern public static unsafe void RenderBundleRelease(RenderBundle* renderBundle);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDraw")]
    extern public static unsafe void RenderBundleEncoderDraw(RenderBundleEncoder* renderBundleEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDrawIndexed")]
    extern public static unsafe void RenderBundleEncoderDrawIndexed(RenderBundleEncoder* renderBundleEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDrawIndexedIndirect")]
    extern public static unsafe void RenderBundleEncoderDrawIndexedIndirect(RenderBundleEncoder* renderBundleEncoder, Buffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDrawIndirect")]
    extern public static unsafe void RenderBundleEncoderDrawIndirect(RenderBundleEncoder* renderBundleEncoder, Buffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderFinish")]
    extern public static unsafe RenderBundle* RenderBundleEncoderFinish(RenderBundleEncoder* renderBundleEncoder, RenderBundleDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderFinish")]
    extern public static unsafe RenderBundle* RenderBundleEncoderFinish(RenderBundleEncoder* renderBundleEncoder, in RenderBundleDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPopDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPopDebugGroup(RenderBundleEncoder* renderBundleEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetBindGroup")]
    extern public static unsafe void RenderBundleEncoderSetBindGroup(RenderBundleEncoder* renderBundleEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetBindGroup")]
    extern public static unsafe void RenderBundleEncoderSetBindGroup(RenderBundleEncoder* renderBundleEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetIndexBuffer")]
    extern public static unsafe void RenderBundleEncoderSetIndexBuffer(RenderBundleEncoder* renderBundleEncoder, Buffer* buffer, IndexFormat format, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetPipeline")]
    extern public static unsafe void RenderBundleEncoderSetPipeline(RenderBundleEncoder* renderBundleEncoder, RenderPipeline* pipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetVertexBuffer")]
    extern public static unsafe void RenderBundleEncoderSetVertexBuffer(RenderBundleEncoder* renderBundleEncoder, uint slot, Buffer* buffer, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderReference")]
    extern public static unsafe void RenderBundleEncoderReference(RenderBundleEncoder* renderBundleEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderRelease")]
    extern public static unsafe void RenderBundleEncoderRelease(RenderBundleEncoder* renderBundleEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderBeginOcclusionQuery")]
    extern public static unsafe void RenderPassEncoderBeginOcclusionQuery(RenderPassEncoder* renderPassEncoder, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderBeginPipelineStatisticsQuery")]
    extern public static unsafe void RenderPassEncoderBeginPipelineStatisticsQuery(RenderPassEncoder* renderPassEncoder, QuerySet* querySet, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDraw")]
    extern public static unsafe void RenderPassEncoderDraw(RenderPassEncoder* renderPassEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDrawIndexed")]
    extern public static unsafe void RenderPassEncoderDrawIndexed(RenderPassEncoder* renderPassEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDrawIndexedIndirect")]
    extern public static unsafe void RenderPassEncoderDrawIndexedIndirect(RenderPassEncoder* renderPassEncoder, Buffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDrawIndirect")]
    extern public static unsafe void RenderPassEncoderDrawIndirect(RenderPassEncoder* renderPassEncoder, Buffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderEnd")]
    extern public static unsafe void RenderPassEncoderEnd(RenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderEndOcclusionQuery")]
    extern public static unsafe void RenderPassEncoderEndOcclusionQuery(RenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderEndPipelineStatisticsQuery")]
    extern public static unsafe void RenderPassEncoderEndPipelineStatisticsQuery(RenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderExecuteBundles")]
    extern public static unsafe void RenderPassEncoderExecuteBundles(RenderPassEncoder* renderPassEncoder, nuint bundleCount, RenderBundle** bundles);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderExecuteBundles")]
    extern public static unsafe void RenderPassEncoderExecuteBundles(RenderPassEncoder* renderPassEncoder, nuint bundleCount, ref RenderBundle* bundles);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPopDebugGroup")]
    extern public static unsafe void RenderPassEncoderPopDebugGroup(RenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBindGroup")]
    extern public static unsafe void RenderPassEncoderSetBindGroup(RenderPassEncoder* renderPassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBindGroup")]
    extern public static unsafe void RenderPassEncoderSetBindGroup(RenderPassEncoder* renderPassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBlendConstant")]
    extern public static unsafe void RenderPassEncoderSetBlendConstant(RenderPassEncoder* renderPassEncoder, Color* color);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBlendConstant")]
    extern public static unsafe void RenderPassEncoderSetBlendConstant(RenderPassEncoder* renderPassEncoder, in Color color);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetIndexBuffer")]
    extern public static unsafe void RenderPassEncoderSetIndexBuffer(RenderPassEncoder* renderPassEncoder, Buffer* buffer, IndexFormat format, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetPipeline")]
    extern public static unsafe void RenderPassEncoderSetPipeline(RenderPassEncoder* renderPassEncoder, RenderPipeline* pipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetScissorRect")]
    extern public static unsafe void RenderPassEncoderSetScissorRect(RenderPassEncoder* renderPassEncoder, uint x, uint y, uint width, uint height);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetStencilReference")]
    extern public static unsafe void RenderPassEncoderSetStencilReference(RenderPassEncoder* renderPassEncoder, uint reference);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetVertexBuffer")]
    extern public static unsafe void RenderPassEncoderSetVertexBuffer(RenderPassEncoder* renderPassEncoder, uint slot, Buffer* buffer, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetViewport")]
    extern public static unsafe void RenderPassEncoderSetViewport(RenderPassEncoder* renderPassEncoder, float x, float y, float width, float height, float minDepth, float maxDepth);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderReference")]
    extern public static unsafe void RenderPassEncoderReference(RenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderRelease")]
    extern public static unsafe void RenderPassEncoderRelease(RenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineGetBindGroupLayout")]
    extern public static unsafe BindGroupLayout* RenderPipelineGetBindGroupLayout(RenderPipeline* renderPipeline, uint groupIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineReference")]
    extern public static unsafe void RenderPipelineReference(RenderPipeline* renderPipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineRelease")]
    extern public static unsafe void RenderPipelineRelease(RenderPipeline* renderPipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(Sampler* sampler, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(Sampler* sampler, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(Sampler* sampler, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerReference")]
    extern public static unsafe void SamplerReference(Sampler* sampler);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerRelease")]
    extern public static unsafe void SamplerRelease(Sampler* sampler);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleGetCompilationInfo")]
    extern public static unsafe void ShaderModuleGetCompilationInfo(ShaderModule* shaderModule, PfnCompilationInfoCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleReference")]
    extern public static unsafe void ShaderModuleReference(ShaderModule* shaderModule);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleRelease")]
    extern public static unsafe void ShaderModuleRelease(ShaderModule* shaderModule);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceConfigure")]
    extern public static unsafe void SurfaceConfigure(Surface* surface, SurfaceConfiguration* config);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceConfigure")]
    extern public static unsafe void SurfaceConfigure(Surface* surface, in SurfaceConfiguration config);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceGetCapabilities")]
    extern public static unsafe void SurfaceGetCapabilities(Surface* surface, Adapter* adapter, SurfaceCapabilities* capabilities);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceGetCapabilities")]
    extern public static unsafe void SurfaceGetCapabilities(Surface* surface, Adapter* adapter, ref SurfaceCapabilities capabilities);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceGetCurrentTexture")]
    extern public static unsafe void SurfaceGetCurrentTexture(Surface* surface, SurfaceTexture* surfaceTexture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceGetCurrentTexture")]
    extern public static unsafe void SurfaceGetCurrentTexture(Surface* surface, ref SurfaceTexture surfaceTexture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceGetPreferredFormat")]
    extern public static unsafe TextureFormat SurfaceGetPreferredFormat(Surface* surface, Adapter* adapter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfacePresent")]
    extern public static unsafe void SurfacePresent(Surface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceUnconfigure")]
    extern public static unsafe void SurfaceUnconfigure(Surface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceReference")]
    extern public static unsafe void SurfaceReference(Surface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceRelease")]
    extern public static unsafe void SurfaceRelease(Surface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceCapabilitiesFreeMembers")]
    extern public static unsafe void SurfaceCapabilitiesFreeMembers(SurfaceCapabilities capabilities);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureCreateView")]
    extern public static unsafe TextureView* TextureCreateView(Texture* texture, TextureViewDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureCreateView")]
    extern public static unsafe TextureView* TextureCreateView(Texture* texture, in TextureViewDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureDestroy")]
    extern public static unsafe void TextureDestroy(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetDepthOrArrayLayers")]
    extern public static unsafe uint TextureGetDepthOrArrayLayers(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetDimension")]
    extern public static unsafe TextureDimension TextureGetDimension(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetFormat")]
    extern public static unsafe TextureFormat TextureGetFormat(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetHeight")]
    extern public static unsafe uint TextureGetHeight(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetMipLevelCount")]
    extern public static unsafe uint TextureGetMipLevelCount(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetSampleCount")]
    extern public static unsafe uint TextureGetSampleCount(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetUsage")]
    extern public static unsafe TextureUsage TextureGetUsage(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetWidth")]
    extern public static unsafe uint TextureGetWidth(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(Texture* texture, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(Texture* texture, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(Texture* texture, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureReference")]
    extern public static unsafe void TextureReference(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureRelease")]
    extern public static unsafe void TextureRelease(Texture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(TextureView* textureView, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(TextureView* textureView, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(TextureView* textureView, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewReference")]
    extern public static unsafe void TextureViewReference(TextureView* textureView);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewRelease")]
    extern public static unsafe void TextureViewRelease(TextureView* textureView);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateSwapChain")]
    extern public static unsafe WGPUSwapChain_Browser* DeviceCreateSwapChain(Device* device, Surface* surface, in WGPUSwapChainDescriptor_Browser descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSwapChainGetCurrentTextureView")]
    extern public static unsafe TextureView* SwapChainGetCurrentTextureView(WGPUSwapChain_Browser* swapChain);
}
