namespace Pollus.Emscripten;

using Pollus.Mathematics;

unsafe public struct WGPURenderPassColorAttachment_Browser()
{
    public Silk.NET.WebGPU.ChainedStruct* NextInChain;
    public Silk.NET.WebGPU.TextureView* View; // nullable
    public uint DepthSlice = uint.MaxValue;
    public Silk.NET.WebGPU.TextureView* ResolveTarget; // nullable
    public Silk.NET.WebGPU.LoadOp LoadOp;
    public Silk.NET.WebGPU.StoreOp StoreOp;
    public Vec4<double> ClearValue;
}