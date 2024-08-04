namespace Pollus.Graphics.WGPU;

using Silk.NET.WebGPU;

public struct WGPURenderPassDepthStencilAttachment
{
    public static readonly WGPURenderPassDepthStencilAttachment Default = new()
    {

    };

    public WGPUTextureView View;

    public LoadOp DepthLoadOp;
    public StoreOp DepthStoreOp;
    public float DepthClearValue;
    public bool DepthReadOnly;

    public LoadOp StencilLoadOp;
    public StoreOp StencilStoreOp;
    public uint StencilClearValue;
    public bool StencilReadOnly;
}