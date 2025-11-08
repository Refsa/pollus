#pragma warning disable CS0169

namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public struct RenderPassColorAttachment
{
    nint padding1;
    public required nint View;
    public uint DepthSlice;
    public nint ResolveTarget;

    public LoadOp LoadOp;
    public StoreOp StoreOp;
    public Vec4<double> ClearValue;

    public RenderPassColorAttachment()
    {
        LoadOp = LoadOp.Clear;
        StoreOp = StoreOp.Store;
        ClearValue = Vec4<double>.One;
    }
}

#pragma warning restore CS0169