namespace Pollus.Graphics.Rendering;

public struct BlendComponent
{
    public Silk.NET.WebGPU.BlendOperation Operation { get; set; }
    public Silk.NET.WebGPU.BlendFactor SrcFactor { get; set; }
    public Silk.NET.WebGPU.BlendFactor DstFactor { get; set; }
}