namespace Pollus.Graphics.WGPU;

using Pollus.Mathematics;
using Silk.NET.WebGPU;

public struct WGPURenderPassColorAttachment
{
    nint padding1;
    public nint View;
    public nint ResolveTarget;

    public LoadOp LoadOp;
    public StoreOp StoreOp;
    public Vector4<double> ClearValue;

    public WGPURenderPassColorAttachment(nint textureView, nint resolveTarget, Vector4<double> clearValue, LoadOp loadOp, StoreOp storeOp)
    {
        this.View = textureView;
        this.ResolveTarget = resolveTarget;
        LoadOp = loadOp;
        StoreOp = storeOp;
        ClearValue = clearValue;
    }
}
