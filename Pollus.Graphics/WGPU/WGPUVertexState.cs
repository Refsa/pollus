namespace Pollus.Graphics.WGPU;

public struct WGPUVertexState
{
    public WGPUShaderModule ShaderModule { get; init; }
    public string EntryPoint { get; init; }

    public WGPUConstantEntry[]? Constants { get; init; }
    public WGPUVertexBufferLayout[]? Layouts { get; init; }
}

public struct WGPUVertexBufferLayout
{
    public ulong Stride;
    public Silk.NET.WebGPU.VertexStepMode StepMode;
    public Silk.NET.WebGPU.VertexAttribute[] Attributes;
}
