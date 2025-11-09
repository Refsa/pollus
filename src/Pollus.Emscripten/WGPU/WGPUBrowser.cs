namespace Pollus.Emscripten;

using WGPU;

public partial class WGPUBrowser : IDisposable
{
    public void Dispose() { }

    public unsafe WGPUInstance* CreateInstance(WGPUInstanceDescriptor* descriptor)
    {
        return WGPUBrowserNative.CreateInstance(descriptor);
    }
    public unsafe WGPUInstance* CreateInstance(in WGPUInstanceDescriptor descriptor)
    {
        return WGPUBrowserNative.CreateInstance(descriptor);
    }
    public unsafe Silk.NET.WebGPU.PfnProc GetProcAddress(WGPUDevice* device, byte* procName)
    {
        return WGPUBrowserNative.GetProcAddress(device, procName);
    }
    public unsafe Silk.NET.WebGPU.PfnProc GetProcAddress(WGPUDevice* device, in byte procName)
    {
        return WGPUBrowserNative.GetProcAddress(device, procName);
    }
    public unsafe Silk.NET.WebGPU.PfnProc GetProcAddress(WGPUDevice* device, string procName)
    {
        return WGPUBrowserNative.GetProcAddress(device, procName);
    }
    public unsafe nuint AdapterEnumerateFeatures(WGPUAdapter* adapter, WGPUFeatureName* features)
    {
        return WGPUBrowserNative.AdapterEnumerateFeatures(adapter, features);
    }
    public unsafe nuint AdapterEnumerateFeatures(WGPUAdapter* adapter, ref WGPUFeatureName features)
    {
        return WGPUBrowserNative.AdapterEnumerateFeatures(adapter, ref features);
    }
    public unsafe bool AdapterGetLimits(WGPUAdapter* adapter, WGPUSupportedLimits* limits)
    {
        return WGPUBrowserNative.AdapterGetLimits(adapter, limits);
    }
    public unsafe bool AdapterGetLimits(WGPUAdapter* adapter, ref WGPUSupportedLimits limits)
    {
        return WGPUBrowserNative.AdapterGetLimits(adapter, ref limits);
    }
    public unsafe void AdapterGetProperties(WGPUAdapter* adapter, WGPUAdapterProperties* properties)
    {
        WGPUBrowserNative.AdapterGetProperties(adapter, properties);
    }
    public unsafe void AdapterGetProperties(WGPUAdapter* adapter, ref WGPUAdapterProperties properties)
    {
        WGPUBrowserNative.AdapterGetProperties(adapter, ref properties);
    }
    public unsafe bool AdapterHasFeature(WGPUAdapter* adapter, WGPUFeatureName feature)
    {
        return WGPUBrowserNative.AdapterHasFeature(adapter, feature);
    }

    public unsafe void AdapterRequestDevice(WGPUAdapter* adapter, WGPUDeviceDescriptor* descriptor, delegate* unmanaged[Cdecl]<WGPURequestDeviceStatus, WGPUDevice*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.AdapterRequestDevice(adapter, descriptor, (nint)callback, userdata);
    }
    public unsafe void AdapterRequestDevice(WGPUAdapter* adapter, in WGPUDeviceDescriptor descriptor, delegate* unmanaged[Cdecl]<WGPURequestDeviceStatus, WGPUDevice*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.AdapterRequestDevice(adapter, descriptor, (nint)callback, userdata);
    }

    public unsafe void AdapterReference(WGPUAdapter* adapter)
    {
        WGPUBrowserNative.AdapterReference(adapter);
    }
    public unsafe void AdapterRelease(WGPUAdapter* adapter)
    {
        WGPUBrowserNative.AdapterRelease(adapter);
    }
    public unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, byte* label)
    {
        WGPUBrowserNative.BindGroupSetLabel(bindGroup, label);
    }
    public unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, in byte label)
    {
        WGPUBrowserNative.BindGroupSetLabel(bindGroup, label);
    }
    public unsafe void BindGroupSetLabel(WGPUBindGroup* bindGroup, string label)
    {
        WGPUBrowserNative.BindGroupSetLabel(bindGroup, label);
    }
    public unsafe void BindGroupReference(WGPUBindGroup* bindGroup)
    {
        WGPUBrowserNative.BindGroupReference(bindGroup);
    }
    public unsafe void BindGroupRelease(WGPUBindGroup* bindGroup)
    {
        WGPUBrowserNative.BindGroupRelease(bindGroup);
    }
    public unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, byte* label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(bindGroupLayout, label);
    }
    public unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, in byte label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(bindGroupLayout, label);
    }
    public unsafe void BindGroupLayoutSetLabel(WGPUBindGroupLayout* bindGroupLayout, string label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(bindGroupLayout, label);
    }
    public unsafe void BindGroupLayoutReference(WGPUBindGroupLayout* bindGroupLayout)
    {
        WGPUBrowserNative.BindGroupLayoutReference(bindGroupLayout);
    }
    public unsafe void BindGroupLayoutRelease(WGPUBindGroupLayout* bindGroupLayout)
    {
        WGPUBrowserNative.BindGroupLayoutRelease(bindGroupLayout);
    }
    public unsafe void BufferDestroy(WGPUBuffer* buffer)
    {
        WGPUBrowserNative.BufferDestroy(buffer);
    }
    public unsafe void* BufferGetConstMappedRange(WGPUBuffer* buffer, nuint offset, nuint size)
    {
        return WGPUBrowserNative.BufferGetConstMappedRange(buffer, offset, size);
    }
    public unsafe WGPUBufferMapState BufferGetMapState(WGPUBuffer* buffer)
    {
        return WGPUBrowserNative.BufferGetMapState(buffer);
    }
    public unsafe void* BufferGetMappedRange(WGPUBuffer* buffer, nuint offset, nuint size)
    {
        return WGPUBrowserNative.BufferGetMappedRange(buffer, offset, size);
    }
    public unsafe ulong BufferGetSize(WGPUBuffer* buffer)
    {
        return WGPUBrowserNative.BufferGetSize(buffer);
    }
    public unsafe WGPUBufferUsage BufferGetUsage(WGPUBuffer* buffer)
    {
        return WGPUBrowserNative.BufferGetUsage(buffer);
    }
    public unsafe void BufferMapAsync(WGPUBuffer* buffer, WGPUMapMode mode, nuint offset, nuint size, Silk.NET.WebGPU.PfnBufferMapCallback callback, void* userdata)
    {
        WGPUBrowserNative.BufferMapAsync(buffer, mode, offset, size, callback, userdata);
    }
    public unsafe void BufferSetLabel(WGPUBuffer* buffer, byte* label)
    {
        WGPUBrowserNative.BufferSetLabel(buffer, label);
    }
    public unsafe void BufferSetLabel(WGPUBuffer* buffer, in byte label)
    {
        WGPUBrowserNative.BufferSetLabel(buffer, label);
    }
    public unsafe void BufferSetLabel(WGPUBuffer* buffer, string label)
    {
        WGPUBrowserNative.BufferSetLabel(buffer, label);
    }
    public unsafe void BufferUnmap(WGPUBuffer* buffer)
    {
        WGPUBrowserNative.BufferUnmap(buffer);
    }
    public unsafe void BufferReference(WGPUBuffer* buffer)
    {
        WGPUBrowserNative.BufferReference(buffer);
    }
    public unsafe void BufferRelease(WGPUBuffer* buffer)
    {
        WGPUBrowserNative.BufferRelease(buffer);
    }
    public unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, byte* label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(commandBuffer, label);
    }
    public unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, in byte label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(commandBuffer, label);
    }
    public unsafe void CommandBufferSetLabel(WGPUCommandBuffer* commandBuffer, string label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(commandBuffer, label);
    }
    public unsafe void CommandBufferReference(WGPUCommandBuffer* commandBuffer)
    {
        WGPUBrowserNative.CommandBufferReference(commandBuffer);
    }
    public unsafe void CommandBufferRelease(WGPUCommandBuffer* commandBuffer)
    {
        WGPUBrowserNative.CommandBufferRelease(commandBuffer);
    }
    public unsafe WGPUComputePassEncoder* CommandEncoderBeginComputePass(WGPUCommandEncoder* commandEncoder, WGPUComputePassDescriptor* descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginComputePass(commandEncoder, descriptor);
    }
    public unsafe WGPUComputePassEncoder* CommandEncoderBeginComputePass(WGPUCommandEncoder* commandEncoder, in WGPUComputePassDescriptor descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginComputePass(commandEncoder, descriptor);
    }
    public unsafe WGPURenderPassEncoder* CommandEncoderBeginRenderPass(WGPUCommandEncoder* commandEncoder, WGPU.WGPURenderPassDescriptor* descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }
    public unsafe WGPURenderPassEncoder* CommandEncoderBeginRenderPass(WGPUCommandEncoder* commandEncoder, in WGPU.WGPURenderPassDescriptor descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }
    public unsafe void CommandEncoderClearBuffer(WGPUCommandEncoder* commandEncoder, WGPUBuffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.CommandEncoderClearBuffer(commandEncoder, buffer, offset, size);
    }
    public unsafe void CommandEncoderCopyBufferToBuffer(WGPUCommandEncoder* commandEncoder, WGPUBuffer* source, ulong sourceOffset, WGPUBuffer* destination, ulong destinationOffset, ulong size)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToBuffer(commandEncoder, source, sourceOffset, destination, destinationOffset, size);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyBuffer* source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyBuffer source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyBuffer* destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyBuffer* destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyBuffer destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyBuffer destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyBuffer* destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyBuffer* destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyBuffer destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyBuffer destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, WGPUImageCopyTexture* source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyTexture* destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, WGPUImageCopyTexture* destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyTexture destination, WGPUExtent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(WGPUCommandEncoder* commandEncoder, in WGPUImageCopyTexture source, in WGPUImageCopyTexture destination, in WGPUExtent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe WGPUCommandBuffer* CommandEncoderFinish(WGPUCommandEncoder* commandEncoder, WGPUCommandBufferDescriptor* descriptor)
    {
        return WGPUBrowserNative.CommandEncoderFinish(commandEncoder, descriptor);
    }
    public unsafe WGPUCommandBuffer* CommandEncoderFinish(WGPUCommandEncoder* commandEncoder, in WGPUCommandBufferDescriptor descriptor)
    {
        return WGPUBrowserNative.CommandEncoderFinish(commandEncoder, descriptor);
    }
    public unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(commandEncoder, markerLabel);
    }
    public unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(commandEncoder, markerLabel);
    }
    public unsafe void CommandEncoderInsertDebugMarker(WGPUCommandEncoder* commandEncoder, string markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(commandEncoder, markerLabel);
    }
    public unsafe void CommandEncoderPopDebugGroup(WGPUCommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderPopDebugGroup(commandEncoder);
    }
    public unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(commandEncoder, groupLabel);
    }
    public unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(commandEncoder, groupLabel);
    }
    public unsafe void CommandEncoderPushDebugGroup(WGPUCommandEncoder* commandEncoder, string groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(commandEncoder, groupLabel);
    }
    public unsafe void CommandEncoderResolveQuerySet(WGPUCommandEncoder* commandEncoder, WGPUQuerySet* querySet, uint firstQuery, uint queryCount, WGPUBuffer* destination, ulong destinationOffset)
    {
        WGPUBrowserNative.CommandEncoderResolveQuerySet(commandEncoder, querySet, firstQuery, queryCount, destination, destinationOffset);
    }
    public unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, byte* label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(commandEncoder, label);
    }
    public unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, in byte label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(commandEncoder, label);
    }
    public unsafe void CommandEncoderSetLabel(WGPUCommandEncoder* commandEncoder, string label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(commandEncoder, label);
    }
    public unsafe void CommandEncoderWriteTimestamp(WGPUCommandEncoder* commandEncoder, WGPUQuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.CommandEncoderWriteTimestamp(commandEncoder, querySet, queryIndex);
    }
    public unsafe void CommandEncoderReference(WGPUCommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderReference(commandEncoder);
    }
    public unsafe void CommandEncoderRelease(WGPUCommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderRelease(commandEncoder);
    }
    public unsafe void ComputePassEncoderBeginPipelineStatisticsQuery(WGPUComputePassEncoder* computePassEncoder, WGPUQuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.ComputePassEncoderBeginPipelineStatisticsQuery(computePassEncoder, querySet, queryIndex);
    }
    public unsafe void ComputePassEncoderDispatchWorkgroups(WGPUComputePassEncoder* computePassEncoder, uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ)
    {
        WGPUBrowserNative.ComputePassEncoderDispatchWorkgroups(computePassEncoder, workgroupCountX, workgroupCountY, workgroupCountZ);
    }
    public unsafe void ComputePassEncoderDispatchWorkgroupsIndirect(WGPUComputePassEncoder* computePassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.ComputePassEncoderDispatchWorkgroupsIndirect(computePassEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void ComputePassEncoderEnd(WGPUComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderEnd(computePassEncoder);
    }
    public unsafe void ComputePassEncoderEndPipelineStatisticsQuery(WGPUComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderEndPipelineStatisticsQuery(computePassEncoder);
    }
    public unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(computePassEncoder, markerLabel);
    }
    public unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(computePassEncoder, markerLabel);
    }
    public unsafe void ComputePassEncoderInsertDebugMarker(WGPUComputePassEncoder* computePassEncoder, string markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(computePassEncoder, markerLabel);
    }
    public unsafe void ComputePassEncoderPopDebugGroup(WGPUComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderPopDebugGroup(computePassEncoder);
    }
    public unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(computePassEncoder, groupLabel);
    }
    public unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(computePassEncoder, groupLabel);
    }
    public unsafe void ComputePassEncoderPushDebugGroup(WGPUComputePassEncoder* computePassEncoder, string groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(computePassEncoder, groupLabel);
    }
    public unsafe void ComputePassEncoderSetBindGroup(WGPUComputePassEncoder* computePassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.ComputePassEncoderSetBindGroup(computePassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void ComputePassEncoderSetBindGroup(WGPUComputePassEncoder* computePassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.ComputePassEncoderSetBindGroup(computePassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, byte* label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(computePassEncoder, label);
    }
    public unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, in byte label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(computePassEncoder, label);
    }
    public unsafe void ComputePassEncoderSetLabel(WGPUComputePassEncoder* computePassEncoder, string label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(computePassEncoder, label);
    }
    public unsafe void ComputePassEncoderSetPipeline(WGPUComputePassEncoder* computePassEncoder, WGPUComputePipeline* pipeline)
    {
        WGPUBrowserNative.ComputePassEncoderSetPipeline(computePassEncoder, pipeline);
    }
    public unsafe void ComputePassEncoderReference(WGPUComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderReference(computePassEncoder);
    }
    public unsafe void ComputePassEncoderRelease(WGPUComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderRelease(computePassEncoder);
    }
    public unsafe WGPUBindGroupLayout* ComputePipelineGetBindGroupLayout(WGPUComputePipeline* computePipeline, uint groupIndex)
    {
        return WGPUBrowserNative.ComputePipelineGetBindGroupLayout(computePipeline, groupIndex);
    }
    public unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, byte* label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(computePipeline, label);
    }
    public unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, in byte label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(computePipeline, label);
    }
    public unsafe void ComputePipelineSetLabel(WGPUComputePipeline* computePipeline, string label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(computePipeline, label);
    }
    public unsafe void ComputePipelineReference(WGPUComputePipeline* computePipeline)
    {
        WGPUBrowserNative.ComputePipelineReference(computePipeline);
    }
    public unsafe void ComputePipelineRelease(WGPUComputePipeline* computePipeline)
    {
        WGPUBrowserNative.ComputePipelineRelease(computePipeline);
    }
    public unsafe WGPUBindGroup* DeviceCreateBindGroup(WGPUDevice* device, WGPUBindGroupDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroup(device, descriptor);
    }
    public unsafe WGPUBindGroup* DeviceCreateBindGroup(WGPUDevice* device, in WGPUBindGroupDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroup(device, descriptor);
    }
    public unsafe WGPUBindGroupLayout* DeviceCreateBindGroupLayout(WGPUDevice* device, WGPUBindGroupLayoutDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroupLayout(device, descriptor);
    }
    public unsafe WGPUBindGroupLayout* DeviceCreateBindGroupLayout(WGPUDevice* device, in WGPUBindGroupLayoutDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroupLayout(device, descriptor);
    }
    public unsafe WGPUBuffer* DeviceCreateBuffer(WGPUDevice* device, WGPUBufferDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBuffer(device, descriptor);
    }
    public unsafe WGPUBuffer* DeviceCreateBuffer(WGPUDevice* device, in WGPUBufferDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBuffer(device, descriptor);
    }
    public unsafe WGPUCommandEncoder* DeviceCreateCommandEncoder(WGPUDevice* device, WGPUCommandEncoderDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateCommandEncoder(device, descriptor);
    }
    public unsafe WGPUCommandEncoder* DeviceCreateCommandEncoder(WGPUDevice* device, in WGPUCommandEncoderDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateCommandEncoder(device, descriptor);
    }
    public unsafe WGPUComputePipeline* DeviceCreateComputePipeline(WGPUDevice* device, WGPUComputePipelineDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateComputePipeline(device, descriptor);
    }
    public unsafe WGPUComputePipeline* DeviceCreateComputePipeline(WGPUDevice* device, in WGPUComputePipelineDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateComputePipeline(device, descriptor);
    }
    public unsafe void DeviceCreateComputePipelineAsync(WGPUDevice* device, WGPUComputePipelineDescriptor* descriptor, Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateComputePipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe void DeviceCreateComputePipelineAsync(WGPUDevice* device, in WGPUComputePipelineDescriptor descriptor, Silk.NET.WebGPU.PfnCreateComputePipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateComputePipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe WGPUPipelineLayout* DeviceCreatePipelineLayout(WGPUDevice* device, WGPUPipelineLayoutDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreatePipelineLayout(device, descriptor);
    }
    public unsafe WGPUPipelineLayout* DeviceCreatePipelineLayout(WGPUDevice* device, in WGPUPipelineLayoutDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreatePipelineLayout(device, descriptor);
    }
    public unsafe WGPUQuerySet* DeviceCreateQuerySet(WGPUDevice* device, WGPUQuerySetDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateQuerySet(device, descriptor);
    }
    public unsafe WGPUQuerySet* DeviceCreateQuerySet(WGPUDevice* device, in WGPUQuerySetDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateQuerySet(device, descriptor);
    }
    public unsafe WGPURenderBundleEncoder* DeviceCreateRenderBundleEncoder(WGPUDevice* device, WGPURenderBundleEncoderDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderBundleEncoder(device, descriptor);
    }
    public unsafe WGPURenderBundleEncoder* DeviceCreateRenderBundleEncoder(WGPUDevice* device, in WGPURenderBundleEncoderDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderBundleEncoder(device, descriptor);
    }
    public unsafe WGPURenderPipeline* DeviceCreateRenderPipeline(WGPUDevice* device, WGPURenderPipelineDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderPipeline(device, descriptor);
    }
    public unsafe WGPURenderPipeline* DeviceCreateRenderPipeline(WGPUDevice* device, in WGPURenderPipelineDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderPipeline(device, descriptor);
    }
    public unsafe void DeviceCreateRenderPipelineAsync(WGPUDevice* device, WGPURenderPipelineDescriptor* descriptor, Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateRenderPipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe void DeviceCreateRenderPipelineAsync(WGPUDevice* device, in WGPURenderPipelineDescriptor descriptor, Silk.NET.WebGPU.PfnCreateRenderPipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateRenderPipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe WGPUSampler* DeviceCreateSampler(WGPUDevice* device, WGPUSamplerDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateSampler(device, descriptor);
    }
    public unsafe WGPUSampler* DeviceCreateSampler(WGPUDevice* device, in WGPUSamplerDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateSampler(device, descriptor);
    }
    public unsafe WGPUShaderModule* DeviceCreateShaderModule(WGPUDevice* device, WGPUShaderModuleDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateShaderModule(device, descriptor);
    }
    public unsafe WGPUShaderModule* DeviceCreateShaderModule(WGPUDevice* device, in WGPUShaderModuleDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateShaderModule(device, descriptor);
    }
    public unsafe WGPUTexture* DeviceCreateTexture(WGPUDevice* device, WGPUTextureDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateTexture(device, descriptor);
    }
    public unsafe WGPUTexture* DeviceCreateTexture(WGPUDevice* device, in WGPUTextureDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateTexture(device, descriptor);
    }
    public unsafe void DeviceDestroy(WGPUDevice* device)
    {
        WGPUBrowserNative.DeviceDestroy(device);
    }
    public unsafe nuint DeviceEnumerateFeatures(WGPUDevice* device, WGPUFeatureName* features)
    {
        return WGPUBrowserNative.DeviceEnumerateFeatures(device, features);
    }
    public unsafe nuint DeviceEnumerateFeatures(WGPUDevice* device, ref WGPUFeatureName features)
    {
        return WGPUBrowserNative.DeviceEnumerateFeatures(device, ref features);
    }
    public unsafe bool DeviceGetLimits(WGPUDevice* device, WGPUSupportedLimits* limits)
    {
        return WGPUBrowserNative.DeviceGetLimits(device, limits);
    }
    public unsafe bool DeviceGetLimits(WGPUDevice* device, ref WGPUSupportedLimits limits)
    {
        return WGPUBrowserNative.DeviceGetLimits(device, ref limits);
    }
    public unsafe WGPUQueue* DeviceGetQueue(WGPUDevice* device)
    {
        return WGPUBrowserNative.DeviceGetQueue(device);
    }
    public unsafe bool DeviceHasFeature(WGPUDevice* device, WGPUFeatureName feature)
    {
        return WGPUBrowserNative.DeviceHasFeature(device, feature);
    }
    public unsafe void DevicePopErrorScope(WGPUDevice* device, Silk.NET.WebGPU.PfnErrorCallback callback, void* userdata)
    {
        WGPUBrowserNative.DevicePopErrorScope(device, callback, userdata);
    }
    public unsafe void DevicePushErrorScope(WGPUDevice* device, WGPUErrorFilter filter)
    {
        WGPUBrowserNative.DevicePushErrorScope(device, filter);
    }
    public unsafe void DeviceSetLabel(WGPUDevice* device, byte* label)
    {
        WGPUBrowserNative.DeviceSetLabel(device, label);
    }
    public unsafe void DeviceSetLabel(WGPUDevice* device, in byte label)
    {
        WGPUBrowserNative.DeviceSetLabel(device, label);
    }
    public unsafe void DeviceSetLabel(WGPUDevice* device, string label)
    {
        WGPUBrowserNative.DeviceSetLabel(device, label);
    }
    public unsafe void DeviceSetUncapturedErrorCallback(WGPUDevice* device, Silk.NET.WebGPU.PfnErrorCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceSetUncapturedErrorCallback(device, callback, userdata);
    }
    public unsafe void DeviceReference(WGPUDevice* device)
    {
        WGPUBrowserNative.DeviceReference(device);
    }
    public unsafe void DeviceRelease(WGPUDevice* device)
    {
        WGPUBrowserNative.DeviceRelease(device);
    }
    public unsafe WGPUSurface* InstanceCreateSurface(WGPUInstance* instance, WGPUSurfaceDescriptor* descriptor)
    {
        return WGPUBrowserNative.InstanceCreateSurface(instance, descriptor);
    }
    public unsafe WGPUSurface* InstanceCreateSurface(WGPUInstance* instance, in WGPUSurfaceDescriptor descriptor)
    {
        return WGPUBrowserNative.InstanceCreateSurface(instance, descriptor);
    }
    public unsafe void InstanceProcessEvents(WGPUInstance* instance)
    {
        WGPUBrowserNative.InstanceProcessEvents(instance);
    }

    public unsafe void InstanceRequestAdapter(WGPUInstance* instance, WGPURequestAdapterOptions* options, delegate* unmanaged[Cdecl]<WGPURequestAdapterStatus, WGPUAdapter*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.InstanceRequestAdapter(instance, options, (nint)callback, userdata);
    }
    public unsafe void InstanceRequestAdapter(WGPUInstance* instance, in WGPURequestAdapterOptions options, delegate* unmanaged[Cdecl]<WGPURequestAdapterStatus, WGPUAdapter*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.InstanceRequestAdapter(instance, options, (nint)callback, userdata);
    }

    public unsafe void InstanceReference(WGPUInstance* instance)
    {
        WGPUBrowserNative.InstanceReference(instance);
    }
    public unsafe void InstanceRelease(WGPUInstance* instance)
    {
        WGPUBrowserNative.InstanceRelease(instance);
    }
    public unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, byte* label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(pipelineLayout, label);
    }
    public unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, in byte label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(pipelineLayout, label);
    }
    public unsafe void PipelineLayoutSetLabel(WGPUPipelineLayout* pipelineLayout, string label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(pipelineLayout, label);
    }
    public unsafe void PipelineLayoutReference(WGPUPipelineLayout* pipelineLayout)
    {
        WGPUBrowserNative.PipelineLayoutReference(pipelineLayout);
    }
    public unsafe void PipelineLayoutRelease(WGPUPipelineLayout* pipelineLayout)
    {
        WGPUBrowserNative.PipelineLayoutRelease(pipelineLayout);
    }
    public unsafe void QuerySetDestroy(WGPUQuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetDestroy(querySet);
    }
    public unsafe uint QuerySetGetCount(WGPUQuerySet* querySet)
    {
        return WGPUBrowserNative.QuerySetGetCount(querySet);
    }
    public unsafe WGPUQueryType QuerySetGetType(WGPUQuerySet* querySet)
    {
        return WGPUBrowserNative.QuerySetGetType(querySet);
    }
    public unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, byte* label)
    {
        WGPUBrowserNative.QuerySetSetLabel(querySet, label);
    }
    public unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, in byte label)
    {
        WGPUBrowserNative.QuerySetSetLabel(querySet, label);
    }
    public unsafe void QuerySetSetLabel(WGPUQuerySet* querySet, string label)
    {
        WGPUBrowserNative.QuerySetSetLabel(querySet, label);
    }
    public unsafe void QuerySetReference(WGPUQuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetReference(querySet);
    }
    public unsafe void QuerySetRelease(WGPUQuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetRelease(querySet);
    }
    public unsafe void QueueOnSubmittedWorkDone(WGPUQueue* queue, Silk.NET.WebGPU.PfnQueueWorkDoneCallback callback, void* userdata)
    {
        WGPUBrowserNative.QueueOnSubmittedWorkDone(queue, callback, userdata);
    }
    public unsafe void QueueSetLabel(WGPUQueue* queue, byte* label)
    {
        WGPUBrowserNative.QueueSetLabel(queue, label);
    }
    public unsafe void QueueSetLabel(WGPUQueue* queue, in byte label)
    {
        WGPUBrowserNative.QueueSetLabel(queue, label);
    }
    public unsafe void QueueSetLabel(WGPUQueue* queue, string label)
    {
        WGPUBrowserNative.QueueSetLabel(queue, label);
    }
    public unsafe void QueueSubmit(WGPUQueue* queue, nuint commandCount, WGPUCommandBuffer** commands)
    {
        WGPUBrowserNative.QueueSubmit(queue, commandCount, commands);
    }
    public unsafe void QueueSubmit(WGPUQueue* queue, nuint commandCount, ref WGPUCommandBuffer* commands)
    {
        WGPUBrowserNative.QueueSubmit(queue, commandCount, ref commands);
    }
    public unsafe void QueueWriteBuffer(WGPUQueue* queue, WGPUBuffer* buffer, ulong bufferOffset, void* data, nuint size)
    {
        WGPUBrowserNative.QueueWriteBuffer(queue, buffer, bufferOffset, data, size);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, in WGPUExtent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, WGPUExtent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, WGPUImageCopyTexture* destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, in WGPUExtent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, WGPUExtent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, WGPUTextureDataLayout* dataLayout, in WGPUExtent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, WGPUExtent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(WGPUQueue* queue, in WGPUImageCopyTexture destination, void* data, nuint dataSize, in WGPUTextureDataLayout dataLayout, in WGPUExtent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueReference(WGPUQueue* queue)
    {
        WGPUBrowserNative.QueueReference(queue);
    }
    public unsafe void QueueRelease(WGPUQueue* queue)
    {
        WGPUBrowserNative.QueueRelease(queue);
    }
    public unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, byte* label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(renderBundle, label);
    }
    public unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, in byte label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(renderBundle, label);
    }
    public unsafe void RenderBundleSetLabel(WGPURenderBundle* renderBundle, string label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(renderBundle, label);
    }
    public unsafe void RenderBundleReference(WGPURenderBundle* renderBundle)
    {
        WGPUBrowserNative.RenderBundleReference(renderBundle);
    }
    public unsafe void RenderBundleRelease(WGPURenderBundle* renderBundle)
    {
        WGPUBrowserNative.RenderBundleRelease(renderBundle);
    }
    public unsafe void RenderBundleEncoderDraw(WGPURenderBundleEncoder* renderBundleEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderBundleEncoderDraw(renderBundleEncoder, vertexCount, instanceCount, firstVertex, firstInstance);
    }
    public unsafe void RenderBundleEncoderDrawIndexed(WGPURenderBundleEncoder* renderBundleEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndexed(renderBundleEncoder, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }
    public unsafe void RenderBundleEncoderDrawIndexedIndirect(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndexedIndirect(renderBundleEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void RenderBundleEncoderDrawIndirect(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndirect(renderBundleEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe WGPURenderBundle* RenderBundleEncoderFinish(WGPURenderBundleEncoder* renderBundleEncoder, WGPURenderBundleDescriptor* descriptor)
    {
        return WGPUBrowserNative.RenderBundleEncoderFinish(renderBundleEncoder, descriptor);
    }
    public unsafe WGPURenderBundle* RenderBundleEncoderFinish(WGPURenderBundleEncoder* renderBundleEncoder, in WGPURenderBundleDescriptor descriptor)
    {
        return WGPUBrowserNative.RenderBundleEncoderFinish(renderBundleEncoder, descriptor);
    }
    public unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(renderBundleEncoder, markerLabel);
    }
    public unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(renderBundleEncoder, markerLabel);
    }
    public unsafe void RenderBundleEncoderInsertDebugMarker(WGPURenderBundleEncoder* renderBundleEncoder, string markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(renderBundleEncoder, markerLabel);
    }
    public unsafe void RenderBundleEncoderPopDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderPopDebugGroup(renderBundleEncoder);
    }
    public unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(renderBundleEncoder, groupLabel);
    }
    public unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(renderBundleEncoder, groupLabel);
    }
    public unsafe void RenderBundleEncoderPushDebugGroup(WGPURenderBundleEncoder* renderBundleEncoder, string groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(renderBundleEncoder, groupLabel);
    }
    public unsafe void RenderBundleEncoderSetBindGroup(WGPURenderBundleEncoder* renderBundleEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.RenderBundleEncoderSetBindGroup(renderBundleEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderBundleEncoderSetBindGroup(WGPURenderBundleEncoder* renderBundleEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.RenderBundleEncoderSetBindGroup(renderBundleEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderBundleEncoderSetIndexBuffer(WGPURenderBundleEncoder* renderBundleEncoder, WGPUBuffer* buffer, WGPUIndexFormat format, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderBundleEncoderSetIndexBuffer(renderBundleEncoder, buffer, format, offset, size);
    }
    public unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, byte* label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(renderBundleEncoder, label);
    }
    public unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, in byte label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(renderBundleEncoder, label);
    }
    public unsafe void RenderBundleEncoderSetLabel(WGPURenderBundleEncoder* renderBundleEncoder, string label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(renderBundleEncoder, label);
    }
    public unsafe void RenderBundleEncoderSetPipeline(WGPURenderBundleEncoder* renderBundleEncoder, WGPURenderPipeline* pipeline)
    {
        WGPUBrowserNative.RenderBundleEncoderSetPipeline(renderBundleEncoder, pipeline);
    }
    public unsafe void RenderBundleEncoderSetVertexBuffer(WGPURenderBundleEncoder* renderBundleEncoder, uint slot, WGPUBuffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderBundleEncoderSetVertexBuffer(renderBundleEncoder, slot, buffer, offset, size);
    }
    public unsafe void RenderBundleEncoderReference(WGPURenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderReference(renderBundleEncoder);
    }
    public unsafe void RenderBundleEncoderRelease(WGPURenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderRelease(renderBundleEncoder);
    }
    public unsafe void RenderPassEncoderBeginOcclusionQuery(WGPURenderPassEncoder* renderPassEncoder, uint queryIndex)
    {
        WGPUBrowserNative.RenderPassEncoderBeginOcclusionQuery(renderPassEncoder, queryIndex);
    }
    public unsafe void RenderPassEncoderBeginPipelineStatisticsQuery(WGPURenderPassEncoder* renderPassEncoder, WGPUQuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.RenderPassEncoderBeginPipelineStatisticsQuery(renderPassEncoder, querySet, queryIndex);
    }
    public unsafe void RenderPassEncoderDraw(WGPURenderPassEncoder* renderPassEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderPassEncoderDraw(renderPassEncoder, vertexCount, instanceCount, firstVertex, firstInstance);
    }
    public unsafe void RenderPassEncoderDrawIndexed(WGPURenderPassEncoder* renderPassEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndexed(renderPassEncoder, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }
    public unsafe void RenderPassEncoderDrawIndexedIndirect(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndexedIndirect(renderPassEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void RenderPassEncoderDrawIndirect(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndirect(renderPassEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void RenderPassEncoderEnd(WGPURenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEnd(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderEndOcclusionQuery(WGPURenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEndOcclusionQuery(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderEndPipelineStatisticsQuery(WGPURenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEndPipelineStatisticsQuery(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderExecuteBundles(WGPURenderPassEncoder* renderPassEncoder, nuint bundleCount, WGPURenderBundle** bundles)
    {
        WGPUBrowserNative.RenderPassEncoderExecuteBundles(renderPassEncoder, bundleCount, bundles);
    }
    public unsafe void RenderPassEncoderExecuteBundles(WGPURenderPassEncoder* renderPassEncoder, nuint bundleCount, ref WGPURenderBundle* bundles)
    {
        WGPUBrowserNative.RenderPassEncoderExecuteBundles(renderPassEncoder, bundleCount, ref bundles);
    }
    public unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(renderPassEncoder, markerLabel);
    }
    public unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(renderPassEncoder, markerLabel);
    }
    public unsafe void RenderPassEncoderInsertDebugMarker(WGPURenderPassEncoder* renderPassEncoder, string markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(renderPassEncoder, markerLabel);
    }
    public unsafe void RenderPassEncoderPopDebugGroup(WGPURenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderPopDebugGroup(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(renderPassEncoder, groupLabel);
    }
    public unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(renderPassEncoder, groupLabel);
    }
    public unsafe void RenderPassEncoderPushDebugGroup(WGPURenderPassEncoder* renderPassEncoder, string groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(renderPassEncoder, groupLabel);
    }
    public unsafe void RenderPassEncoderSetBindGroup(WGPURenderPassEncoder* renderPassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.RenderPassEncoderSetBindGroup(renderPassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderPassEncoderSetBindGroup(WGPURenderPassEncoder* renderPassEncoder, uint groupIndex, WGPUBindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.RenderPassEncoderSetBindGroup(renderPassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderPassEncoderSetBlendConstant(WGPURenderPassEncoder* renderPassEncoder, WGPUColor* color)
    {
        WGPUBrowserNative.RenderPassEncoderSetBlendConstant(renderPassEncoder, color);
    }
    public unsafe void RenderPassEncoderSetBlendConstant(WGPURenderPassEncoder* renderPassEncoder, in WGPUColor color)
    {
        WGPUBrowserNative.RenderPassEncoderSetBlendConstant(renderPassEncoder, color);
    }
    public unsafe void RenderPassEncoderSetIndexBuffer(WGPURenderPassEncoder* renderPassEncoder, WGPUBuffer* buffer, WGPUIndexFormat format, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderPassEncoderSetIndexBuffer(renderPassEncoder, buffer, format, offset, size);
    }
    public unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, byte* label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(renderPassEncoder, label);
    }
    public unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, in byte label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(renderPassEncoder, label);
    }
    public unsafe void RenderPassEncoderSetLabel(WGPURenderPassEncoder* renderPassEncoder, string label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(renderPassEncoder, label);
    }
    public unsafe void RenderPassEncoderSetPipeline(WGPURenderPassEncoder* renderPassEncoder, WGPURenderPipeline* pipeline)
    {
        WGPUBrowserNative.RenderPassEncoderSetPipeline(renderPassEncoder, pipeline);
    }
    public unsafe void RenderPassEncoderSetScissorRect(WGPURenderPassEncoder* renderPassEncoder, uint x, uint y, uint width, uint height)
    {
        WGPUBrowserNative.RenderPassEncoderSetScissorRect(renderPassEncoder, x, y, width, height);
    }
    public unsafe void RenderPassEncoderSetStencilReference(WGPURenderPassEncoder* renderPassEncoder, uint reference)
    {
        WGPUBrowserNative.RenderPassEncoderSetStencilReference(renderPassEncoder, reference);
    }
    public unsafe void RenderPassEncoderSetVertexBuffer(WGPURenderPassEncoder* renderPassEncoder, uint slot, WGPUBuffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderPassEncoderSetVertexBuffer(renderPassEncoder, slot, buffer, offset, size);
    }
    public unsafe void RenderPassEncoderSetViewport(WGPURenderPassEncoder* renderPassEncoder, float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        WGPUBrowserNative.RenderPassEncoderSetViewport(renderPassEncoder, x, y, width, height, minDepth, maxDepth);
    }
    public unsafe void RenderPassEncoderReference(WGPURenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderReference(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderRelease(WGPURenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderRelease(renderPassEncoder);
    }
    public unsafe WGPUBindGroupLayout* RenderPipelineGetBindGroupLayout(WGPURenderPipeline* renderPipeline, uint groupIndex)
    {
        return WGPUBrowserNative.RenderPipelineGetBindGroupLayout(renderPipeline, groupIndex);
    }
    public unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, byte* label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(renderPipeline, label);
    }
    public unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, in byte label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(renderPipeline, label);
    }
    public unsafe void RenderPipelineSetLabel(WGPURenderPipeline* renderPipeline, string label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(renderPipeline, label);
    }
    public unsafe void RenderPipelineReference(WGPURenderPipeline* renderPipeline)
    {
        WGPUBrowserNative.RenderPipelineReference(renderPipeline);
    }
    public unsafe void RenderPipelineRelease(WGPURenderPipeline* renderPipeline)
    {
        WGPUBrowserNative.RenderPipelineRelease(renderPipeline);
    }
    public unsafe void SamplerSetLabel(WGPUSampler* sampler, byte* label)
    {
        WGPUBrowserNative.SamplerSetLabel(sampler, label);
    }
    public unsafe void SamplerSetLabel(WGPUSampler* sampler, in byte label)
    {
        WGPUBrowserNative.SamplerSetLabel(sampler, label);
    }
    public unsafe void SamplerSetLabel(WGPUSampler* sampler, string label)
    {
        WGPUBrowserNative.SamplerSetLabel(sampler, label);
    }
    public unsafe void SamplerReference(WGPUSampler* sampler)
    {
        WGPUBrowserNative.SamplerReference(sampler);
    }
    public unsafe void SamplerRelease(WGPUSampler* sampler)
    {
        WGPUBrowserNative.SamplerRelease(sampler);
    }
    public unsafe void ShaderModuleGetCompilationInfo(WGPUShaderModule* shaderModule, Silk.NET.WebGPU.PfnCompilationInfoCallback callback, void* userdata)
    {
        WGPUBrowserNative.ShaderModuleGetCompilationInfo(shaderModule, callback, userdata);
    }
    public unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, byte* label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(shaderModule, label);
    }
    public unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, in byte label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(shaderModule, label);
    }
    public unsafe void ShaderModuleSetLabel(WGPUShaderModule* shaderModule, string label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(shaderModule, label);
    }
    public unsafe void ShaderModuleReference(WGPUShaderModule* shaderModule)
    {
        WGPUBrowserNative.ShaderModuleReference(shaderModule);
    }
    public unsafe void ShaderModuleRelease(WGPUShaderModule* shaderModule)
    {
        WGPUBrowserNative.ShaderModuleRelease(shaderModule);
    }
    public unsafe WGPUTextureFormat SurfaceGetPreferredFormat(WGPUSurface* surface, WGPUAdapter* adapter)
    {
        return WGPUBrowserNative.SurfaceGetPreferredFormat(surface, adapter);
    }
    public unsafe void SurfacePresent(WGPUSurface* surface)
    {
        WGPUBrowserNative.SurfacePresent(surface);
    }
    public unsafe void SurfaceUnconfigure(WGPUSurface* surface)
    {
        WGPUBrowserNative.SurfaceUnconfigure(surface);
    }
    public unsafe void SurfaceReference(WGPUSurface* surface)
    {
        WGPUBrowserNative.SurfaceReference(surface);
    }
    public unsafe void SurfaceRelease(WGPUSurface* surface)
    {
        WGPUBrowserNative.SurfaceRelease(surface);
    }
    public unsafe WGPUTextureView* TextureCreateView(WGPUTexture* texture, WGPUTextureViewDescriptor* descriptor)
    {
        return WGPUBrowserNative.TextureCreateView(texture, descriptor);
    }
    public unsafe WGPUTextureView* TextureCreateView(WGPUTexture* texture, in WGPUTextureViewDescriptor descriptor)
    {
        return WGPUBrowserNative.TextureCreateView(texture, descriptor);
    }
    public unsafe void TextureDestroy(WGPUTexture* texture)
    {
        WGPUBrowserNative.TextureDestroy(texture);
    }
    public unsafe uint TextureGetDepthOrArrayLayers(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetDepthOrArrayLayers(texture);
    }
    public unsafe WGPUTextureDimension TextureGetDimension(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetDimension(texture);
    }
    public unsafe WGPUTextureFormat TextureGetFormat(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetFormat(texture);
    }
    public unsafe uint TextureGetHeight(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetHeight(texture);
    }
    public unsafe uint TextureGetMipLevelCount(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetMipLevelCount(texture);
    }
    public unsafe uint TextureGetSampleCount(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetSampleCount(texture);
    }
    public unsafe WGPUTextureUsage TextureGetUsage(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetUsage(texture);
    }
    public unsafe uint TextureGetWidth(WGPUTexture* texture)
    {
        return WGPUBrowserNative.TextureGetWidth(texture);
    }
    public unsafe void TextureSetLabel(WGPUTexture* texture, byte* label)
    {
        WGPUBrowserNative.TextureSetLabel(texture, label);
    }
    public unsafe void TextureSetLabel(WGPUTexture* texture, in byte label)
    {
        WGPUBrowserNative.TextureSetLabel(texture, label);
    }
    public unsafe void TextureSetLabel(WGPUTexture* texture, string label)
    {
        WGPUBrowserNative.TextureSetLabel(texture, label);
    }
    public unsafe void TextureReference(WGPUTexture* texture)
    {
        WGPUBrowserNative.TextureReference(texture);
    }
    public unsafe void TextureRelease(WGPUTexture* texture)
    {
        WGPUBrowserNative.TextureRelease(texture);
    }
    public unsafe void TextureViewSetLabel(WGPUTextureView* textureView, byte* label)
    {
        WGPUBrowserNative.TextureViewSetLabel(textureView, label);
    }
    public unsafe void TextureViewSetLabel(WGPUTextureView* textureView, in byte label)
    {
        WGPUBrowserNative.TextureViewSetLabel(textureView, label);
    }
    public unsafe void TextureViewSetLabel(WGPUTextureView* textureView, string label)
    {
        WGPUBrowserNative.TextureViewSetLabel(textureView, label);
    }
    public unsafe void TextureViewReference(WGPUTextureView* textureView)
    {
        WGPUBrowserNative.TextureViewReference(textureView);
    }
    public unsafe void TextureViewRelease(WGPUTextureView* textureView)
    {
        WGPUBrowserNative.TextureViewRelease(textureView);
    }

    public unsafe WGPU.WGPUSwapChain* DeviceCreateSwapChain(WGPUDevice* device, WGPUSurface* surface, WGPU.WGPUSwapChainDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateSwapChain(device, surface, in descriptor);
    }
    public unsafe WGPUTextureView* SwapChainGetCurrentTextureView(WGPU.WGPUSwapChain* swapChain)
    {
        return WGPUBrowserNative.SwapChainGetCurrentTextureView(swapChain);
    }
}