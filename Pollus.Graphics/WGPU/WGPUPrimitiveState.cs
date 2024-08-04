namespace Pollus.Graphics.WGPU;

public struct WGPUPrimitiveState
{
    public Silk.NET.WebGPU.PrimitiveTopology Topology;
    public Silk.NET.WebGPU.IndexFormat IndexFormat;
    public Silk.NET.WebGPU.FrontFace FrontFace;
    public Silk.NET.WebGPU.CullMode CullMode;
}
