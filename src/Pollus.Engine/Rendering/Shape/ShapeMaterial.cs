namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class ShapeMaterial : IMaterial
{
    public static string Name => "shape";

    public static readonly VertexFormat[] InstanceFormats = [
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Model
        VertexFormat.Float32x4, // Color
    ];

    public static VertexBufferLayout[] VertexLayouts => [
        VertexBufferLayout.Vertex(0, [
            VertexFormat.Float32x2,
            VertexFormat.Float32x2,
        ]),
        VertexBufferLayout.Instance(2, InstanceFormats),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = "shape-render-pipeline",
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

    public IBinding[][] Bindings => [
        [new UniformBinding<SceneUniform>()]
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
}