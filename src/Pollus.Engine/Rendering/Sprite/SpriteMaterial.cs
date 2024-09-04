namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class SpriteMaterial : IMaterial
{
    public static readonly VertexFormat[] InstanceFormats = [
        VertexFormat.Mat3x4,
        VertexFormat.Float32x4,
        VertexFormat.Float32x4,
    ];

    public static string Name => "SpriteMaterial";

    public static VertexBufferLayout[] VertexLayouts => [
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

    public IBinding[][] Bindings => [
        [new UniformBinding<SceneUniform>(), Texture, Sampler]
    ];

    public required Handle<ShaderAsset> ShaderSource { get; set; }
    public required TextureBinding Texture { get; set; }
    public required SamplerBinding Sampler { get; set; }
}
