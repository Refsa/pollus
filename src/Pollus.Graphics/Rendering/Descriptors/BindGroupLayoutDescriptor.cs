namespace Pollus.Graphics.Rendering;

public ref struct BindGroupLayoutDescriptor
{
    public ReadOnlySpan<char> Label { get; init; }
    public BindGroupLayoutEntry[] Entries { get; init; }
}
