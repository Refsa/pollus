namespace Pollus.Graphics.Rendering;

public enum BufferType
{
    Vertex,
    Index,
    Uniform,
    DynamicUniform,
    Storage,
}

public interface IBufferData
{
    BufferType Usage { get; }
    ulong SizeInBytes { get; }
}