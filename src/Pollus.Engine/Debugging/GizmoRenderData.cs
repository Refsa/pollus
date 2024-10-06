namespace Pollus.Debugging;

using Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

class GizmoRenderData
{
    static readonly RenderPipelineDescriptor BasePipelineDescriptor = new()
    {
        Label = "gizmo::basePipeline",
        VertexState = new()
        {
            EntryPoint = "vs_main",
            Layouts = [
                VertexBufferLayout.Vertex(0, [VertexFormat.Float32x2, VertexFormat.Float32x2, VertexFormat.Float32x4])
            ]
        },
        FragmentState = new()
        {
            EntryPoint = "fs_main",
            ColorTargets = [ColorTargetState.Default],
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default,
    };

    bool isRenderResourcesSetup = false;
    Handle<GPUPipelineLayout> pipelineLayoutHandle = Handle<GPUPipelineLayout>.Null;
    public Handle<GPUBindGroup> BindGroupHandle = Handle<GPUBindGroup>.Null;

    public void Setup(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (isRenderResourcesSetup is true) return;

        using var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = "gizmo::bindGroupLayout",
            Entries = [
                BindGroupLayoutEntry.Uniform<SceneUniform>(0, ShaderStage.Vertex | ShaderStage.Fragment),
                ]
        });

        pipelineLayoutHandle = renderAssets.Add(gpuContext.CreatePipelineLayout(new()
        {
            Label = "gizmo::pipelineLayout",
            Layouts = [bindGroupLayout],
        }));

        var sceneUniformRenderData = renderAssets.Get<UniformRenderData>(new Handle<Uniform<SceneUniform>>(0));
        var sceneUniformBuffer = renderAssets.Get(sceneUniformRenderData.UniformBuffer);
        BindGroupHandle = renderAssets.Add(gpuContext.CreateBindGroup(new()
        {
            Label = "gizmo::bindGroup",
            Layout = bindGroupLayout,
            Entries = [
                BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer, 0),
                ]
        }));

        isRenderResourcesSetup = true;
    }

    public Handle<GPURenderPipeline> SetupPipeline(IWGPUContext gpuContext, RenderAssets renderAssets, bool filled)
    {
        Setup(gpuContext, renderAssets);

        using var gizmoShader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = "gizmo::shader",
            Content = GizmoShaders.GIZMO_SHADER,
        });

        var pipelineLayout = renderAssets.Get(pipelineLayoutHandle);

        var pipelineDescriptor = BasePipelineDescriptor with
        {
            VertexState = BasePipelineDescriptor.VertexState with
            {
                ShaderModule = gizmoShader,
            },
            FragmentState = BasePipelineDescriptor.FragmentState with
            {
                ShaderModule = gizmoShader,
                ColorTargets = [ColorTargetState.Default with
                {
                    Format = gpuContext.GetSurfaceFormat(),
                }]
            },
            PrimitiveState = PrimitiveState.Default with
            {
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None,
                Topology = PrimitiveTopology.TriangleStrip,
            },
            PipelineLayout = pipelineLayout,
        };

        return renderAssets.Add(gpuContext.CreateRenderPipeline(pipelineDescriptor with
        {
            Label = $"gizmo::{(filled ? "filled" : "outlined")}Pipeline",
            PrimitiveState = filled switch
            {
                true => PrimitiveState.Default with
                {
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None,
                    Topology = PrimitiveTopology.TriangleStrip,
                },
                false => PrimitiveState.Default with
                {
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None,
                    Topology = PrimitiveTopology.LineStrip,
                },
            },
        }));
    }
}