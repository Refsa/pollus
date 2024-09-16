namespace Pollus.Graphics.Rendering;

public struct BlendComponent
{
    public BlendOperation Operation { get; set; }
    public BlendFactor SrcFactor { get; set; }
    public BlendFactor DstFactor { get; set; }
}

public enum BlendOperation
{
    Add = 0,
    Subtract = 1,
    ReverseSubtract = 2,
    Min = 3,
    Max = 4,
}

public enum BlendFactor
{
    Zero = 0,
    One = 1,
    Src = 2,
    OneMinusSrc = 3,
    SrcAlpha = 4,
    OneMinusSrcAlpha = 5,
    Dst = 6,
    OneMinusDst = 7,
    DstAlpha = 8,
    OneMinusDstAlpha = 9,
    SrcAlphaSaturated = 10,
    Constant = 11,
    OneMinusConstant = 12,
}