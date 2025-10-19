namespace Pollus.Emscripten;

using Silk.NET.WebGPU;

public class WGPUBrowser : IDisposable
{
    public void Dispose() { }

    public unsafe Instance* CreateInstance(WGPU.WGPUInstanceDescriptor* descriptor)
    {
        return WGPUBrowserNative.CreateInstance(descriptor);
    }
    public unsafe Instance* CreateInstance(in WGPU.WGPUInstanceDescriptor descriptor)
    {
        return WGPUBrowserNative.CreateInstance(descriptor);
    }
    public unsafe PfnProc GetProcAddress(Device* device, byte* procName)
    {
        return WGPUBrowserNative.GetProcAddress(device, procName);
    }
    public unsafe PfnProc GetProcAddress(Device* device, in byte procName)
    {
        return WGPUBrowserNative.GetProcAddress(device, procName);
    }
    public unsafe PfnProc GetProcAddress(Device* device, string procName)
    {
        return WGPUBrowserNative.GetProcAddress(device, procName);
    }
    public unsafe nuint AdapterEnumerateFeatures(Adapter* adapter, FeatureName* features)
    {
        return WGPUBrowserNative.AdapterEnumerateFeatures(adapter, features);
    }
    public unsafe nuint AdapterEnumerateFeatures(Adapter* adapter, ref FeatureName features)
    {
        return WGPUBrowserNative.AdapterEnumerateFeatures(adapter, ref features);
    }
    public unsafe bool AdapterGetLimits(Adapter* adapter, SupportedLimits* limits)
    {
        return WGPUBrowserNative.AdapterGetLimits(adapter, limits);
    }
    public unsafe bool AdapterGetLimits(Adapter* adapter, ref SupportedLimits limits)
    {
        return WGPUBrowserNative.AdapterGetLimits(adapter, ref limits);
    }
    public unsafe void AdapterGetProperties(Adapter* adapter, AdapterProperties* properties)
    {
        WGPUBrowserNative.AdapterGetProperties(adapter, properties);
    }
    public unsafe void AdapterGetProperties(Adapter* adapter, ref AdapterProperties properties)
    {
        WGPUBrowserNative.AdapterGetProperties(adapter, ref properties);
    }
    public unsafe bool AdapterHasFeature(Adapter* adapter, FeatureName feature)
    {
        return WGPUBrowserNative.AdapterHasFeature(adapter, feature);
    }

    public unsafe void AdapterRequestDevice(Adapter* adapter, WGPU.WGPUDeviceDescriptor* descriptor, delegate* unmanaged[Cdecl]<RequestDeviceStatus, Device*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.AdapterRequestDevice(adapter, descriptor, (nint)callback, userdata);
    }
    public unsafe void AdapterRequestDevice(Adapter* adapter, in WGPU.WGPUDeviceDescriptor descriptor, delegate* unmanaged[Cdecl]<RequestDeviceStatus, Device*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.AdapterRequestDevice(adapter, descriptor, (nint)callback, userdata);
    }

    public unsafe void AdapterReference(Adapter* adapter)
    {
        WGPUBrowserNative.AdapterReference(adapter);
    }
    public unsafe void AdapterRelease(Adapter* adapter)
    {
        WGPUBrowserNative.AdapterRelease(adapter);
    }
    public unsafe void BindGroupSetLabel(BindGroup* bindGroup, byte* label)
    {
        WGPUBrowserNative.BindGroupSetLabel(bindGroup, label);
    }
    public unsafe void BindGroupSetLabel(BindGroup* bindGroup, in byte label)
    {
        WGPUBrowserNative.BindGroupSetLabel(bindGroup, label);
    }
    public unsafe void BindGroupSetLabel(BindGroup* bindGroup, string label)
    {
        WGPUBrowserNative.BindGroupSetLabel(bindGroup, label);
    }
    public unsafe void BindGroupReference(BindGroup* bindGroup)
    {
        WGPUBrowserNative.BindGroupReference(bindGroup);
    }
    public unsafe void BindGroupRelease(BindGroup* bindGroup)
    {
        WGPUBrowserNative.BindGroupRelease(bindGroup);
    }
    public unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, byte* label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(bindGroupLayout, label);
    }
    public unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, in byte label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(bindGroupLayout, label);
    }
    public unsafe void BindGroupLayoutSetLabel(BindGroupLayout* bindGroupLayout, string label)
    {
        WGPUBrowserNative.BindGroupLayoutSetLabel(bindGroupLayout, label);
    }
    public unsafe void BindGroupLayoutReference(BindGroupLayout* bindGroupLayout)
    {
        WGPUBrowserNative.BindGroupLayoutReference(bindGroupLayout);
    }
    public unsafe void BindGroupLayoutRelease(BindGroupLayout* bindGroupLayout)
    {
        WGPUBrowserNative.BindGroupLayoutRelease(bindGroupLayout);
    }
    public unsafe void BufferDestroy(Buffer* buffer)
    {
        WGPUBrowserNative.BufferDestroy(buffer);
    }
    public unsafe void* BufferGetConstMappedRange(Buffer* buffer, nuint offset, nuint size)
    {
        return WGPUBrowserNative.BufferGetConstMappedRange(buffer, offset, size);
    }
    public unsafe BufferMapState BufferGetMapState(Buffer* buffer)
    {
        return WGPUBrowserNative.BufferGetMapState(buffer);
    }
    public unsafe void* BufferGetMappedRange(Buffer* buffer, nuint offset, nuint size)
    {
        return WGPUBrowserNative.BufferGetMappedRange(buffer, offset, size);
    }
    public unsafe ulong BufferGetSize(Buffer* buffer)
    {
        return WGPUBrowserNative.BufferGetSize(buffer);
    }
    public unsafe BufferUsage BufferGetUsage(Buffer* buffer)
    {
        return WGPUBrowserNative.BufferGetUsage(buffer);
    }
    public unsafe void BufferMapAsync(Buffer* buffer, MapMode mode, nuint offset, nuint size, PfnBufferMapCallback callback, void* userdata)
    {
        WGPUBrowserNative.BufferMapAsync(buffer, mode, offset, size, callback, userdata);
    }
    public unsafe void BufferSetLabel(Buffer* buffer, byte* label)
    {
        WGPUBrowserNative.BufferSetLabel(buffer, label);
    }
    public unsafe void BufferSetLabel(Buffer* buffer, in byte label)
    {
        WGPUBrowserNative.BufferSetLabel(buffer, label);
    }
    public unsafe void BufferSetLabel(Buffer* buffer, string label)
    {
        WGPUBrowserNative.BufferSetLabel(buffer, label);
    }
    public unsafe void BufferUnmap(Buffer* buffer)
    {
        WGPUBrowserNative.BufferUnmap(buffer);
    }
    public unsafe void BufferReference(Buffer* buffer)
    {
        WGPUBrowserNative.BufferReference(buffer);
    }
    public unsafe void BufferRelease(Buffer* buffer)
    {
        WGPUBrowserNative.BufferRelease(buffer);
    }
    public unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, byte* label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(commandBuffer, label);
    }
    public unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, in byte label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(commandBuffer, label);
    }
    public unsafe void CommandBufferSetLabel(CommandBuffer* commandBuffer, string label)
    {
        WGPUBrowserNative.CommandBufferSetLabel(commandBuffer, label);
    }
    public unsafe void CommandBufferReference(CommandBuffer* commandBuffer)
    {
        WGPUBrowserNative.CommandBufferReference(commandBuffer);
    }
    public unsafe void CommandBufferRelease(CommandBuffer* commandBuffer)
    {
        WGPUBrowserNative.CommandBufferRelease(commandBuffer);
    }
    public unsafe ComputePassEncoder* CommandEncoderBeginComputePass(CommandEncoder* commandEncoder, ComputePassDescriptor* descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginComputePass(commandEncoder, descriptor);
    }
    public unsafe ComputePassEncoder* CommandEncoderBeginComputePass(CommandEncoder* commandEncoder, in ComputePassDescriptor descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginComputePass(commandEncoder, descriptor);
    }
    public unsafe RenderPassEncoder* CommandEncoderBeginRenderPass(CommandEncoder* commandEncoder, WGPU.WGPURenderPassDescriptor* descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }
    public unsafe RenderPassEncoder* CommandEncoderBeginRenderPass(CommandEncoder* commandEncoder, in WGPU.WGPURenderPassDescriptor descriptor)
    {
        return WGPUBrowserNative.CommandEncoderBeginRenderPass(commandEncoder, descriptor);
    }
    public unsafe void CommandEncoderClearBuffer(CommandEncoder* commandEncoder, Buffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.CommandEncoderClearBuffer(commandEncoder, buffer, offset, size);
    }
    public unsafe void CommandEncoderCopyBufferToBuffer(CommandEncoder* commandEncoder, Buffer* source, ulong sourceOffset, Buffer* destination, ulong destinationOffset, ulong size)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToBuffer(commandEncoder, source, sourceOffset, destination, destinationOffset, size);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, ImageCopyTexture* destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, ImageCopyTexture* destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, in ImageCopyTexture destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, ImageCopyBuffer* source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, ImageCopyTexture* destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, ImageCopyTexture* destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, in ImageCopyTexture destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyBufferToTexture(CommandEncoder* commandEncoder, in ImageCopyBuffer source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyBufferToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyBuffer* destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyBuffer* destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyBuffer destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyBuffer destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyBuffer* destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyBuffer* destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyBuffer destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToBuffer(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyBuffer destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToBuffer(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyTexture* destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, ImageCopyTexture* destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyTexture destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, ImageCopyTexture* source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyTexture* destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, ImageCopyTexture* destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyTexture destination, Extent3D* copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe void CommandEncoderCopyTextureToTexture(CommandEncoder* commandEncoder, in ImageCopyTexture source, in ImageCopyTexture destination, in Extent3D copySize)
    {
        WGPUBrowserNative.CommandEncoderCopyTextureToTexture(commandEncoder, source, destination, copySize);
    }
    public unsafe CommandBuffer* CommandEncoderFinish(CommandEncoder* commandEncoder, CommandBufferDescriptor* descriptor)
    {
        return WGPUBrowserNative.CommandEncoderFinish(commandEncoder, descriptor);
    }
    public unsafe CommandBuffer* CommandEncoderFinish(CommandEncoder* commandEncoder, in CommandBufferDescriptor descriptor)
    {
        return WGPUBrowserNative.CommandEncoderFinish(commandEncoder, descriptor);
    }
    public unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(commandEncoder, markerLabel);
    }
    public unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(commandEncoder, markerLabel);
    }
    public unsafe void CommandEncoderInsertDebugMarker(CommandEncoder* commandEncoder, string markerLabel)
    {
        WGPUBrowserNative.CommandEncoderInsertDebugMarker(commandEncoder, markerLabel);
    }
    public unsafe void CommandEncoderPopDebugGroup(CommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderPopDebugGroup(commandEncoder);
    }
    public unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(commandEncoder, groupLabel);
    }
    public unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(commandEncoder, groupLabel);
    }
    public unsafe void CommandEncoderPushDebugGroup(CommandEncoder* commandEncoder, string groupLabel)
    {
        WGPUBrowserNative.CommandEncoderPushDebugGroup(commandEncoder, groupLabel);
    }
    public unsafe void CommandEncoderResolveQuerySet(CommandEncoder* commandEncoder, QuerySet* querySet, uint firstQuery, uint queryCount, Buffer* destination, ulong destinationOffset)
    {
        WGPUBrowserNative.CommandEncoderResolveQuerySet(commandEncoder, querySet, firstQuery, queryCount, destination, destinationOffset);
    }
    public unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, byte* label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(commandEncoder, label);
    }
    public unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, in byte label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(commandEncoder, label);
    }
    public unsafe void CommandEncoderSetLabel(CommandEncoder* commandEncoder, string label)
    {
        WGPUBrowserNative.CommandEncoderSetLabel(commandEncoder, label);
    }
    public unsafe void CommandEncoderWriteTimestamp(CommandEncoder* commandEncoder, QuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.CommandEncoderWriteTimestamp(commandEncoder, querySet, queryIndex);
    }
    public unsafe void CommandEncoderReference(CommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderReference(commandEncoder);
    }
    public unsafe void CommandEncoderRelease(CommandEncoder* commandEncoder)
    {
        WGPUBrowserNative.CommandEncoderRelease(commandEncoder);
    }
    public unsafe void ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder* computePassEncoder, QuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.ComputePassEncoderBeginPipelineStatisticsQuery(computePassEncoder, querySet, queryIndex);
    }
    public unsafe void ComputePassEncoderDispatchWorkgroups(ComputePassEncoder* computePassEncoder, uint workgroupCountX, uint workgroupCountY, uint workgroupCountZ)
    {
        WGPUBrowserNative.ComputePassEncoderDispatchWorkgroups(computePassEncoder, workgroupCountX, workgroupCountY, workgroupCountZ);
    }
    public unsafe void ComputePassEncoderDispatchWorkgroupsIndirect(ComputePassEncoder* computePassEncoder, Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.ComputePassEncoderDispatchWorkgroupsIndirect(computePassEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void ComputePassEncoderEnd(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderEnd(computePassEncoder);
    }
    public unsafe void ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderEndPipelineStatisticsQuery(computePassEncoder);
    }
    public unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(computePassEncoder, markerLabel);
    }
    public unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(computePassEncoder, markerLabel);
    }
    public unsafe void ComputePassEncoderInsertDebugMarker(ComputePassEncoder* computePassEncoder, string markerLabel)
    {
        WGPUBrowserNative.ComputePassEncoderInsertDebugMarker(computePassEncoder, markerLabel);
    }
    public unsafe void ComputePassEncoderPopDebugGroup(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderPopDebugGroup(computePassEncoder);
    }
    public unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(computePassEncoder, groupLabel);
    }
    public unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(computePassEncoder, groupLabel);
    }
    public unsafe void ComputePassEncoderPushDebugGroup(ComputePassEncoder* computePassEncoder, string groupLabel)
    {
        WGPUBrowserNative.ComputePassEncoderPushDebugGroup(computePassEncoder, groupLabel);
    }
    public unsafe void ComputePassEncoderSetBindGroup(ComputePassEncoder* computePassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.ComputePassEncoderSetBindGroup(computePassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void ComputePassEncoderSetBindGroup(ComputePassEncoder* computePassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.ComputePassEncoderSetBindGroup(computePassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, byte* label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(computePassEncoder, label);
    }
    public unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, in byte label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(computePassEncoder, label);
    }
    public unsafe void ComputePassEncoderSetLabel(ComputePassEncoder* computePassEncoder, string label)
    {
        WGPUBrowserNative.ComputePassEncoderSetLabel(computePassEncoder, label);
    }
    public unsafe void ComputePassEncoderSetPipeline(ComputePassEncoder* computePassEncoder, ComputePipeline* pipeline)
    {
        WGPUBrowserNative.ComputePassEncoderSetPipeline(computePassEncoder, pipeline);
    }
    public unsafe void ComputePassEncoderReference(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderReference(computePassEncoder);
    }
    public unsafe void ComputePassEncoderRelease(ComputePassEncoder* computePassEncoder)
    {
        WGPUBrowserNative.ComputePassEncoderRelease(computePassEncoder);
    }
    public unsafe BindGroupLayout* ComputePipelineGetBindGroupLayout(ComputePipeline* computePipeline, uint groupIndex)
    {
        return WGPUBrowserNative.ComputePipelineGetBindGroupLayout(computePipeline, groupIndex);
    }
    public unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, byte* label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(computePipeline, label);
    }
    public unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, in byte label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(computePipeline, label);
    }
    public unsafe void ComputePipelineSetLabel(ComputePipeline* computePipeline, string label)
    {
        WGPUBrowserNative.ComputePipelineSetLabel(computePipeline, label);
    }
    public unsafe void ComputePipelineReference(ComputePipeline* computePipeline)
    {
        WGPUBrowserNative.ComputePipelineReference(computePipeline);
    }
    public unsafe void ComputePipelineRelease(ComputePipeline* computePipeline)
    {
        WGPUBrowserNative.ComputePipelineRelease(computePipeline);
    }
    public unsafe BindGroup* DeviceCreateBindGroup(Device* device, BindGroupDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroup(device, descriptor);
    }
    public unsafe BindGroup* DeviceCreateBindGroup(Device* device, in BindGroupDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroup(device, descriptor);
    }
    public unsafe BindGroupLayout* DeviceCreateBindGroupLayout(Device* device, BindGroupLayoutDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroupLayout(device, descriptor);
    }
    public unsafe BindGroupLayout* DeviceCreateBindGroupLayout(Device* device, in BindGroupLayoutDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBindGroupLayout(device, descriptor);
    }
    public unsafe Buffer* DeviceCreateBuffer(Device* device, BufferDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBuffer(device, descriptor);
    }
    public unsafe Buffer* DeviceCreateBuffer(Device* device, in BufferDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateBuffer(device, descriptor);
    }
    public unsafe CommandEncoder* DeviceCreateCommandEncoder(Device* device, CommandEncoderDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateCommandEncoder(device, descriptor);
    }
    public unsafe CommandEncoder* DeviceCreateCommandEncoder(Device* device, in CommandEncoderDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateCommandEncoder(device, descriptor);
    }
    public unsafe ComputePipeline* DeviceCreateComputePipeline(Device* device, ComputePipelineDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateComputePipeline(device, descriptor);
    }
    public unsafe ComputePipeline* DeviceCreateComputePipeline(Device* device, in ComputePipelineDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateComputePipeline(device, descriptor);
    }
    public unsafe void DeviceCreateComputePipelineAsync(Device* device, ComputePipelineDescriptor* descriptor, PfnCreateComputePipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateComputePipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe void DeviceCreateComputePipelineAsync(Device* device, in ComputePipelineDescriptor descriptor, PfnCreateComputePipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateComputePipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe PipelineLayout* DeviceCreatePipelineLayout(Device* device, PipelineLayoutDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreatePipelineLayout(device, descriptor);
    }
    public unsafe PipelineLayout* DeviceCreatePipelineLayout(Device* device, in PipelineLayoutDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreatePipelineLayout(device, descriptor);
    }
    public unsafe QuerySet* DeviceCreateQuerySet(Device* device, QuerySetDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateQuerySet(device, descriptor);
    }
    public unsafe QuerySet* DeviceCreateQuerySet(Device* device, in QuerySetDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateQuerySet(device, descriptor);
    }
    public unsafe RenderBundleEncoder* DeviceCreateRenderBundleEncoder(Device* device, RenderBundleEncoderDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderBundleEncoder(device, descriptor);
    }
    public unsafe RenderBundleEncoder* DeviceCreateRenderBundleEncoder(Device* device, in RenderBundleEncoderDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderBundleEncoder(device, descriptor);
    }
    public unsafe RenderPipeline* DeviceCreateRenderPipeline(Device* device, RenderPipelineDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderPipeline(device, descriptor);
    }
    public unsafe RenderPipeline* DeviceCreateRenderPipeline(Device* device, in RenderPipelineDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateRenderPipeline(device, descriptor);
    }
    public unsafe void DeviceCreateRenderPipelineAsync(Device* device, RenderPipelineDescriptor* descriptor, PfnCreateRenderPipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateRenderPipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe void DeviceCreateRenderPipelineAsync(Device* device, in RenderPipelineDescriptor descriptor, PfnCreateRenderPipelineAsyncCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceCreateRenderPipelineAsync(device, descriptor, callback, userdata);
    }
    public unsafe Sampler* DeviceCreateSampler(Device* device, SamplerDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateSampler(device, descriptor);
    }
    public unsafe Sampler* DeviceCreateSampler(Device* device, in SamplerDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateSampler(device, descriptor);
    }
    public unsafe ShaderModule* DeviceCreateShaderModule(Device* device, ShaderModuleDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateShaderModule(device, descriptor);
    }
    public unsafe ShaderModule* DeviceCreateShaderModule(Device* device, in ShaderModuleDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateShaderModule(device, descriptor);
    }
    public unsafe Texture* DeviceCreateTexture(Device* device, TextureDescriptor* descriptor)
    {
        return WGPUBrowserNative.DeviceCreateTexture(device, descriptor);
    }
    public unsafe Texture* DeviceCreateTexture(Device* device, in TextureDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateTexture(device, descriptor);
    }
    public unsafe void DeviceDestroy(Device* device)
    {
        WGPUBrowserNative.DeviceDestroy(device);
    }
    public unsafe nuint DeviceEnumerateFeatures(Device* device, FeatureName* features)
    {
        return WGPUBrowserNative.DeviceEnumerateFeatures(device, features);
    }
    public unsafe nuint DeviceEnumerateFeatures(Device* device, ref FeatureName features)
    {
        return WGPUBrowserNative.DeviceEnumerateFeatures(device, ref features);
    }
    public unsafe bool DeviceGetLimits(Device* device, SupportedLimits* limits)
    {
        return WGPUBrowserNative.DeviceGetLimits(device, limits);
    }
    public unsafe bool DeviceGetLimits(Device* device, ref SupportedLimits limits)
    {
        return WGPUBrowserNative.DeviceGetLimits(device, ref limits);
    }
    public unsafe Queue* DeviceGetQueue(Device* device)
    {
        return WGPUBrowserNative.DeviceGetQueue(device);
    }
    public unsafe bool DeviceHasFeature(Device* device, FeatureName feature)
    {
        return WGPUBrowserNative.DeviceHasFeature(device, feature);
    }
    public unsafe void DevicePopErrorScope(Device* device, PfnErrorCallback callback, void* userdata)
    {
        WGPUBrowserNative.DevicePopErrorScope(device, callback, userdata);
    }
    public unsafe void DevicePushErrorScope(Device* device, ErrorFilter filter)
    {
        WGPUBrowserNative.DevicePushErrorScope(device, filter);
    }
    public unsafe void DeviceSetLabel(Device* device, byte* label)
    {
        WGPUBrowserNative.DeviceSetLabel(device, label);
    }
    public unsafe void DeviceSetLabel(Device* device, in byte label)
    {
        WGPUBrowserNative.DeviceSetLabel(device, label);
    }
    public unsafe void DeviceSetLabel(Device* device, string label)
    {
        WGPUBrowserNative.DeviceSetLabel(device, label);
    }
    public unsafe void DeviceSetUncapturedErrorCallback(Device* device, PfnErrorCallback callback, void* userdata)
    {
        WGPUBrowserNative.DeviceSetUncapturedErrorCallback(device, callback, userdata);
    }
    public unsafe void DeviceReference(Device* device)
    {
        WGPUBrowserNative.DeviceReference(device);
    }
    public unsafe void DeviceRelease(Device* device)
    {
        WGPUBrowserNative.DeviceRelease(device);
    }
    public unsafe Surface* InstanceCreateSurface(Instance* instance, SurfaceDescriptor* descriptor)
    {
        return WGPUBrowserNative.InstanceCreateSurface(instance, descriptor);
    }
    public unsafe Surface* InstanceCreateSurface(Instance* instance, in SurfaceDescriptor descriptor)
    {
        return WGPUBrowserNative.InstanceCreateSurface(instance, descriptor);
    }
    public unsafe void InstanceProcessEvents(Instance* instance)
    {
        WGPUBrowserNative.InstanceProcessEvents(instance);
    }

    public unsafe void InstanceRequestAdapter(Instance* instance, RequestAdapterOptions* options, delegate* unmanaged[Cdecl]<RequestAdapterStatus, Adapter*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.InstanceRequestAdapter(instance, options, (nint)callback, userdata);
    }
    public unsafe void InstanceRequestAdapter(Instance* instance, in RequestAdapterOptions options, delegate* unmanaged[Cdecl]<RequestAdapterStatus, Adapter*, byte*, void*, void> callback, void* userdata)
    {
        WGPUBrowserNative.InstanceRequestAdapter(instance, options, (nint)callback, userdata);
    }

    public unsafe void InstanceReference(Instance* instance)
    {
        WGPUBrowserNative.InstanceReference(instance);
    }
    public unsafe void InstanceRelease(Instance* instance)
    {
        WGPUBrowserNative.InstanceRelease(instance);
    }
    public unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, byte* label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(pipelineLayout, label);
    }
    public unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, in byte label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(pipelineLayout, label);
    }
    public unsafe void PipelineLayoutSetLabel(PipelineLayout* pipelineLayout, string label)
    {
        WGPUBrowserNative.PipelineLayoutSetLabel(pipelineLayout, label);
    }
    public unsafe void PipelineLayoutReference(PipelineLayout* pipelineLayout)
    {
        WGPUBrowserNative.PipelineLayoutReference(pipelineLayout);
    }
    public unsafe void PipelineLayoutRelease(PipelineLayout* pipelineLayout)
    {
        WGPUBrowserNative.PipelineLayoutRelease(pipelineLayout);
    }
    public unsafe void QuerySetDestroy(QuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetDestroy(querySet);
    }
    public unsafe uint QuerySetGetCount(QuerySet* querySet)
    {
        return WGPUBrowserNative.QuerySetGetCount(querySet);
    }
    public unsafe QueryType QuerySetGetType(QuerySet* querySet)
    {
        return WGPUBrowserNative.QuerySetGetType(querySet);
    }
    public unsafe void QuerySetSetLabel(QuerySet* querySet, byte* label)
    {
        WGPUBrowserNative.QuerySetSetLabel(querySet, label);
    }
    public unsafe void QuerySetSetLabel(QuerySet* querySet, in byte label)
    {
        WGPUBrowserNative.QuerySetSetLabel(querySet, label);
    }
    public unsafe void QuerySetSetLabel(QuerySet* querySet, string label)
    {
        WGPUBrowserNative.QuerySetSetLabel(querySet, label);
    }
    public unsafe void QuerySetReference(QuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetReference(querySet);
    }
    public unsafe void QuerySetRelease(QuerySet* querySet)
    {
        WGPUBrowserNative.QuerySetRelease(querySet);
    }
    public unsafe void QueueOnSubmittedWorkDone(Queue* queue, PfnQueueWorkDoneCallback callback, void* userdata)
    {
        WGPUBrowserNative.QueueOnSubmittedWorkDone(queue, callback, userdata);
    }
    public unsafe void QueueSetLabel(Queue* queue, byte* label)
    {
        WGPUBrowserNative.QueueSetLabel(queue, label);
    }
    public unsafe void QueueSetLabel(Queue* queue, in byte label)
    {
        WGPUBrowserNative.QueueSetLabel(queue, label);
    }
    public unsafe void QueueSetLabel(Queue* queue, string label)
    {
        WGPUBrowserNative.QueueSetLabel(queue, label);
    }
    public unsafe void QueueSubmit(Queue* queue, nuint commandCount, CommandBuffer** commands)
    {
        WGPUBrowserNative.QueueSubmit(queue, commandCount, commands);
    }
    public unsafe void QueueSubmit(Queue* queue, nuint commandCount, ref CommandBuffer* commands)
    {
        WGPUBrowserNative.QueueSubmit(queue, commandCount, ref commands);
    }
    public unsafe void QueueWriteBuffer(Queue* queue, Buffer* buffer, ulong bufferOffset, void* data, nuint size)
    {
        WGPUBrowserNative.QueueWriteBuffer(queue, buffer, bufferOffset, data, size);
    }
    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, Extent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, in Extent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, Extent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, ImageCopyTexture* destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, in Extent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, Extent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, TextureDataLayout* dataLayout, in Extent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, Extent3D* writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueWriteTexture(Queue* queue, in ImageCopyTexture destination, void* data, nuint dataSize, in TextureDataLayout dataLayout, in Extent3D writeSize)
    {
        WGPUBrowserNative.QueueWriteTexture(queue, destination, data, dataSize, dataLayout, writeSize);
    }
    public unsafe void QueueReference(Queue* queue)
    {
        WGPUBrowserNative.QueueReference(queue);
    }
    public unsafe void QueueRelease(Queue* queue)
    {
        WGPUBrowserNative.QueueRelease(queue);
    }
    public unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, byte* label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(renderBundle, label);
    }
    public unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, in byte label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(renderBundle, label);
    }
    public unsafe void RenderBundleSetLabel(RenderBundle* renderBundle, string label)
    {
        WGPUBrowserNative.RenderBundleSetLabel(renderBundle, label);
    }
    public unsafe void RenderBundleReference(RenderBundle* renderBundle)
    {
        WGPUBrowserNative.RenderBundleReference(renderBundle);
    }
    public unsafe void RenderBundleRelease(RenderBundle* renderBundle)
    {
        WGPUBrowserNative.RenderBundleRelease(renderBundle);
    }
    public unsafe void RenderBundleEncoderDraw(RenderBundleEncoder* renderBundleEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderBundleEncoderDraw(renderBundleEncoder, vertexCount, instanceCount, firstVertex, firstInstance);
    }
    public unsafe void RenderBundleEncoderDrawIndexed(RenderBundleEncoder* renderBundleEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndexed(renderBundleEncoder, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }
    public unsafe void RenderBundleEncoderDrawIndexedIndirect(RenderBundleEncoder* renderBundleEncoder, Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndexedIndirect(renderBundleEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void RenderBundleEncoderDrawIndirect(RenderBundleEncoder* renderBundleEncoder, Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderBundleEncoderDrawIndirect(renderBundleEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe RenderBundle* RenderBundleEncoderFinish(RenderBundleEncoder* renderBundleEncoder, RenderBundleDescriptor* descriptor)
    {
        return WGPUBrowserNative.RenderBundleEncoderFinish(renderBundleEncoder, descriptor);
    }
    public unsafe RenderBundle* RenderBundleEncoderFinish(RenderBundleEncoder* renderBundleEncoder, in RenderBundleDescriptor descriptor)
    {
        return WGPUBrowserNative.RenderBundleEncoderFinish(renderBundleEncoder, descriptor);
    }
    public unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(renderBundleEncoder, markerLabel);
    }
    public unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(renderBundleEncoder, markerLabel);
    }
    public unsafe void RenderBundleEncoderInsertDebugMarker(RenderBundleEncoder* renderBundleEncoder, string markerLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderInsertDebugMarker(renderBundleEncoder, markerLabel);
    }
    public unsafe void RenderBundleEncoderPopDebugGroup(RenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderPopDebugGroup(renderBundleEncoder);
    }
    public unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(renderBundleEncoder, groupLabel);
    }
    public unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(renderBundleEncoder, groupLabel);
    }
    public unsafe void RenderBundleEncoderPushDebugGroup(RenderBundleEncoder* renderBundleEncoder, string groupLabel)
    {
        WGPUBrowserNative.RenderBundleEncoderPushDebugGroup(renderBundleEncoder, groupLabel);
    }
    public unsafe void RenderBundleEncoderSetBindGroup(RenderBundleEncoder* renderBundleEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.RenderBundleEncoderSetBindGroup(renderBundleEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderBundleEncoderSetBindGroup(RenderBundleEncoder* renderBundleEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.RenderBundleEncoderSetBindGroup(renderBundleEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderBundleEncoderSetIndexBuffer(RenderBundleEncoder* renderBundleEncoder, Buffer* buffer, IndexFormat format, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderBundleEncoderSetIndexBuffer(renderBundleEncoder, buffer, format, offset, size);
    }
    public unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, byte* label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(renderBundleEncoder, label);
    }
    public unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, in byte label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(renderBundleEncoder, label);
    }
    public unsafe void RenderBundleEncoderSetLabel(RenderBundleEncoder* renderBundleEncoder, string label)
    {
        WGPUBrowserNative.RenderBundleEncoderSetLabel(renderBundleEncoder, label);
    }
    public unsafe void RenderBundleEncoderSetPipeline(RenderBundleEncoder* renderBundleEncoder, RenderPipeline* pipeline)
    {
        WGPUBrowserNative.RenderBundleEncoderSetPipeline(renderBundleEncoder, pipeline);
    }
    public unsafe void RenderBundleEncoderSetVertexBuffer(RenderBundleEncoder* renderBundleEncoder, uint slot, Buffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderBundleEncoderSetVertexBuffer(renderBundleEncoder, slot, buffer, offset, size);
    }
    public unsafe void RenderBundleEncoderReference(RenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderReference(renderBundleEncoder);
    }
    public unsafe void RenderBundleEncoderRelease(RenderBundleEncoder* renderBundleEncoder)
    {
        WGPUBrowserNative.RenderBundleEncoderRelease(renderBundleEncoder);
    }
    public unsafe void RenderPassEncoderBeginOcclusionQuery(RenderPassEncoder* renderPassEncoder, uint queryIndex)
    {
        WGPUBrowserNative.RenderPassEncoderBeginOcclusionQuery(renderPassEncoder, queryIndex);
    }
    public unsafe void RenderPassEncoderBeginPipelineStatisticsQuery(RenderPassEncoder* renderPassEncoder, QuerySet* querySet, uint queryIndex)
    {
        WGPUBrowserNative.RenderPassEncoderBeginPipelineStatisticsQuery(renderPassEncoder, querySet, queryIndex);
    }
    public unsafe void RenderPassEncoderDraw(RenderPassEncoder* renderPassEncoder, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderPassEncoderDraw(renderPassEncoder, vertexCount, instanceCount, firstVertex, firstInstance);
    }
    public unsafe void RenderPassEncoderDrawIndexed(RenderPassEncoder* renderPassEncoder, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndexed(renderPassEncoder, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }
    public unsafe void RenderPassEncoderDrawIndexedIndirect(RenderPassEncoder* renderPassEncoder, Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndexedIndirect(renderPassEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void RenderPassEncoderDrawIndirect(RenderPassEncoder* renderPassEncoder, Buffer* indirectBuffer, ulong indirectOffset)
    {
        WGPUBrowserNative.RenderPassEncoderDrawIndirect(renderPassEncoder, indirectBuffer, indirectOffset);
    }
    public unsafe void RenderPassEncoderEnd(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEnd(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderEndOcclusionQuery(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEndOcclusionQuery(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderEndPipelineStatisticsQuery(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderEndPipelineStatisticsQuery(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderExecuteBundles(RenderPassEncoder* renderPassEncoder, nuint bundleCount, RenderBundle** bundles)
    {
        WGPUBrowserNative.RenderPassEncoderExecuteBundles(renderPassEncoder, bundleCount, bundles);
    }
    public unsafe void RenderPassEncoderExecuteBundles(RenderPassEncoder* renderPassEncoder, nuint bundleCount, ref RenderBundle* bundles)
    {
        WGPUBrowserNative.RenderPassEncoderExecuteBundles(renderPassEncoder, bundleCount, ref bundles);
    }
    public unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, byte* markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(renderPassEncoder, markerLabel);
    }
    public unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, in byte markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(renderPassEncoder, markerLabel);
    }
    public unsafe void RenderPassEncoderInsertDebugMarker(RenderPassEncoder* renderPassEncoder, string markerLabel)
    {
        WGPUBrowserNative.RenderPassEncoderInsertDebugMarker(renderPassEncoder, markerLabel);
    }
    public unsafe void RenderPassEncoderPopDebugGroup(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderPopDebugGroup(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, byte* groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(renderPassEncoder, groupLabel);
    }
    public unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, in byte groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(renderPassEncoder, groupLabel);
    }
    public unsafe void RenderPassEncoderPushDebugGroup(RenderPassEncoder* renderPassEncoder, string groupLabel)
    {
        WGPUBrowserNative.RenderPassEncoderPushDebugGroup(renderPassEncoder, groupLabel);
    }
    public unsafe void RenderPassEncoderSetBindGroup(RenderPassEncoder* renderPassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, uint* dynamicOffsets)
    {
        WGPUBrowserNative.RenderPassEncoderSetBindGroup(renderPassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderPassEncoderSetBindGroup(RenderPassEncoder* renderPassEncoder, uint groupIndex, BindGroup* group, nuint dynamicOffsetCount, in uint dynamicOffsets)
    {
        WGPUBrowserNative.RenderPassEncoderSetBindGroup(renderPassEncoder, groupIndex, group, dynamicOffsetCount, dynamicOffsets);
    }
    public unsafe void RenderPassEncoderSetBlendConstant(RenderPassEncoder* renderPassEncoder, Color* color)
    {
        WGPUBrowserNative.RenderPassEncoderSetBlendConstant(renderPassEncoder, color);
    }
    public unsafe void RenderPassEncoderSetBlendConstant(RenderPassEncoder* renderPassEncoder, in Color color)
    {
        WGPUBrowserNative.RenderPassEncoderSetBlendConstant(renderPassEncoder, color);
    }
    public unsafe void RenderPassEncoderSetIndexBuffer(RenderPassEncoder* renderPassEncoder, Buffer* buffer, IndexFormat format, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderPassEncoderSetIndexBuffer(renderPassEncoder, buffer, format, offset, size);
    }
    public unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, byte* label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(renderPassEncoder, label);
    }
    public unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, in byte label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(renderPassEncoder, label);
    }
    public unsafe void RenderPassEncoderSetLabel(RenderPassEncoder* renderPassEncoder, string label)
    {
        WGPUBrowserNative.RenderPassEncoderSetLabel(renderPassEncoder, label);
    }
    public unsafe void RenderPassEncoderSetPipeline(RenderPassEncoder* renderPassEncoder, RenderPipeline* pipeline)
    {
        WGPUBrowserNative.RenderPassEncoderSetPipeline(renderPassEncoder, pipeline);
    }
    public unsafe void RenderPassEncoderSetScissorRect(RenderPassEncoder* renderPassEncoder, uint x, uint y, uint width, uint height)
    {
        WGPUBrowserNative.RenderPassEncoderSetScissorRect(renderPassEncoder, x, y, width, height);
    }
    public unsafe void RenderPassEncoderSetStencilReference(RenderPassEncoder* renderPassEncoder, uint reference)
    {
        WGPUBrowserNative.RenderPassEncoderSetStencilReference(renderPassEncoder, reference);
    }
    public unsafe void RenderPassEncoderSetVertexBuffer(RenderPassEncoder* renderPassEncoder, uint slot, Buffer* buffer, ulong offset, ulong size)
    {
        WGPUBrowserNative.RenderPassEncoderSetVertexBuffer(renderPassEncoder, slot, buffer, offset, size);
    }
    public unsafe void RenderPassEncoderSetViewport(RenderPassEncoder* renderPassEncoder, float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        WGPUBrowserNative.RenderPassEncoderSetViewport(renderPassEncoder, x, y, width, height, minDepth, maxDepth);
    }
    public unsafe void RenderPassEncoderReference(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderReference(renderPassEncoder);
    }
    public unsafe void RenderPassEncoderRelease(RenderPassEncoder* renderPassEncoder)
    {
        WGPUBrowserNative.RenderPassEncoderRelease(renderPassEncoder);
    }
    public unsafe BindGroupLayout* RenderPipelineGetBindGroupLayout(RenderPipeline* renderPipeline, uint groupIndex)
    {
        return WGPUBrowserNative.RenderPipelineGetBindGroupLayout(renderPipeline, groupIndex);
    }
    public unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, byte* label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(renderPipeline, label);
    }
    public unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, in byte label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(renderPipeline, label);
    }
    public unsafe void RenderPipelineSetLabel(RenderPipeline* renderPipeline, string label)
    {
        WGPUBrowserNative.RenderPipelineSetLabel(renderPipeline, label);
    }
    public unsafe void RenderPipelineReference(RenderPipeline* renderPipeline)
    {
        WGPUBrowserNative.RenderPipelineReference(renderPipeline);
    }
    public unsafe void RenderPipelineRelease(RenderPipeline* renderPipeline)
    {
        WGPUBrowserNative.RenderPipelineRelease(renderPipeline);
    }
    public unsafe void SamplerSetLabel(Sampler* sampler, byte* label)
    {
        WGPUBrowserNative.SamplerSetLabel(sampler, label);
    }
    public unsafe void SamplerSetLabel(Sampler* sampler, in byte label)
    {
        WGPUBrowserNative.SamplerSetLabel(sampler, label);
    }
    public unsafe void SamplerSetLabel(Sampler* sampler, string label)
    {
        WGPUBrowserNative.SamplerSetLabel(sampler, label);
    }
    public unsafe void SamplerReference(Sampler* sampler)
    {
        WGPUBrowserNative.SamplerReference(sampler);
    }
    public unsafe void SamplerRelease(Sampler* sampler)
    {
        WGPUBrowserNative.SamplerRelease(sampler);
    }
    public unsafe void ShaderModuleGetCompilationInfo(ShaderModule* shaderModule, PfnCompilationInfoCallback callback, void* userdata)
    {
        WGPUBrowserNative.ShaderModuleGetCompilationInfo(shaderModule, callback, userdata);
    }
    public unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, byte* label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(shaderModule, label);
    }
    public unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, in byte label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(shaderModule, label);
    }
    public unsafe void ShaderModuleSetLabel(ShaderModule* shaderModule, string label)
    {
        WGPUBrowserNative.ShaderModuleSetLabel(shaderModule, label);
    }
    public unsafe void ShaderModuleReference(ShaderModule* shaderModule)
    {
        WGPUBrowserNative.ShaderModuleReference(shaderModule);
    }
    public unsafe void ShaderModuleRelease(ShaderModule* shaderModule)
    {
        WGPUBrowserNative.ShaderModuleRelease(shaderModule);
    }
    public unsafe void SurfaceConfigure(Surface* surface, SurfaceConfiguration* config)
    {
        WGPUBrowserNative.SurfaceConfigure(surface, config);
    }
    public unsafe void SurfaceConfigure(Surface* surface, in SurfaceConfiguration config)
    {
        WGPUBrowserNative.SurfaceConfigure(surface, config);
    }
    public unsafe void SurfaceGetCapabilities(Surface* surface, Adapter* adapter, SurfaceCapabilities* capabilities)
    {
        WGPUBrowserNative.SurfaceGetCapabilities(surface, adapter, capabilities);
    }
    public unsafe void SurfaceGetCapabilities(Surface* surface, Adapter* adapter, ref SurfaceCapabilities capabilities)
    {
        WGPUBrowserNative.SurfaceGetCapabilities(surface, adapter, ref capabilities);
    }
    public unsafe void SurfaceGetCurrentTexture(Surface* surface, SurfaceTexture* surfaceTexture)
    {
        WGPUBrowserNative.SurfaceGetCurrentTexture(surface, surfaceTexture);
    }
    public unsafe void SurfaceGetCurrentTexture(Surface* surface, ref SurfaceTexture surfaceTexture)
    {
        WGPUBrowserNative.SurfaceGetCurrentTexture(surface, ref surfaceTexture);
    }
    public unsafe TextureFormat SurfaceGetPreferredFormat(Surface* surface, Adapter* adapter)
    {
        return WGPUBrowserNative.SurfaceGetPreferredFormat(surface, adapter);
    }
    public unsafe void SurfacePresent(Surface* surface)
    {
        WGPUBrowserNative.SurfacePresent(surface);
    }
    public unsafe void SurfaceUnconfigure(Surface* surface)
    {
        WGPUBrowserNative.SurfaceUnconfigure(surface);
    }
    public unsafe void SurfaceReference(Surface* surface)
    {
        WGPUBrowserNative.SurfaceReference(surface);
    }
    public unsafe void SurfaceRelease(Surface* surface)
    {
        WGPUBrowserNative.SurfaceRelease(surface);
    }
    public unsafe void SurfaceCapabilitiesFreeMembers(SurfaceCapabilities capabilities)
    {
        WGPUBrowserNative.SurfaceCapabilitiesFreeMembers(capabilities);
    }
    public unsafe TextureView* TextureCreateView(Texture* texture, TextureViewDescriptor* descriptor)
    {
        return WGPUBrowserNative.TextureCreateView(texture, descriptor);
    }
    public unsafe TextureView* TextureCreateView(Texture* texture, in TextureViewDescriptor descriptor)
    {
        return WGPUBrowserNative.TextureCreateView(texture, descriptor);
    }
    public unsafe void TextureDestroy(Texture* texture)
    {
        WGPUBrowserNative.TextureDestroy(texture);
    }
    public unsafe uint TextureGetDepthOrArrayLayers(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetDepthOrArrayLayers(texture);
    }
    public unsafe TextureDimension TextureGetDimension(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetDimension(texture);
    }
    public unsafe TextureFormat TextureGetFormat(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetFormat(texture);
    }
    public unsafe uint TextureGetHeight(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetHeight(texture);
    }
    public unsafe uint TextureGetMipLevelCount(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetMipLevelCount(texture);
    }
    public unsafe uint TextureGetSampleCount(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetSampleCount(texture);
    }
    public unsafe TextureUsage TextureGetUsage(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetUsage(texture);
    }
    public unsafe uint TextureGetWidth(Texture* texture)
    {
        return WGPUBrowserNative.TextureGetWidth(texture);
    }
    public unsafe void TextureSetLabel(Texture* texture, byte* label)
    {
        WGPUBrowserNative.TextureSetLabel(texture, label);
    }
    public unsafe void TextureSetLabel(Texture* texture, in byte label)
    {
        WGPUBrowserNative.TextureSetLabel(texture, label);
    }
    public unsafe void TextureSetLabel(Texture* texture, string label)
    {
        WGPUBrowserNative.TextureSetLabel(texture, label);
    }
    public unsafe void TextureReference(Texture* texture)
    {
        WGPUBrowserNative.TextureReference(texture);
    }
    public unsafe void TextureRelease(Texture* texture)
    {
        WGPUBrowserNative.TextureRelease(texture);
    }
    public unsafe void TextureViewSetLabel(TextureView* textureView, byte* label)
    {
        WGPUBrowserNative.TextureViewSetLabel(textureView, label);
    }
    public unsafe void TextureViewSetLabel(TextureView* textureView, in byte label)
    {
        WGPUBrowserNative.TextureViewSetLabel(textureView, label);
    }
    public unsafe void TextureViewSetLabel(TextureView* textureView, string label)
    {
        WGPUBrowserNative.TextureViewSetLabel(textureView, label);
    }
    public unsafe void TextureViewReference(TextureView* textureView)
    {
        WGPUBrowserNative.TextureViewReference(textureView);
    }
    public unsafe void TextureViewRelease(TextureView* textureView)
    {
        WGPUBrowserNative.TextureViewRelease(textureView);
    }

    public unsafe WGPU.WGPUSwapChain* DeviceCreateSwapChain(Device* device, Surface* surface, WGPU.WGPUSwapChainDescriptor descriptor)
    {
        return WGPUBrowserNative.DeviceCreateSwapChain(device, surface, in descriptor);
    }
    public unsafe TextureView* SwapChainGetCurrentTextureView(WGPU.WGPUSwapChain* swapChain)
    {
        return WGPUBrowserNative.SwapChainGetCurrentTextureView(swapChain);
    }
}