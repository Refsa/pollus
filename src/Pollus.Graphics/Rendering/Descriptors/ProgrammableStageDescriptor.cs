namespace Pollus.Graphics.Rendering;

public ref struct ProgrammableStageDescriptor
{
    public GPUShader Shader { get; init; }
    public string EntryPoint { get; init; }
    public ReadOnlySpan<ConstantEntry> Constants { get; init; }
}