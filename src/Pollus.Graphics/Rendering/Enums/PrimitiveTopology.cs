namespace Pollus.Graphics.Rendering;

public enum PrimitiveTopology
{
#if BROWSER
    Undefined = 0x00000000,
    PointList = 0x00000001,
    LineList = 0x00000002,
    LineStrip = 0x00000003,
    TriangleList = 0x00000004,
    TriangleStrip = 0x00000005,
    Force32 = 0x7FFFFFFF,
#else
    PointList = 0,
    LineList = 1,
    LineStrip = 2,
    TriangleList = 3,
    TriangleStrip = 4,
#endif
}