namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

public partial record struct UIImage() : IComponent
{
    public Handle Texture; // Handle<Texture2D>
    public Handle Sampler; // Handle<SamplerAsset>, Handle.Null = use default
    public Rect Slice = new(Vec2f.Zero, Vec2f.One); // UV rect, default = full image
}
