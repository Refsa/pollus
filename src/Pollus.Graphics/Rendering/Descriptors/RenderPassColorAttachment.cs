#pragma warning disable CS0169

namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.Platform;
using Pollus.Mathematics;

public struct RenderPassColorAttachment
{
    nint padding1;
    public required NativeHandle<TextureViewTag> View;
    public uint? DepthSlice;
    public NativeHandle<TextureViewTag>? ResolveTarget;

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