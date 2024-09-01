#pragma warning disable CS0169

namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public struct RenderPassColorAttachment
{
    nint padding1;
    public nint View;
    public nint ResolveTarget;

    public LoadOp LoadOp;
    public StoreOp StoreOp;
    public Vec4<double> ClearValue;

    public RenderPassColorAttachment(nint textureView, nint resolveTarget, Vec4<double> clearValue, LoadOp loadOp, StoreOp storeOp)
    {
        this.View = textureView;
        this.ResolveTarget = resolveTarget;
        LoadOp = loadOp;
        StoreOp = storeOp;
        ClearValue = clearValue;
    }
}

#pragma warning restore CS0169