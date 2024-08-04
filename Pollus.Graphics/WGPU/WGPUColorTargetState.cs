namespace Pollus.Graphics.WGPU;

public struct WGPUColorTargetState
{
    public static readonly WGPUColorTargetState Default = new WGPUColorTargetState
    {
        Format = Silk.NET.WebGPU.TextureFormat.Bgra8Unorm,
        WriteMask = Silk.NET.WebGPU.ColorWriteMask.All,
        Blend = WGPUBlendState.Default,
    };

    public Silk.NET.WebGPU.TextureFormat Format;
    public Silk.NET.WebGPU.ColorWriteMask WriteMask;
    public WGPUBlendState? Blend;
}

public struct WGPUBlendState
{
    public static readonly WGPUBlendState Default = new WGPUBlendState
    {
        Color = new WGPUBlendComponent
        {
            Operation = Silk.NET.WebGPU.BlendOperation.Add,
            SrcFactor = Silk.NET.WebGPU.BlendFactor.SrcAlpha,
            DstFactor = Silk.NET.WebGPU.BlendFactor.OneMinusSrcAlpha,
        },
        Alpha = new WGPUBlendComponent
        {
            Operation = Silk.NET.WebGPU.BlendOperation.Add,
            SrcFactor = Silk.NET.WebGPU.BlendFactor.One,
            DstFactor = Silk.NET.WebGPU.BlendFactor.Zero,
        },
    };

    public WGPUBlendComponent Color { get; init; }
    public WGPUBlendComponent Alpha { get; init; }
}

public struct WGPUBlendComponent
{
    public Silk.NET.WebGPU.BlendOperation Operation { get; set; }
    public Silk.NET.WebGPU.BlendFactor SrcFactor { get; set; }
    public Silk.NET.WebGPU.BlendFactor DstFactor { get; set; }
}