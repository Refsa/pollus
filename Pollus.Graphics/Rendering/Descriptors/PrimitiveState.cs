namespace Pollus.Graphics.Rendering;

public struct PrimitiveState
{
    public static readonly PrimitiveState Default = new PrimitiveState
    {
        Topology = Silk.NET.WebGPU.PrimitiveTopology.TriangleList,
        FrontFace = Silk.NET.WebGPU.FrontFace.Ccw,
        CullMode = Silk.NET.WebGPU.CullMode.None,
        IndexFormat = Silk.NET.WebGPU.IndexFormat.Undefined,
    };

    public Silk.NET.WebGPU.PrimitiveTopology Topology;
    public Silk.NET.WebGPU.IndexFormat IndexFormat;
    public Silk.NET.WebGPU.FrontFace FrontFace;
    public Silk.NET.WebGPU.CullMode CullMode;
}
