namespace Pollus.Graphics.Rendering;

public struct BlendState
{
    public static readonly BlendState Default = new BlendState
    {
        Color = new BlendComponent
        {
            Operation = Silk.NET.WebGPU.BlendOperation.Add,
            SrcFactor = Silk.NET.WebGPU.BlendFactor.SrcAlpha,
            DstFactor = Silk.NET.WebGPU.BlendFactor.OneMinusSrcAlpha,
        },
        Alpha = new BlendComponent
        {
            Operation = Silk.NET.WebGPU.BlendOperation.Add,
            SrcFactor = Silk.NET.WebGPU.BlendFactor.One,
            DstFactor = Silk.NET.WebGPU.BlendFactor.Zero,
        },
    };

    public BlendComponent Color { get; init; }
    public BlendComponent Alpha { get; init; }
}
