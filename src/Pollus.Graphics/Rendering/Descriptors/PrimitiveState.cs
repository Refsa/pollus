namespace Pollus.Graphics.Rendering;

public struct PrimitiveState
{
    public static readonly PrimitiveState Default = new()
    {
        Topology = PrimitiveTopology.TriangleList,
        FrontFace = FrontFace.CW,
        CullMode = CullMode.Back,
        IndexFormat = IndexFormat.Undefined,
    };

    public PrimitiveTopology Topology;
    public IndexFormat IndexFormat;
    public FrontFace FrontFace;
    public CullMode CullMode;
}
