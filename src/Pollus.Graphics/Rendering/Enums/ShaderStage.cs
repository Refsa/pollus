namespace Pollus.Graphics.Rendering;

[Flags]
public enum ShaderStage
{
    None = 0,
    Vertex = 1,
    Fragment = 2,
    Compute = 4,
}
