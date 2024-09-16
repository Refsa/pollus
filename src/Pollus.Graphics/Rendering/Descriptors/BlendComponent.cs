namespace Pollus.Graphics.Rendering;

public struct BlendComponent
{
    public BlendOperation Operation { get; set; }
    public BlendFactor SrcFactor { get; set; }
    public BlendFactor DstFactor { get; set; }
}