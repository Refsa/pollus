namespace Pollus.Graphics.WGPU;

public struct WGPUColorTargetState
{
    public Silk.NET.WebGPU.TextureFormat Format;
    public Silk.NET.WebGPU.ColorWriteMask WriteMask;
    public Silk.NET.WebGPU.BlendState? Blend;
}