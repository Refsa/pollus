namespace Pollus.Graphics.Rendering;

[Flags]
public enum BufferUsage
{
    None = 0,
    MapRead = 1,
    MapWrite = 2,
    CopySrc = 4,
    CopyDst = 8,
    Index = 0x10,
    Vertex = 0x20,
    Uniform = 0x40,
    Storage = 0x80,
    Indirect = 0x100,
    QueryResolve = 0x200,
}