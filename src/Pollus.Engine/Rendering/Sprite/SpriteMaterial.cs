namespace Pollus.Engine.Rendering;

using Core.Assets;
using Core.Serialization;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

[Serialize, Asset]
public partial class SpriteMaterial : IMaterial
{
    public static readonly VertexFormat[] InstanceFormats =
    [
        VertexFormat.Float32x4,
        VertexFormat.Float32x3,
        VertexFormat.Uint16x4,
        VertexFormat.Uint32,
    ];

    public static string Name => "SpriteMaterial";

    public static VertexBufferLayout[] VertexLayouts =>
    [
        VertexBufferLayout.Instance(0, InstanceFormats),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = """DefaultMaterial-render-pipeline""",
        VertexState = new()
        {
            EntryPoint = """vs_main""",
            Layouts = VertexLayouts,
        },
        FragmentState = new()
        {
            EntryPoint = """fs_main""",
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default with
        {
            Topology = PrimitiveTopology.TriangleStrip,
            FrontFace = FrontFace.Ccw,
        },
    };

    public IBinding[][] Bindings =>
    [
        [new UniformBinding<SceneUniform>(), Texture, Sampler]
    ];

    public required Handle<ShaderAsset> ShaderSource { get; set; }
    public required TextureBinding Texture { get; set; }
    public required SamplerBinding Sampler { get; set; }
}
