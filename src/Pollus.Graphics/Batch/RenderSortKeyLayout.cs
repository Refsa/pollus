namespace Pollus.Graphics;

public readonly record struct RenderSortKeyLayout(int Shift, ulong Suffix)
{
    public static RenderSortKeyLayout Direct { get; } = new(0, 0);

    public static RenderSortKeyLayout DepthThenBatch(int batchKey)
    {
        return new RenderSortKeyLayout(
            Shift: 32,
            Suffix: (uint)batchKey
        );
    }

    public ulong Compose(uint localKey)
    {
        return ((ulong)localKey << Shift) | Suffix;
    }
}