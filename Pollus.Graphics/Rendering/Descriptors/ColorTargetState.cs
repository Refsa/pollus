namespace Pollus.Graphics.Rendering;

public struct ColorTargetState
{
    public static readonly ColorTargetState Default = new ColorTargetState
    {
        Format = Silk.NET.WebGPU.TextureFormat.Bgra8Unorm,
        WriteMask = Silk.NET.WebGPU.ColorWriteMask.All,
        Blend = BlendState.Default,
    };

    public Silk.NET.WebGPU.TextureFormat Format;
    public Silk.NET.WebGPU.ColorWriteMask WriteMask;
    public BlendState? Blend;
}
