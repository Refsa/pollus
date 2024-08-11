namespace Pollus.Graphics.Rendering;

public struct VertexBufferLayout
{
    public ulong Stride;
    public Silk.NET.WebGPU.VertexStepMode StepMode;
    public VertexAttribute[] Attributes;

    public VertexBufferLayout(Silk.NET.WebGPU.VertexStepMode stepMode, uint startShaderLocation, Span<VertexFormat> formats)
    {
        Stride = 0;
        var expandedFormatCounts = 0;
        for (int i = 0; i < formats.Length; i++)
        {
            Stride += formats[i].Stride();
            expandedFormatCounts += formats[i].GetFormatCount();
        }
        StepMode = stepMode;
        Attributes = new VertexAttribute[expandedFormatCounts];

        ulong offset = 0;
        for (int i = 0; i < formats.Length; i++)
        {
            for (int k = 0; k < formats[i].GetFormatCount(); k++)
            {
                var nativeFormat = formats[i].GetNativeFormat();
                Attributes[i + k] = new VertexAttribute(nativeFormat, offset, startShaderLocation++);
                offset += nativeFormat.Stride();
            }
        }
    }

    public static VertexBufferLayout Vertex(uint startShaderLocation, Span<VertexFormat> formats)
    {
        return new VertexBufferLayout(Silk.NET.WebGPU.VertexStepMode.Vertex, startShaderLocation, formats);
    }

    public static VertexBufferLayout Instance(uint startShaderLocation, Span<VertexFormat> formats)
    {
        return new VertexBufferLayout(Silk.NET.WebGPU.VertexStepMode.Instance, startShaderLocation, formats);
    }
}

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