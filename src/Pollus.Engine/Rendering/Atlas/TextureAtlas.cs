namespace Pollus.Engine.Rendering;

using Pollus.Core.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

[Asset]
public partial class TextureAtlas
{
    readonly Rect<int>[] slices;
    readonly Dictionary<string, int>? sliceNames;

    public string Name { get; }
    public Handle<Texture2D> TextureHandle { get; }
    public int Count => slices.Length;

    TextureAtlas(string name, Handle<Texture2D> textureHandle, Rect<int>[] slices, Dictionary<string, int>? sliceNames)
    {
        Name = name;
        TextureHandle = textureHandle;
        this.slices = slices;
        this.sliceNames = sliceNames;
    }

    public Rect<int> GetRect(int index) => slices[index];
    public Rect<int> GetRect(string name) => sliceNames is not null && sliceNames.TryGetValue(name, out var idx)
        ? slices[idx]
        : throw new KeyNotFoundException($"TextureAtlas '{Name}' has no slice named '{name}'");

    public bool TryGetRect(string name, out Rect<int> rect)
    {
        if (sliceNames is not null && sliceNames.TryGetValue(name, out var idx))
        {
            rect = slices[idx];
            return true;
        }
        rect = default;
        return false;
    }

    public static TextureAtlas From(string name, Handle<Texture2D> texture, params ReadOnlySpan<(string name, Rect<int> rect)> slicesByName)
    {
        var sliceNames = new Dictionary<string, int>();
        var slices = new Rect<int>[slicesByName.Length];
        var idx = 0;
        foreach (var kvp in slicesByName)
        {
            sliceNames[kvp.name] = idx;
            slices[idx++] = kvp.rect;
        }

        return new(name, texture, slices, sliceNames);
    }

    public static TextureAtlas FromGrid(string name, Handle<Texture2D> texture, int rows, int cols, Vec2<int> size)
    {
        var slices = new Rect<int>[rows * cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                slices[i * cols + j] = Rect<int>.FromOriginSize(
                    new(j * size.X, i * size.Y),
                    new(size.X, size.Y)
                );
            }
        }

        return new(name, texture, slices, null);
    }
}
