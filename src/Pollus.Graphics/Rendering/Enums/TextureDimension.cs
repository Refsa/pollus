namespace Pollus.Graphics.Rendering;

public enum TextureDimension
{
#if BROWSER
    Undefined = 0x00000000,
    Dimension1D = 0x00000001,
    Dimension2D = 0x00000002,
    Dimension3D = 0x00000003,
    Force32 = 0x7FFFFFFF,
#else
    Dimension1D = 0,
    Dimension2D = 1,
    Dimension3D = 2,
#endif
}