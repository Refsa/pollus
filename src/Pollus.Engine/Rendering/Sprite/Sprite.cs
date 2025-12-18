namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

public partial struct Sprite : IComponent
{
    public required Handle<SpriteMaterial> Material;
    public required Rect Slice;
    public Color Color;
}