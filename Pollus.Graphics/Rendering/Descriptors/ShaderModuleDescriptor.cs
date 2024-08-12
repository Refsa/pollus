namespace Pollus.Graphics.Rendering;

public enum ShaderBackend
{
    WGSL,
}

public ref struct ShaderModuleDescriptor
{
    public ShaderBackend Backend { get; init; }
    public ReadOnlySpan<char> Label { get; init; }
    public ReadOnlySpan<byte> Content { get; init; }
}
