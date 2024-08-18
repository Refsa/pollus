namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;

public class SamplerAsset
{
    public required SamplerDescriptor Descriptor { get; init; }

    public static implicit operator SamplerAsset(SamplerDescriptor descriptor) => new() { Descriptor = descriptor };
}