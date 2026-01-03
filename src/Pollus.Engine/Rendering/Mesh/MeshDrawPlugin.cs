namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public partial struct MeshDraw<TMaterial> : IComponent
    where TMaterial : IMaterial
{
    public required Handle<MeshAsset> Mesh;
    public required Handle<TMaterial> Material;
}

[Asset]
public partial class Material : IMaterial
{
    public static string Name => "DefaultMaterial";

    public static VertexBufferLayout[] VertexLayouts =>
    [
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
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default with
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.Ccw,
            Topology = PrimitiveTopology.TriangleList,
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

public class MeshDrawPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial, IAsset
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
    ];

    public void Apply(World world)
    {
        world.AddPlugin(new MaterialPlugin<TMaterial>());

        world.Schedule.AddSystems(CoreStage.PreRender, [
            new ExtractMeshDrawSystem<TMaterial>(),
        ]);
    }
}