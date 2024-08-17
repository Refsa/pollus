namespace Pollus.Graphics.Rendering;

public struct BufferBindingLayout
{
    public static readonly BufferBindingLayout Undefined = new()
    {
        Type = BufferBindingType.Undefined,
        HasDynamicOffset = false,
        MinBindingSize = 0
    };

    public BufferBindingType Type;
    public bool HasDynamicOffset;
    public ulong MinBindingSize;

    public BufferBindingLayout(BufferBindingType type, bool hasDynamicOffset, ulong minBindingSize)
    {
        Type = type;
        HasDynamicOffset = hasDynamicOffset;
        MinBindingSize = minBindingSize;
    }
}
