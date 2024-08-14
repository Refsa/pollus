namespace Pollus.Emscripten;

using Pollus.Mathematics;
using Silk.NET.WebGPU;

unsafe public struct WGPURenderPassColorAttachment_Browser
{
    public TextureView* View; // nullable
    public TextureView* ResolveTarget; // nullable
    public LoadOp LoadOp;
    public StoreOp StoreOp;
    public Vec4<double> ClearValue;
}