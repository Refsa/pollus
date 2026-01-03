namespace Pollus.Engine.Rendering;

using Collections;
using ECS;
using Transform;
using Utils;

public partial struct TextDraw : IComponent
{
    public static readonly EntityBuilder<TextDraw, TextMesh, Transform2D> Bundle = Entity.With(
        TextDraw.Default,
        TextMesh.Default,
        Transform2D.Default
    );

    public static readonly TextDraw Default = new TextDraw()
    {
        Font = Handle<FontAsset>.Null,
        Color = Color.WHITE,
        Size = 12f,
        Text = NativeUtf8.Null,
    };

    public required Handle<FontAsset> Font;
    public required Color Color;
    public required float Size;
    public bool IsDirty = false;

    NativeUtf8 text;

    public required NativeUtf8 Text
    {
        get => text;
        set
        {
            text.Dispose();
            text = value;
            IsDirty = true;
        }
    }

    public TextDraw()
    {
        text = NativeUtf8.Null;
    }
}

public partial struct TextMesh : IComponent
{
    public static readonly TextMesh Default = new TextMesh()
    {
        Mesh = Handle<TextMeshAsset>.Null,
        Material = Handle<FontMaterial>.Null,
    };

    public required Handle<TextMeshAsset> Mesh;
    public required Handle<FontMaterial> Material;
}