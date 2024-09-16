namespace Pollus.Graphics.Rendering;

public struct BlendState
{
    public static readonly BlendState Default = new BlendState
    {
        Color = new BlendComponent
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
        Alpha = new BlendComponent
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.One,
            DstFactor = BlendFactor.Zero,
        },
    };

    public BlendComponent Color { get; init; }
    public BlendComponent Alpha { get; init; }
}
