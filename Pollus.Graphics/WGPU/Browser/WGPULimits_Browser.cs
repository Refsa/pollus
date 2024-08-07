namespace Pollus.Graphics.WGPU.Browser;

public struct WGPULimits_Browser
{
    public uint MaxTextureDimension1D;
    public uint MaxTextureDimension2D;
    public uint MaxTextureDimension3D;
    public uint MaxTextureArrayLayers;
    public uint MaxBindGroups;
    public uint MaxDynamicUniformBuffersPerPipelineLayout;
    public uint MaxDynamicStorageBuffersPerPipelineLayout;
    public uint MaxSampledTexturesPerShaderStage;
    public uint MaxSamplersPerShaderStage;
    public uint MaxStorageBuffersPerShaderStage;
    public uint MaxStorageTexturesPerShaderStage;
    public uint MaxUniformBuffersPerShaderStage;
    public ulong MaxUniformBufferBindingSize;
    public ulong MaxStorageBufferBindingSize;
    public uint MinUniformBufferOffsetAlignment;
    public uint MinStorageBufferOffsetAlignment;
    public uint MaxVertexBuffers;
    public uint MaxVertexAttributes;
    public uint MaxVertexBufferArrayStride;
    public uint MaxInterStageShaderComponents;
    public uint MaxInterStageShaderVariables;
    public uint MaxColorAttachments;
    public uint MaxComputeWorkgroupStorageSize;
    public uint MaxComputeInvocationsPerWorkgroup;
    public uint MaxComputeWorkgroupSizeX;
    public uint MaxComputeWorkgroupSizeY;
    public uint MaxComputeWorkgroupSizeZ;
    public uint MaxComputeWorkgroupsPerDimension;
}
