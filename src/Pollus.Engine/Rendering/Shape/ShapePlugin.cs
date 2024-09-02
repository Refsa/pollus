namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class ShapePlugin : IPlugin
{
    static ShapePlugin()
    {
        AssetsFetch<Shape>.Register();
        AssetsFetch<ShapeMaterial>.Register();
        ResourceFetch<ShapeBatches>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<ShapeMaterial>());
        world.Resources.Get<RenderAssets>().AddLoader(new ShapeRenderDataLoader());
        world.Resources.Get<RenderSteps>().Add(new ShapeBatchDraw());
        world.Resources.Add(new ShapeBatches());

        world.Schedule.AddSystems(CoreStage.PreRender, [new ExtractShapesSystem()]);
    }
}

public struct ShapeDraw : IComponent
{
    public static EntityBuilder<ShapeDraw, Transform2> Bundle => new(
        new()
        {
            MaterialHandle = Handle<ShapeMaterial>.Null,
            ShapeHandle = Handle<Shape>.Null,
            Color = Color.WHITE,
        },
        Transform2.Default
    );

    public required Handle<ShapeMaterial> MaterialHandle;
    public required Handle<Shape> ShapeHandle;
    public required Color Color;
}

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

    public required Handle<ShaderAsset> ShaderSource { get; set; }
}