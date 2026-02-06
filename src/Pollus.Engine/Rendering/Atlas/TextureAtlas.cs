namespace Pollus.Engine.Rendering;

using Pollus.Core.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

[Asset]
public partial class TextureAtlas
{
    Rect[] slices;
    Dictionary<string, int> sliceNames;

    public required string Name { get; init; }
    public required Handle<Texture2D> TextureHandle { get; init; }

    public Rect GetRect(int index) => slices[index];
    public Rect GetRect(string name) => slices[sliceNames[name]];

    public static TextureAtlas From(string name, Handle<Texture2D> texture, params ReadOnlySpan<(string name, Rect rect)> slicesByName)
    {
        var sliceNames = new Dictionary<string, int>();
        var slices = new Rect[slicesByName.Length];
        var idx = 0;
        foreach (var kvp in slicesByName)
        {
            sliceNames[kvp.name] = idx;
            slices[idx++] = kvp.rect;
        }

        return new()
        {
            Name = name,
            TextureHandle = texture,
            slices = slices,
            sliceNames = sliceNames
        };
    }

    public static TextureAtlas FromGrid(string name, Handle<Texture2D> texture, int rows, int cols, Vec2<int> size)
    {
        var slices = new Rect[rows * cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                slices[i * cols + j] = Rect.FromOriginSize(
                    new Vec2f(j * size.X, i * size.Y),
                    new Vec2f(size.X, size.Y)
                );
            }
        }

        return new()
        {
            Name = name,
            TextureHandle = texture,
            slices = slices,
            sliceNames = Enumerable.Range(0, slices.Length).ToDictionary(s => s.ToString(), s => s),
        };
    }
}