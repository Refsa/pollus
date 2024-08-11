namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public struct RenderPassColorAttachment
{
    nint padding1;
    public nint View;
    public nint ResolveTarget;

    public Silk.NET.WebGPU.LoadOp LoadOp;
    public Silk.NET.WebGPU.StoreOp StoreOp;
    public Vec4<double> ClearValue;

    public RenderPassColorAttachment(nint textureView, nint resolveTarget, Vec4<double> clearValue, Silk.NET.WebGPU.LoadOp loadOp, Silk.NET.WebGPU.StoreOp storeOp)
    {
        this.View = textureView;
        this.ResolveTarget = resolveTarget;
        LoadOp = loadOp;
        StoreOp = storeOp;
        ClearValue = clearValue;
    }
}
