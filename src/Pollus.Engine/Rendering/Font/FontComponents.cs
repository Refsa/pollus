namespace Pollus.Engine.Rendering;

using Collections;
using ECS;
using Transform;
using Utils;

public interface ITextDraw
{
    public bool IsDirty { get; set; }
    public NativeUtf8 Text { get; set; }
    public float Size { get; set; }
    public Color Color { get; set; }
}

[Required<TextMesh>, Required<TextFont>, Required<GlobalTransform>]
public partial struct TextDraw : IComponent, IDefault<TextDraw>, ITextDraw
{
    public static readonly EntityBuilder<TextDraw, TextMesh, TextFont, Transform2D, GlobalTransform> Bundle = Entity.With(
        TextDraw.Default,
        TextMesh.Default,
        TextFont.Default,
        Transform2D.Default,
        GlobalTransform.Default
    );

    public static TextDraw Default { get; } = new TextDraw()
    {
        Color = Color.WHITE,
        Size = 12f,
        Text = NativeUtf8.Null,
    };

    public required Color Color { get; set; }
    public required float Size { get; set; }
    public bool IsDirty { get; set; }

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

public partial struct TextFont : IComponent, IDefault<TextFont>
{
    public static TextFont Default { get; } = new()
    {
        Font = Handle<FontAsset>.Null,
        Material = Handle.Null,
        RenderStep = RenderStep2D.Main,
    };

    public required Handle<FontAsset> Font;
    public required Handle Material;
    public required RenderStep2D RenderStep;
}

public partial struct TextMesh : IComponent, IDefault<TextMesh>
{
    public static TextMesh Default { get; } = new()
    {
        Mesh = Handle<TextMeshAsset>.Null,
    };

    public required Handle<TextMeshAsset> Mesh;
}
