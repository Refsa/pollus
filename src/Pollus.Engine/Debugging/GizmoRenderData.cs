namespace Pollus.Debugging;

using Core.Assets;
using Pollus.Graphics.WGPU;
using Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

[Asset]
partial class GizmoFilledMaterial : IMaterial
{
    public static string Name => "GizmoFilledMaterial";

    public static VertexBufferLayout[] VertexLayouts =>
    [
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

    public IBinding[][] Bindings =>
    [
        [
            new UniformBinding<SceneUniform>(),
        ]
    ];
}

[Asset]
partial class GizmoOutlinedMaterial : IMaterial
{
    public static string Name => "GizmoOutlinedMaterial";

    public static VertexBufferLayout[] VertexLayouts =>
    [
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

    public IBinding[][] Bindings =>
    [
        [
            new UniformBinding<SceneUniform>(),
        ]
    ];
}

class GizmoTextureMaterial
{
    public static readonly BindGroupLayoutEntry[] BindGroupLayoutEntries =
    [
        BindGroupLayoutEntry.Uniform<SceneUniform>(0, ShaderStage.Vertex | ShaderStage.Fragment),
        BindGroupLayoutEntry.TextureEntry(1, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
        BindGroupLayoutEntry.SamplerEntry(2, ShaderStage.Fragment, SamplerBindingType.Filtering),
    ];

    public static RenderPipelineDescriptor PipelineDescriptor(IWGPUContext gpuContext, GPUShader shaderModule, GPUPipelineLayout pipelineLayout) => new()
    {
        Label = """gizmo::fontPipeline""",
        VertexState = new()
        {
            ShaderModule = shaderModule,
            EntryPoint = """vs_main""",
            Layouts =
            [
                VertexBufferLayout.Vertex(0, [VertexFormat.Float32x2, VertexFormat.Float32x2, VertexFormat.Float32x4]),
            ],
        },
        FragmentState = new()
        {
            ShaderModule = shaderModule,
            EntryPoint = """fs_main""",
            ColorTargets =
            [
                ColorTargetState.Default with
                {
                    Format = gpuContext.GetSurfaceFormat(),
                    Blend = new BlendState()
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
                    },
                }
            ]
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default with
        {
            FrontFace = FrontFace.Ccw,
            CullMode = CullMode.None,
            Topology = PrimitiveTopology.TriangleStrip,
        },
        PipelineLayout = pipelineLayout,
    };
}