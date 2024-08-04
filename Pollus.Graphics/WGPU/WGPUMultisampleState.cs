namespace Pollus.Graphics.WGPU;

public struct WGPUMultisampleState
{
    public static readonly WGPUMultisampleState Default = new(1, 0xFFFFFFFF, false);

    public uint Count;
    public uint Mask;
    public bool AlphaToCoverageEnabled;

    public WGPUMultisampleState(uint count, uint mask, bool alphaToCoverageEnabled)
    {
        Count = count;
        Mask = mask;
        AlphaToCoverageEnabled = alphaToCoverageEnabled;
    }
}
