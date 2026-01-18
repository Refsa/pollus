namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;
using Transform;

[Required<Transform2D>, Required<GlobalTransform>]
public partial struct Sprite : IComponent, IDefault<Sprite>
{
    public static Sprite Default { get; } = new Sprite()
    {
        Material = Handle<SpriteMaterial>.Null,
        Slice = Rect.Zero,
        Color = Color.WHITE,
    };

    public required Handle<SpriteMaterial> Material;
    public required Rect Slice;
    public Color Color;
}