namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class UIFontResources
{
    readonly Dictionary<Handle, (Handle<Texture2D> Atlas, Handle<SamplerAsset> Sampler)> fontData = [];

    public void SetFontData(Handle fontHandle, Handle<Texture2D> atlas, Handle<SamplerAsset> sampler)
        => fontData[fontHandle] = (atlas, sampler);

    public (Handle<Texture2D> Atlas, Handle<SamplerAsset> Sampler)? GetFontData(Handle fontHandle)
        => fontData.TryGetValue(fontHandle, out var data) ? data : null;
}
