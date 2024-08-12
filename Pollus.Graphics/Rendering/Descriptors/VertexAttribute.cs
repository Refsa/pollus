namespace Pollus.Graphics.Rendering;

public struct VertexAttribute
{
    public VertexFormat Format;
    public ulong Offset;
    public uint ShaderLocation;

    public VertexAttribute(VertexFormat format, ulong offset, uint shaderLocation)
    {
        Format = format;
        Offset = offset;
        ShaderLocation = shaderLocation;
    }
}