namespace Pollus.Graphics.Rendering;

public struct RenderPassDepthStencilAttachment
{
    public static readonly RenderPassDepthStencilAttachment Default = new()
    {

    };

    public GPUTextureView View;

    public LoadOp DepthLoadOp;
    public StoreOp DepthStoreOp;
    public float DepthClearValue;
    public bool DepthReadOnly;

    public LoadOp StencilLoadOp;
    public StoreOp StencilStoreOp;
    public uint StencilClearValue;
    public bool StencilReadOnly;
}