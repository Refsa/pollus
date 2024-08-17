namespace Pollus.Graphics.Rendering;

public struct ColorTargetState
{
    public static readonly ColorTargetState Default = new ColorTargetState
    {
        Format = TextureFormat.Bgra8Unorm,
        WriteMask = Silk.NET.WebGPU.ColorWriteMask.All,
        Blend = BlendState.Default,
    };

    public TextureFormat Format;
    public Silk.NET.WebGPU.ColorWriteMask WriteMask;
    public BlendState? Blend;
}
