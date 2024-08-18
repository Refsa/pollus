namespace Pollus.Engine.Rendering;

using System.Text;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;

public class Material : IMaterial
{
    public static string Name => "DefaultMaterial";
    public static VertexBufferLayout[] VertexLayouts => [
        VertexBufferLayout.Vertex(0, [
            VertexFormat.Float32x3,
            VertexFormat.Float32x2,
        ]),
        VertexBufferLayout.Instance(5, [
            VertexFormat.Mat4x4,
        ]),
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
            ColorTargets = [
                ColorTargetState.Default with
                {
                    Format = TextureFormat.Rgba8UnormSrgb,
                }
            ],
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default,
    };

    public IBinding[][] Bindings => [
        [new UniformBinding<SceneUniform>(), Texture, Sampler]
    ];

    public required Handle<ShaderAsset> ShaderSource { get; set; }

    public required TextureBinding Texture { get; set; }
    public required SamplerBinding Sampler { get; set; }
}

public interface IMaterial
{
    public static abstract string Name { get; }
    public static abstract VertexBufferLayout[] VertexLayouts { get; }
    public static abstract RenderPipelineDescriptor PipelineDescriptor { get; }

    Handle<ShaderAsset> ShaderSource { get; set; }
    IBinding[][] Bindings { get; }
}

