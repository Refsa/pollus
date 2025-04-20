namespace Pollus.Emscripten;

public struct WGPULimits_Browser
{
    public nuint MaxTextureDimension1D;
    public nuint MaxTextureDimension2D;
    public nuint MaxTextureDimension3D;
    public nuint MaxTextureArrayLayers;
    public nuint MaxBindGroups;
    public nuint MaxDynamicUniformBuffersPerPipelineLayout;
    public nuint MaxDynamicStorageBuffersPerPipelineLayout;
    public nuint MaxSampledTexturesPerShaderStage;
    public nuint MaxSamplersPerShaderStage;
    public nuint MaxStorageBuffersPerShaderStage;
    public nuint MaxStorageTexturesPerShaderStage;
    public nuint MaxUniformBuffersPerShaderStage;
    public ulong MaxUniformBufferBindingSize;
    public ulong MaxStorageBufferBindingSize;
    public nuint MinUniformBufferOffsetAlignment;
    public nuint MinStorageBufferOffsetAlignment;
    public nuint MaxVertexBuffers;
    public nuint MaxVertexAttributes;
    public nuint MaxVertexBufferArrayStride;
    public nuint MaxInterStageShaderComponents;
    public nuint MaxInterStageShaderVariables;
    public nuint MaxColorAttachments;
    public nuint MaxComputeWorkgroupStorageSize;
    public nuint MaxComputeInvocationsPerWorkgroup;
    public nuint MaxComputeWorkgroupSizeX;
    public nuint MaxComputeWorkgroupSizeY;
    public nuint MaxComputeWorkgroupSizeZ;
    public nuint MaxComputeWorkgroupsPerDimension;
}
