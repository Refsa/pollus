namespace Pollus.Graphics.Rendering;

public struct BufferBindingLayout
{
    public static readonly BufferBindingLayout Undefined = new()
    {
        Type = Silk.NET.WebGPU.BufferBindingType.Undefined,
        HasDynamicOffset = false,
        MinBindingSize = 0
    };

    public Silk.NET.WebGPU.BufferBindingType Type;
    public bool HasDynamicOffset;
    public ulong MinBindingSize;

    public BufferBindingLayout(Silk.NET.WebGPU.BufferBindingType type, bool hasDynamicOffset, ulong minBindingSize)
    {
        Type = type;
        HasDynamicOffset = hasDynamicOffset;
        MinBindingSize = minBindingSize;
    }
}
