namespace Pollus.Engine.Rendering;

using Core.Assets;
using Mathematics;
using Utils;

[Asset]
public partial class SpriteAnimation
{
    public struct Frame()
    {
        public required Rect Rect { get; init; }
        public required float Duration { get; init; } = 100;
    }

    public required string Name { get; init; }
    public required Handle<TextureAtlas> AtlasHandle { get; init; }
    public required Frame[] Frames { get; init; }

    public Rect GetUVRect(int frame) => Frames[frame].Rect;
    public Frame GetFrame(int frame) => Frames[frame];

    public static SpriteAnimation From(string name, Handle atlasHandle, TextureAtlas atlas, int frameRate, ReadOnlySpan<string> slices)
    {
        var frames = new Frame[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            frames[i] = new()
            {
                Duration = 1f / frameRate,
                Rect = atlas.GetRect(slices[i]),
            };
        }

        return new()
        {
            Name = name,
            AtlasHandle = atlasHandle,
            Frames = frames,
        };
    }

    public static SpriteAnimation From(string name, Handle atlasHandle, TextureAtlas atlas, int frameRate, ReadOnlySpan<int> slices)
    {
        var frames = new Frame[slices.Length];
        for (int i = 0; i < slices.Length; i++)
        {
            frames[i] = new()
            {
                Duration = 1f / frameRate,
                Rect = atlas.GetRect(slices[i]),
            };
        }

        return new()
        {
            Name = name,
            AtlasHandle = atlasHandle,
            Frames = frames,
        };
    }
}
