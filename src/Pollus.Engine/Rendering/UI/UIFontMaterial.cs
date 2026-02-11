namespace Pollus.Engine.Rendering;

using Core.Assets;
using Graphics.Rendering;
using Utils;

[Asset]
public partial class UIFontMaterial : IMaterial
{
    public static string Name => "ui-font";

    public static readonly VertexFormat[] InstanceFormats =
    [
        VertexFormat.Float32x4, // Offset (xy=screen pos, zw=unused)
        VertexFormat.Float32x4, // Color
    ];

    public static VertexBufferLayout[] VertexLayouts =>
    [
        VertexBufferLayout.Vertex(0, [
            VertexFormat.Float32x2, // Position
            VertexFormat.Float32x2, // UV
            VertexFormat.Float32x4, // Color
        ]),
        VertexBufferLayout.Instance(3, InstanceFormats),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = "ui-font-render-pipeline",
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
            Topology = PrimitiveTopology.TriangleList,
            CullMode = CullMode.None,
            FrontFace = FrontFace.Ccw,
        },
    };

    public IBinding[][] Bindings =>
    [
        [new UniformBinding<UIViewportUniform>(), Texture, Sampler]
    ];

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

    public required Handle<ShaderAsset> ShaderSource { get; set; }
    public required TextureBinding Texture { get; set; }
    public required SamplerBinding Sampler { get; set; }
}
