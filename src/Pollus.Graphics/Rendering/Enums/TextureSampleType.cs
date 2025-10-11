namespace Pollus.Graphics.Rendering;

public enum TextureSampleType
{
    Undefined = 0,
    Float = 1,
    UnfilterableFloat = 2,
    Depth = 3,
    Sint = 4,
    Uint = 5,
    Force32 = 0x7FFFFFFF,
}