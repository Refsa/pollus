namespace Pollus.Graphics.Rendering;

public enum VertexStepMode
{
#if BROWSER
    Undefined = 0x00000000,
    VertexBufferNotUsed = 0x00000001,
    Vertex = 0x00000002,
    Instance = 0x00000003,
    Force32 = 0x7FFFFFFF,
#else
    Vertex = 0,
    Instance = 1,
    VertexBufferNotUsed = 2,
    Force32 = int.MaxValue,
#endif
}