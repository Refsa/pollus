namespace Pollus.Graphics.Rendering;

public struct VertexBufferLayout
{
    public ulong Stride;
    public Silk.NET.WebGPU.VertexStepMode StepMode;
    public Silk.NET.WebGPU.VertexAttribute[] Attributes;
}
