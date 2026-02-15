namespace Pollus.Engine.Rendering;

using Pollus.Utils;

public class UITextResources
{
    readonly Dictionary<Handle, Handle<UIFontMaterial>> fontMaterials = [];

    public void SetMaterial(Handle fontHandle, Handle<UIFontMaterial> material)
        => fontMaterials[fontHandle] = material;

    public Handle<UIFontMaterial> GetMaterial(Handle fontHandle)
        => fontMaterials.TryGetValue(fontHandle, out var mat) ? mat : Handle<UIFontMaterial>.Null;
}
