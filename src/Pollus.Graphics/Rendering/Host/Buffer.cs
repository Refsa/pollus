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
    BufferUsage Usage { get; }
    BufferType Type { get; }
    ulong SizeInBytes { get; }

    void WriteTo(GPUBuffer target, int offset);
}
