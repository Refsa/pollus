namespace Pollus.Graphics.WGPU;

public struct WGPUFragmentState
{
    public WGPUShaderModule ShaderModule;
    public string EntryPoint;

    public WGPUConstantEntry[]? Constants;

    public WGPUColorTargetState[]? ColorTargets;
}
