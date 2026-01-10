namespace Pollus.Graphics;

using Utils;

public record struct RendererKey
{
    public static readonly RendererKey Null = new() { Key = -1 };

    public int Key { get; init; }

    public static RendererKey From<T>()
    {
        return new RendererKey()
        {
            Key = TypeLookup.ID<T>(),
        };
    }

    public override int GetHashCode() => Key;
    public override string ToString() => $"RendererKey({Key})";
}