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
        Slice = Rect<int>.Zero,
        Color = Color.WHITE,
    };

    public required Handle Material;
    public required Rect<int> Slice;
    uint packedColor;
    RenderStep2D renderStep;

    public RenderStep2D RenderStep
    {
        readonly get => renderStep == 0 ? RenderStep2D.Main : renderStep;
        set => renderStep = value;
    }

    public Color Color
    {
        readonly get => PackUtils.UnpackUnorm8x4(packedColor);
        set => packedColor = PackUtils.PackUnorm8x4(value);
    }

    public uint PackedColor => packedColor;
}