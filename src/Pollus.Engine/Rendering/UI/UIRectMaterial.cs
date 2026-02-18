namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

[Asset]
public partial class UIRectMaterial : IMaterial
{
    public static string Name => "ui-rect";

    public static VertexBufferLayout[] VertexLayouts =>
    [
        VertexBufferLayout.Instance(0, [
            VertexFormat.Float32x4, // PosSize
            VertexFormat.Float32x4, // BackgroundColor
            VertexFormat.Float32x4, // BorderColor
            VertexFormat.Float32x4, // BorderRadius
            VertexFormat.Float32x4, // BorderWidths
            VertexFormat.Float32x4, // Extra (x=ShapeType, y=OutlineWidth, z=OutlineOffset, w=reserved)
            VertexFormat.Float32x4, // OutlineColor
            VertexFormat.Float32x4, // UVRect (minU, minV, sizeU, sizeV)
        ]),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = "ui-rect-render-pipeline",
        VertexState = new()
        {
            EntryPoint = "vs_main",
            Layouts = VertexLayouts,
        },
        FragmentState = new()
        {
            EntryPoint = "fs_main",
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default with
        {
            Topology = PrimitiveTopology.TriangleStrip,
            CullMode = CullMode.None,
            FrontFace = FrontFace.Ccw,
        },
    };

    public static BlendState? Blend => BlendState.Default with
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
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
    };

    public IBinding[][] Bindings =>
    [
        [new UniformBinding<UIViewportUniform>(), Texture, Sampler]
    ];

    public required Handle<ShaderAsset> ShaderSource { get; set; }
    public required TextureBinding Texture { get; set; }
    public required SamplerBinding Sampler { get; set; }
}
