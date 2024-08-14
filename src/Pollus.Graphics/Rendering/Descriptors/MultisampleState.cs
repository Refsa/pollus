namespace Pollus.Graphics.Rendering;

public struct MultisampleState
{
    public static readonly MultisampleState Default = new(1, 0xFFFFFFFF, false);

    public uint Count;
    public uint Mask;
    public bool AlphaToCoverageEnabled;

    public MultisampleState(uint count, uint mask, bool alphaToCoverageEnabled)
    {
        Count = count;
        Mask = mask;
        AlphaToCoverageEnabled = alphaToCoverageEnabled;
    }
}
