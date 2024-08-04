namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

public struct WGPUSamplerBindingLayout
{
    public static readonly SamplerBindingLayout Undefined = new()
    {
        Type = SamplerBindingType.Undefined,
    };

    public SamplerBindingType Type;
}
