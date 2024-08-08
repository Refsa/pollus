namespace Pollus.Graphics.Rendering;

public struct SamplerBindingLayout
{
    public static readonly SamplerBindingLayout Undefined = new()
    {
        Type = Silk.NET.WebGPU.SamplerBindingType.Undefined,
    };

    public Silk.NET.WebGPU.SamplerBindingType Type;
}
