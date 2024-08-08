namespace Pollus.Graphics.Rendering;

public enum ShaderBackend
{
    WGSL,
}

public class ShaderModuleDescriptor
{
    public ShaderBackend Backend { get; init; }
    public string Label { get; init; }
    public string Content { get; init; }
}
