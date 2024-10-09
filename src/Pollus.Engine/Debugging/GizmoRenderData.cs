namespace Pollus.Debugging;

using Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

class GizmoFilledMaterial : IMaterial
{
    public static string Name => "GizmoFilledMaterial";

    public static VertexBufferLayout[] VertexLayouts => [
        VertexBufferLayout.Vertex(0, [VertexFormat.Float32x2, VertexFormat.Float32x2, VertexFormat.Float32x4]),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = "gizmo::filledPipeline",
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
            FrontFace = FrontFace.Ccw,
            CullMode = CullMode.None,
            Topology = PrimitiveTopology.TriangleStrip,
        },
    };

    public static BlendState? Blend => BlendState.Default with
    {
        Alpha = new()
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
        Color = new()
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
    };

    public Handle<ShaderAsset> ShaderSource { get; set; }
    public IBinding[][] Bindings => [[
        new UniformBinding<SceneUniform>(),
    ]];
}

class GizmoOutlinedMaterial : IMaterial
{
    public static string Name => "GizmoOutlinedMaterial";

    public static VertexBufferLayout[] VertexLayouts => [
        VertexBufferLayout.Vertex(0, [VertexFormat.Float32x2, VertexFormat.Float32x2, VertexFormat.Float32x4]),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor => new()
    {
        Label = "gizmo::outlinedPipeline",
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
            FrontFace = FrontFace.Ccw,
            CullMode = CullMode.None,
            Topology = PrimitiveTopology.LineStrip,
        },
    };

    public static BlendState? Blend => BlendState.Default with
    {
        Alpha = new()
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
        Color = new()
        {
            Operation = BlendOperation.Add,
            SrcFactor = BlendFactor.SrcAlpha,
            DstFactor = BlendFactor.OneMinusSrcAlpha,
        },
    };

    public Handle<ShaderAsset> ShaderSource { get; set; }
    public IBinding[][] Bindings => [[
        new UniformBinding<SceneUniform>(),
    ]];
}