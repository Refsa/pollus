namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPULimits
{
    public uint maxTextureDimension1D;
    public uint maxTextureDimension2D;
    public uint maxTextureDimension3D;
    public uint maxTextureArrayLayers;
    public uint maxBindGroups;
    public uint maxBindGroupsPlusVertexBuffers;
    public uint maxBindingsPerBindGroup;
    public uint maxDynamicUniformBuffersPerPipelineLayout;
    public uint maxDynamicStorageBuffersPerPipelineLayout;
    public uint maxSampledTexturesPerShaderStage;
    public uint maxSamplersPerShaderStage;
    public uint maxStorageBuffersPerShaderStage;
    public uint maxStorageTexturesPerShaderStage;
    public uint maxUniformBuffersPerShaderStage;
    public ulong maxUniformBufferBindingSize;
    public ulong maxStorageBufferBindingSize;
    public uint minUniformBufferOffsetAlignment;
    public uint minStorageBufferOffsetAlignment;
    public uint maxVertexBuffers;
    public ulong maxBufferSize;
    public uint maxVertexAttributes;
    public uint maxVertexBufferArrayStride;
    public uint maxInterStageShaderComponents;
    public uint maxInterStageShaderVariables;
    public uint maxColorAttachments;
    public uint maxColorAttachmentBytesPerSample;
    public uint maxComputeWorkgroupStorageSize;
    public uint maxComputeInvocationsPerWorkgroup;
    public uint maxComputeWorkgroupSizeX;
    public uint maxComputeWorkgroupSizeY;
    public uint maxComputeWorkgroupSizeZ;
    public uint maxComputeWorkgroupsPerDimension;
}
