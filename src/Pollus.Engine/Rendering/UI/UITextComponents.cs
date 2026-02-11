namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Utils;

public partial struct UITextFont : IComponent, IDefault<UITextFont>
{
    public static UITextFont Default { get; } = new()
    {
        Font = Handle<FontAsset>.Null,
        Material = Handle.Null,
    };

    public required Handle<FontAsset> Font;
    public Handle Material;
}

public class UITextResources
{
    readonly Dictionary<Handle, Handle<UIFontMaterial>> fontMaterials = [];

    public void SetMaterial(Handle fontHandle, Handle<UIFontMaterial> material)
        => fontMaterials[fontHandle] = material;

    public Handle<UIFontMaterial> GetMaterial(Handle fontHandle)
        => fontMaterials.TryGetValue(fontHandle, out var mat) ? mat : Handle<UIFontMaterial>.Null;
}
