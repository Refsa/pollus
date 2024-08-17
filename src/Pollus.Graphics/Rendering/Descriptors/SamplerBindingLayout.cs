namespace Pollus.Graphics.Rendering;

public struct SamplerBindingLayout
{
    public static readonly SamplerBindingLayout Undefined = new()
    {
        Type = SamplerBindingType.Undefined,
    };

    public SamplerBindingType Type;
}
