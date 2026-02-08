namespace Pollus.Emscripten;

using System.Runtime.InteropServices;
using WGPU;

public static class WGPUBrowserNative
{
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCreateInstance")]
    extern public static unsafe WGPUInstance* CreateInstance(WGPU.WGPUInstanceDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCreateInstance")]
    extern public static unsafe WGPUInstance* CreateInstance(in WGPU.WGPUInstanceDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe Silk.NET.WebGPU.PfnProc GetProcAddress(WGPUDevice* device, byte* procName);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe Silk.NET.WebGPU.PfnProc GetProcAddress(WGPUDevice* device, in byte procName);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuGetProcAddress")]
    extern public static unsafe Silk.NET.WebGPU.PfnProc GetProcAddress(WGPUDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string procName);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterEnumerateFeatures")]
    extern public static unsafe nuint AdapterEnumerateFeatures(WGPUAdapter* adapter, WGPUFeatureName* features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterEnumerateFeatures")]
    extern public static unsafe nuint AdapterEnumerateFeatures(WGPUAdapter* adapter, ref WGPUFeatureName features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetLimits")]
    extern public static unsafe bool AdapterGetLimits(WGPUAdapter* adapter, WGPUSupportedLimits* limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetLimits")]
    extern public static unsafe bool AdapterGetLimits(WGPUAdapter* adapter, ref WGPUSupportedLimits limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetProperties")]
    extern public static unsafe void AdapterGetProperties(WGPUAdapter* adapter, WGPUAdapterProperties* properties);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterGetProperties")]
    extern public static unsafe void AdapterGetProperties(WGPUAdapter* adapter, ref WGPUAdapterProperties properties);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterHasFeature")]
    extern public static unsafe bool AdapterHasFeature(WGPUAdapter* adapter, WGPUFeatureName feature);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterRequestDevice")]
    extern public static unsafe void AdapterRequestDevice(WGPUAdapter* adapter, WGPU.WGPUDeviceDescriptor* descriptor, nint callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterRequestDevice")]
    extern public static unsafe void AdapterRequestDevice(WGPUAdapter* adapter, in WGPU.WGPUDeviceDescriptor descriptor, nint callback, void* userdata);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterReference")]
    extern public static unsafe void AdapterReference(WGPUAdapter* adapter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuAdapterRelease")]
    extern public static unsafe void AdapterRelease(WGPUAdapter* adapter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupSetLabel")]
    extern public static unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupReference")]
    extern public static unsafe void BindGroupReference(WGPUBindGroup* bindGroup);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupRelease")]
    extern public static unsafe void BindGroupRelease(WGPUBindGroup* bindGroup);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutSetLabel")]
    extern public static unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutReference")]
    extern public static unsafe void BindGroupLayoutReference(WGPUBindGroupLayout* bindGroupLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBindGroupLayoutRelease")]
    extern public static unsafe void BindGroupLayoutRelease(WGPUBindGroupLayout* bindGroupLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferDestroy")]
    extern public static unsafe void BufferDestroy(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetConstMappedRange")]
    extern public static unsafe void* BufferGetConstMappedRange(WGPUBuffer* buffer, nuint offset, nuint size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetMapState")]
    extern public static unsafe WGPUBufferMapState BufferGetMapState(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetMappedRange")]
    extern public static unsafe void* BufferGetMappedRange(WGPUBuffer* buffer, nuint offset, nuint size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetSize")]
    extern public static unsafe ulong BufferGetSize(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferGetUsage")]
    extern public static unsafe WGPUBufferUsage BufferGetUsage(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferMapAsync")]
    extern public static unsafe void BufferMapAsync(WGPUBuffer* buffer, WGPUMapMode mode, nuint offset, nuint size, Silk.NET.WebGPU.PfnBufferMapCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(WGPUBuffer* buffer, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(WGPUBuffer* buffer, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferSetLabel")]
    extern public static unsafe void BufferSetLabel(WGPUBuffer* buffer, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferUnmap")]
    extern public static unsafe void BufferUnmap(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferReference")]
    extern public static unsafe void BufferReference(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuBufferRelease")]
    extern public static unsafe void BufferRelease(WGPUBuffer* buffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferSetLabel")]
    extern public static unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferReference")]
    extern public static unsafe void CommandBufferReference(WGPUCommandBuffer* commandBuffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandBufferRelease")]
    extern public static unsafe void CommandBufferRelease(WGPUCommandBuffer* commandBuffer);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginComputePass")]
    extern public static unsafe WGPUComputePassEncoder* CommandEncoderBeginComputePass(WGPUCommandEncoder* commandEncoder, WGPUComputePassDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginComputePass")]
    extern public static unsafe WGPUComputePassEncoder* CommandEncoderBeginComputePass(WGPUCommandEncoder* commandEncoder, in WGPUComputePassDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginRenderPass")]
    extern public static unsafe WGPURenderPassEncoder* CommandEncoderBeginRenderPass(WGPUCommandEncoder* commandEncoder, WGPU.WGPURenderPassDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderBeginRenderPass")]
    extern public static unsafe WGPURenderPassEncoder* CommandEncoderBeginRenderPass(WGPUCommandEncoder* commandEncoder, in WGPU.WGPURenderPassDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderClearBuffer")]
    extern public static unsafe void CommandEncoderClearBuffer(WGPUCommandEncoder* commandEncoder, WGPUBuffer* buffer, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToBuffer")]
    extern public static unsafe void CommandEncoderCopyBufferToBuffer(WGPUCommandEncoder* commandEncoder, WGPUBuffer* source, ulong sourceOffset, WGPUBuffer* destination, ulong destinationOffset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyBufferToTexture")]
    extern public static unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyBuffer* destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyBuffer* destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyBuffer destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyBuffer destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyBuffer* destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyBuffer* destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyBuffer destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToBuffer")]
    extern public static unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyBuffer destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderCopyTextureToTexture")]
    extern public static unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderFinish")]
    extern public static unsafe WGPUCommandBuffer* CommandEncoderFinish(WGPUCommandEncoder* commandEncoder, WGPUCommandBufferDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderFinish")]
    extern public static unsafe WGPUCommandBuffer* CommandEncoderFinish(WGPUCommandEncoder* commandEncoder, in WGPUCommandBufferDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderInsertDebugMarker")]
    extern public static unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPopDebugGroup")]
    extern public static unsafe void CommandEncoderPopDebugGroup(WGPUCommandEncoder* commandEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderPushDebugGroup")]
    extern public static unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderResolveQuerySet")]
    extern public static unsafe void CommandEncoderResolveQuerySet(WGPUCommandEncoder* commandEncoder, WGPUQuerySet* querySet, uint firstQuery, uint queryCount, WGPUBuffer* destination, ulong destinationOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderSetLabel")]
    extern public static unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderWriteTimestamp")]
    extern public static unsafe void CommandEncoderWriteTimestamp(WGPUCommandEncoder* commandEncoder, WGPUQuerySet* querySet, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderReference")]
    extern public static unsafe void CommandEncoderReference(WGPUCommandEncoder* commandEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuCommandEncoderRelease")]
    extern public static unsafe void CommandEncoderRelease(WGPUCommandEncoder* commandEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderBeginPipelineStatisticsQuery")]
    extern public static unsafe void ComputePassEncoderBeginPipelineStatisticsQuery(WGPUComputePassEncoder* computePassEncoder, WGPUQuerySet* querySet, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderDispatchWorkgroups")]
    extern public static unsafe void ComputePassEncoderDispatchWorkgroups(WGPUComputePassEncoder* computePassEncoder, uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderDispatchWorkgroupsIndirect")]
    extern public static unsafe void ComputePassEncoderDispatchWorkgroupsIndirect(WGPUComputePassEncoder* computePassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderEnd")]
    extern public static unsafe void ComputePassEncoderEnd(WGPUComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderEndPipelineStatisticsQuery")]
    extern public static unsafe void ComputePassEncoderEndPipelineStatisticsQuery(WGPUComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderInsertDebugMarker")]
    extern public static unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPopDebugGroup")]
    extern public static unsafe void ComputePassEncoderPopDebugGroup(WGPUComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderPushDebugGroup")]
    extern public static unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetBindGroup")]
    extern public static unsafe void ComputePassEncoderSetBindGroup(WGPUComputePassEncoder* computePassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetBindGroup")]
    extern public static unsafe void ComputePassEncoderSetBindGroup(WGPUComputePassEncoder* computePassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetLabel")]
    extern public static unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderSetPipeline")]
    extern public static unsafe void ComputePassEncoderSetPipeline(WGPUComputePassEncoder* computePassEncoder, WGPUComputePipeline* pipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderReference")]
    extern public static unsafe void ComputePassEncoderReference(WGPUComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePassEncoderRelease")]
    extern public static unsafe void ComputePassEncoderRelease(WGPUComputePassEncoder* computePassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineGetBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* ComputePipelineGetBindGroupLayout(WGPUComputePipeline* computePipeline, uint groupIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineSetLabel")]
    extern public static unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineReference")]
    extern public static unsafe void ComputePipelineReference(WGPUComputePipeline* computePipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuComputePipelineRelease")]
    extern public static unsafe void ComputePipelineRelease(WGPUComputePipeline* computePipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroup")]
    extern public static unsafe WGPUBindGroup* DeviceCreateBindGroup(WGPUDevice* device, WGPUBindGroupDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroup")]
    extern public static unsafe WGPUBindGroup* DeviceCreateBindGroup(WGPUDevice* device, in WGPUBindGroupDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* DeviceCreateBindGroupLayout(WGPUDevice* device, WGPUBindGroupLayoutDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* DeviceCreateBindGroupLayout(WGPUDevice* device, in WGPUBindGroupLayoutDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBuffer")]
    extern public static unsafe WGPUBuffer* DeviceCreateBuffer(WGPUDevice* device, WGPUBufferDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateBuffer")]
    extern public static unsafe WGPUBuffer* DeviceCreateBuffer(WGPUDevice* device, in WGPUBufferDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateCommandEncoder")]
    extern public static unsafe WGPUCommandEncoder* DeviceCreateCommandEncoder(WGPUDevice* device, WGPUCommandEncoderDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateCommandEncoder")]
    extern public static unsafe WGPUCommandEncoder* DeviceCreateCommandEncoder(WGPUDevice* device, in WGPUCommandEncoderDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipeline")]
    extern public static unsafe WGPUComputePipeline* DeviceCreateComputePipeline(WGPUDevice* device, WGPUComputePipelineDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipeline")]
    extern public static unsafe WGPUComputePipeline* DeviceCreateComputePipeline(WGPUDevice* device, in WGPUComputePipelineDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipelineAsync")]
    extern public static unsafe void DeviceCreateComputePipelineAsync(WGPUDevice* device, WGPUComputePipelineDescriptor* descriptor, Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateComputePipelineAsync")]
    extern public static unsafe void DeviceCreateComputePipelineAsync(WGPUDevice* device, in WGPUComputePipelineDescriptor descriptor, Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreatePipelineLayout")]
    extern public static unsafe WGPUPipelineLayout* DeviceCreatePipelineLayout(WGPUDevice* device, WGPUPipelineLayoutDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreatePipelineLayout")]
    extern public static unsafe WGPUPipelineLayout* DeviceCreatePipelineLayout(WGPUDevice* device, in WGPUPipelineLayoutDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateQuerySet")]
    extern public static unsafe WGPUQuerySet* DeviceCreateQuerySet(WGPUDevice* device, WGPUQuerySetDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateQuerySet")]
    extern public static unsafe WGPUQuerySet* DeviceCreateQuerySet(WGPUDevice* device, in WGPUQuerySetDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderBundleEncoder")]
    extern public static unsafe WGPURenderBundleEncoder* DeviceCreateRenderBundleEncoder(WGPUDevice* device, WGPURenderBundleEncoderDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderBundleEncoder")]
    extern public static unsafe WGPURenderBundleEncoder* DeviceCreateRenderBundleEncoder(WGPUDevice* device, in WGPURenderBundleEncoderDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipeline")]
    extern public static unsafe WGPURenderPipeline* DeviceCreateRenderPipeline(WGPUDevice* device, WGPURenderPipelineDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipeline")]
    extern public static unsafe WGPURenderPipeline* DeviceCreateRenderPipeline(WGPUDevice* device, in WGPURenderPipelineDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipelineAsync")]
    extern public static unsafe void DeviceCreateRenderPipelineAsync(WGPUDevice* device, WGPURenderPipelineDescriptor* descriptor, Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateRenderPipelineAsync")]
    extern public static unsafe void DeviceCreateRenderPipelineAsync(WGPUDevice* device, in WGPURenderPipelineDescriptor descriptor, Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateSampler")]
    extern public static unsafe WGPUSampler* DeviceCreateSampler(WGPUDevice* device, WGPUSamplerDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateSampler")]
    extern public static unsafe WGPUSampler* DeviceCreateSampler(WGPUDevice* device, in WGPUSamplerDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateShaderModule")]
    extern public static unsafe WGPUShaderModule* DeviceCreateShaderModule(WGPUDevice* device, WGPUShaderModuleDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateShaderModule")]
    extern public static unsafe WGPUShaderModule* DeviceCreateShaderModule(WGPUDevice* device, in WGPUShaderModuleDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateTexture")]
    extern public static unsafe WGPUTexture* DeviceCreateTexture(WGPUDevice* device, WGPUTextureDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateTexture")]
    extern public static unsafe WGPUTexture* DeviceCreateTexture(WGPUDevice* device, in WGPUTextureDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceDestroy")]
    extern public static unsafe void DeviceDestroy(WGPUDevice* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceEnumerateFeatures")]
    extern public static unsafe nuint DeviceEnumerateFeatures(WGPUDevice* device, WGPUFeatureName* features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceEnumerateFeatures")]
    extern public static unsafe nuint DeviceEnumerateFeatures(WGPUDevice* device, ref WGPUFeatureName features);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceGetLimits")]
    extern public static unsafe bool DeviceGetLimits(WGPUDevice* device, WGPUSupportedLimits* limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceGetLimits")]
    extern public static unsafe bool DeviceGetLimits(WGPUDevice* device, ref WGPUSupportedLimits limits);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceGetQueue")]
    extern public static unsafe WGPUQueue* DeviceGetQueue(WGPUDevice* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceHasFeature")]
    extern public static unsafe bool DeviceHasFeature(WGPUDevice* device, WGPUFeatureName feature);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDevicePopErrorScope")]
    extern public static unsafe void DevicePopErrorScope(WGPUDevice* device, Silk.NET.WebGPU.PfnErrorCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDevicePushErrorScope")]
    extern public static unsafe void DevicePushErrorScope(WGPUDevice* device, WGPUErrorFilter filter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(WGPUDevice* device, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(WGPUDevice* device, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetLabel")]
    extern public static unsafe void DeviceSetLabel(WGPUDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceSetUncapturedErrorCallback")]
    extern public static unsafe void DeviceSetUncapturedErrorCallback(WGPUDevice* device, Silk.NET.WebGPU.PfnErrorCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceReference")]
    extern public static unsafe void DeviceReference(WGPUDevice* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceRelease")]
    extern public static unsafe void DeviceRelease(WGPUDevice* device);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceCreateSurface")]
    extern public static unsafe WGPUSurface* InstanceCreateSurface(WGPUInstance* instance, WGPUSurfaceDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceCreateSurface")]
    extern public static unsafe WGPUSurface* InstanceCreateSurface(WGPUInstance* instance, in WGPUSurfaceDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceProcessEvents")]
    extern public static unsafe void InstanceProcessEvents(WGPUInstance* instance);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceRequestAdapter")]
    extern public static unsafe void InstanceRequestAdapter(WGPUInstance* instance, WGPURequestAdapterOptions* options, nint callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceRequestAdapter")]
    extern public static unsafe void InstanceRequestAdapter(WGPUInstance* instance, in WGPURequestAdapterOptions options, nint callback, void* userdata);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceReference")]
    extern public static unsafe void InstanceReference(WGPUInstance* instance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuInstanceRelease")]
    extern public static unsafe void InstanceRelease(WGPUInstance* instance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutSetLabel")]
    extern public static unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutReference")]
    extern public static unsafe void PipelineLayoutReference(WGPUPipelineLayout* pipelineLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuPipelineLayoutRelease")]
    extern public static unsafe void PipelineLayoutRelease(WGPUPipelineLayout* pipelineLayout);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetDestroy")]
    extern public static unsafe void QuerySetDestroy(WGPUQuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetGetCount")]
    extern public static unsafe uint QuerySetGetCount(WGPUQuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetGetType")]
    extern public static unsafe WGPUQueryType QuerySetGetType(WGPUQuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetSetLabel")]
    extern public static unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetReference")]
    extern public static unsafe void QuerySetReference(WGPUQuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQuerySetRelease")]
    extern public static unsafe void QuerySetRelease(WGPUQuerySet* querySet);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueOnSubmittedWorkDone")]
    extern public static unsafe void QueueOnSubmittedWorkDone(WGPUQueue* queue, Silk.NET.WebGPU.PfnQueueWorkDoneCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(WGPUQueue* queue, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(WGPUQueue* queue, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSetLabel")]
    extern public static unsafe void QueueSetLabel(WGPUQueue* queue, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSubmit")]
    extern public static unsafe void QueueSubmit(WGPUQueue* queue, nuint commandCount, WGPUCommandBuffer** commands);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueSubmit")]
    extern public static unsafe void QueueSubmit(WGPUQueue* queue, nuint commandCount, ref WGPUCommandBuffer* commands);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteBuffer")]
    extern public static unsafe void QueueWriteBuffer(WGPUQueue* queue, WGPUBuffer* buffer, ulong bufferOffset, void* data, nuint size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, in WGPUExtent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, WGPUExtent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, in WGPUExtent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, in WGPUExtent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, WGPUExtent3D* writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueWriteTexture")]
    extern public static unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, in WGPUExtent3D writeSize);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueReference")]
    extern public static unsafe void QueueReference(WGPUQueue* queue);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuQueueRelease")]
    extern public static unsafe void QueueRelease(WGPUQueue* queue);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleSetLabel")]
    extern public static unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleReference")]
    extern public static unsafe void RenderBundleReference(WGPURenderBundle* renderBundle);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleRelease")]
    extern public static unsafe void RenderBundleRelease(WGPURenderBundle* renderBundle);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDraw")]
    extern public static unsafe void RenderBundleEncoderDraw(WGPURenderBundleEncoder* renderBundleEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDrawIndexed")]
    extern public static unsafe void RenderBundleEncoderDrawIndexed(WGPURenderBundleEncoder* renderBundleEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDrawIndexedIndirect")]
    extern public static unsafe void RenderBundleEncoderDrawIndexedIndirect(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderDrawIndirect")]
    extern public static unsafe void RenderBundleEncoderDrawIndirect(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderFinish")]
    extern public static unsafe WGPURenderBundle* RenderBundleEncoderFinish(WGPURenderBundleEncoder* renderBundleEncoder, WGPURenderBundleDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderFinish")]
    extern public static unsafe WGPURenderBundle* RenderBundleEncoderFinish(WGPURenderBundleEncoder* renderBundleEncoder, in WGPURenderBundleDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderInsertDebugMarker")]
    extern public static unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPopDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPopDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderPushDebugGroup")]
    extern public static unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetBindGroup")]
    extern public static unsafe void RenderBundleEncoderSetBindGroup(WGPURenderBundleEncoder* renderBundleEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetBindGroup")]
    extern public static unsafe void RenderBundleEncoderSetBindGroup(WGPURenderBundleEncoder* renderBundleEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetIndexBuffer")]
    extern public static unsafe void RenderBundleEncoderSetIndexBuffer(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* buffer, WGPUIndexFormat format, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetLabel")]
    extern public static unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetPipeline")]
    extern public static unsafe void RenderBundleEncoderSetPipeline(WGPURenderBundleEncoder* renderBundleEncoder, WGPURenderPipeline* pipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderSetVertexBuffer")]
    extern public static unsafe void RenderBundleEncoderSetVertexBuffer(WGPURenderBundleEncoder* renderBundleEncoder, uint slot, WGPUBuffer* buffer, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderReference")]
    extern public static unsafe void RenderBundleEncoderReference(WGPURenderBundleEncoder* renderBundleEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderBundleEncoderRelease")]
    extern public static unsafe void RenderBundleEncoderRelease(WGPURenderBundleEncoder* renderBundleEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderBeginOcclusionQuery")]
    extern public static unsafe void RenderPassEncoderBeginOcclusionQuery(WGPURenderPassEncoder* renderPassEncoder, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderBeginPipelineStatisticsQuery")]
    extern public static unsafe void RenderPassEncoderBeginPipelineStatisticsQuery(WGPURenderPassEncoder* renderPassEncoder, WGPUQuerySet* querySet, uint queryIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDraw")]
    extern public static unsafe void RenderPassEncoderDraw(WGPURenderPassEncoder* renderPassEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDrawIndexed")]
    extern public static unsafe void RenderPassEncoderDrawIndexed(WGPURenderPassEncoder* renderPassEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDrawIndexedIndirect")]
    extern public static unsafe void RenderPassEncoderDrawIndexedIndirect(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderDrawIndirect")]
    extern public static unsafe void RenderPassEncoderDrawIndirect(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderEnd")]
    extern public static unsafe void RenderPassEncoderEnd(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderEndOcclusionQuery")]
    extern public static unsafe void RenderPassEncoderEndOcclusionQuery(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderEndPipelineStatisticsQuery")]
    extern public static unsafe void RenderPassEncoderEndPipelineStatisticsQuery(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderExecuteBundles")]
    extern public static unsafe void RenderPassEncoderExecuteBundles(WGPURenderPassEncoder* renderPassEncoder, nuint bundleCount, WGPURenderBundle** bundles);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderExecuteBundles")]
    extern public static unsafe void RenderPassEncoderExecuteBundles(WGPURenderPassEncoder* renderPassEncoder, nuint bundleCount, ref WGPURenderBundle* bundles);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, byte* markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, in byte markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderInsertDebugMarker")]
    extern public static unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string markerLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPopDebugGroup")]
    extern public static unsafe void RenderPassEncoderPopDebugGroup(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, byte* groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, in byte groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderPushDebugGroup")]
    extern public static unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string groupLabel);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBindGroup")]
    extern public static unsafe void RenderPassEncoderSetBindGroup(WGPURenderPassEncoder* renderPassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBindGroup")]
    extern public static unsafe void RenderPassEncoderSetBindGroup(WGPURenderPassEncoder* renderPassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBlendConstant")]
    extern public static unsafe void RenderPassEncoderSetBlendConstant(WGPURenderPassEncoder* renderPassEncoder, WGPUColor* color);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetBlendConstant")]
    extern public static unsafe void RenderPassEncoderSetBlendConstant(WGPURenderPassEncoder* renderPassEncoder, in WGPUColor color);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetIndexBuffer")]
    extern public static unsafe void RenderPassEncoderSetIndexBuffer(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* buffer, WGPUIndexFormat format, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetLabel")]
    extern public static unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetPipeline")]
    extern public static unsafe void RenderPassEncoderSetPipeline(WGPURenderPassEncoder* renderPassEncoder, WGPURenderPipeline* pipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetScissorRect")]
    extern public static unsafe void RenderPassEncoderSetScissorRect(WGPURenderPassEncoder* renderPassEncoder, uint x, uint y, uint width, uint height);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetStencilReference")]
    extern public static unsafe void RenderPassEncoderSetStencilReference(WGPURenderPassEncoder* renderPassEncoder, uint reference);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetVertexBuffer")]
    extern public static unsafe void RenderPassEncoderSetVertexBuffer(WGPURenderPassEncoder* renderPassEncoder, uint slot, WGPUBuffer* buffer, ulong offset, ulong size);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderSetViewport")]
    extern public static unsafe void RenderPassEncoderSetViewport(WGPURenderPassEncoder* renderPassEncoder, float x, float y, float width, float height, float minDepth, float maxDepth);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderReference")]
    extern public static unsafe void RenderPassEncoderReference(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPassEncoderRelease")]
    extern public static unsafe void RenderPassEncoderRelease(WGPURenderPassEncoder* renderPassEncoder);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineGetBindGroupLayout")]
    extern public static unsafe WGPUBindGroupLayout* RenderPipelineGetBindGroupLayout(WGPURenderPipeline* renderPipeline, uint groupIndex);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineSetLabel")]
    extern public static unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineReference")]
    extern public static unsafe void RenderPipelineReference(WGPURenderPipeline* renderPipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuRenderPipelineRelease")]
    extern public static unsafe void RenderPipelineRelease(WGPURenderPipeline* renderPipeline);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(WGPUSampler* sampler, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(WGPUSampler* sampler, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerSetLabel")]
    extern public static unsafe void SamplerSetLabel(WGPUSampler* sampler, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerReference")]
    extern public static unsafe void SamplerReference(WGPUSampler* sampler);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSamplerRelease")]
    extern public static unsafe void SamplerRelease(WGPUSampler* sampler);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleGetCompilationInfo")]
    extern public static unsafe void ShaderModuleGetCompilationInfo(WGPUShaderModule* shaderModule, Silk.NET.WebGPU.PfnCompilationInfoCallback callback, void* userdata);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleSetLabel")]
    extern public static unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleReference")]
    extern public static unsafe void ShaderModuleReference(WGPUShaderModule* shaderModule);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuShaderModuleRelease")]
    extern public static unsafe void ShaderModuleRelease(WGPUShaderModule* shaderModule);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceGetPreferredFormat")]
    extern public static unsafe WGPUTextureFormat SurfaceGetPreferredFormat(WGPUSurface* surface, WGPUAdapter* adapter);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfacePresent")]
    extern public static unsafe void SurfacePresent(WGPUSurface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceUnconfigure")]
    extern public static unsafe void SurfaceUnconfigure(WGPUSurface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceReference")]
    extern public static unsafe void SurfaceReference(WGPUSurface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSurfaceRelease")]
    extern public static unsafe void SurfaceRelease(WGPUSurface* surface);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureCreateView")]
    extern public static unsafe WGPUTextureView* TextureCreateView(WGPUTexture* texture, WGPUTextureViewDescriptor* descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureCreateView")]
    extern public static unsafe WGPUTextureView* TextureCreateView(WGPUTexture* texture, in WGPUTextureViewDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureDestroy")]
    extern public static unsafe void TextureDestroy(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetDepthOrArrayLayers")]
    extern public static unsafe uint TextureGetDepthOrArrayLayers(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetDimension")]
    extern public static unsafe WGPUTextureDimension TextureGetDimension(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetFormat")]
    extern public static unsafe WGPUTextureFormat TextureGetFormat(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetHeight")]
    extern public static unsafe uint TextureGetHeight(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetMipLevelCount")]
    extern public static unsafe uint TextureGetMipLevelCount(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetSampleCount")]
    extern public static unsafe uint TextureGetSampleCount(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetUsage")]
    extern public static unsafe WGPUTextureUsage TextureGetUsage(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureGetWidth")]
    extern public static unsafe uint TextureGetWidth(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(WGPUTexture* texture, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(WGPUTexture* texture, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureSetLabel")]
    extern public static unsafe void TextureSetLabel(WGPUTexture* texture, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureReference")]
    extern public static unsafe void TextureReference(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureRelease")]
    extern public static unsafe void TextureRelease(WGPUTexture* texture);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(WGPUTextureView* textureView, byte* label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(WGPUTextureView* textureView, in byte label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewSetLabel")]
    extern public static unsafe void TextureViewSetLabel(WGPUTextureView* textureView, [MarshalAs(UnmanagedType.LPUTF8Str)] string label);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewReference")]
    extern public static unsafe void TextureViewReference(WGPUTextureView* textureView);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuTextureViewRelease")]
    extern public static unsafe void TextureViewRelease(WGPUTextureView* textureView);

    [DllImport("__Internal_emscripten", EntryPoint = "wgpuDeviceCreateSwapChain")]
    extern public static unsafe WGPUSwapChain* DeviceCreateSwapChain(WGPUDevice* device, WGPUSurface* surface, in WGPUSwapChainDescriptor descriptor);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSwapChainGetCurrentTextureView")]
    extern public static unsafe WGPUTextureView* SwapChainGetCurrentTextureView(WGPUSwapChain* swapChain);
    [DllImport("__Internal_emscripten", EntryPoint = "wgpuSwapChainRelease")]
    extern public static unsafe void SwapChainRelease(WGPUSwapChain* swapChain);
}
