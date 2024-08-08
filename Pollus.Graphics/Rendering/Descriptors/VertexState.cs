namespace Pollus.Graphics.Rendering;

public struct VertexState
{
    public GPUShader ShaderModule { get; init; }
    public string EntryPoint { get; init; }

    public ConstantEntry[]? Constants { get; init; }
    public VertexBufferLayout[]? Layouts { get; init; }
}
