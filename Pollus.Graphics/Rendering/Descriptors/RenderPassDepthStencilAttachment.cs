namespace Pollus.Graphics.Rendering;

public struct RenderPassDepthStencilAttachment
{
    public static readonly RenderPassDepthStencilAttachment Default = new()
    {

    };

    public GPUTextureView View;

    public Silk.NET.WebGPU.LoadOp DepthLoadOp;
    public Silk.NET.WebGPU.StoreOp DepthStoreOp;
    public float DepthClearValue;
    public bool DepthReadOnly;

    public Silk.NET.WebGPU.LoadOp StencilLoadOp;
    public Silk.NET.WebGPU.StoreOp StencilStoreOp;
    public uint StencilClearValue;
    public bool StencilReadOnly;
}