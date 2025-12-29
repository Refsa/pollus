namespace Pollus.Engine.Rendering;

using Core.Assets;
using Core.Serialization;
using Pollus.Graphics.Rendering;

[Serialize, Asset]
public partial class SamplerAsset
{
    public required SamplerDescriptor Descriptor { get; set; }

    public static implicit operator SamplerAsset(SamplerDescriptor descriptor) => new() { Descriptor = descriptor };
}