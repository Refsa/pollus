namespace Pollus.Graphics.Rendering;

using System.Diagnostics.CodeAnalysis;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class Blit : IDisposable
{
    public static readonly Handle<Blit> Handle = new();

    const string BLIT_SHADER = """
    struct VertexOutput {
        @builtin(position) position : vec4f,
        @location(0) uv : vec2f,
    }

    @vertex
    fn vs_main(@builtin(vertex_index) vertex_index: u32) -> VertexOutput {
        var out: VertexOutput;
        
        out.uv = vec2f(
            f32((vertex_index << 1u) & 2u),
            f32(vertex_index & 2u)
        );

        out.position = vec4f(out.uv * 2.0 - 1.0, 0.0, 1.0);
        out.uv.y = 1.0 - out.uv.y;

        return out;
    }

    @group(0) @binding(0) var srcTexture: texture_2d<f32>;
    @group(0) @binding(1) var srcSampler: sampler;

    @fragment
    fn fs_main(in: VertexOutput) -> @location(0) vec4f {
        return textureSample(srcTexture, srcSampler, in.uv);
    }
    """;

    GPUSampler? sampler;
    GPUBindGroupLayout? bindGroupLayout;
    GPUShader? shaderModule;

    // TODO: should probably move this out of here, lifetime of BindGroup is tied to the texture view
    Dictionary<nint, GPUBindGroup> bindGroups = new();
    Dictionary<nint, GPURenderPipeline> pipelines = new();

    public void Dispose()
    {
        foreach (var pipeline in pipelines.Values) pipeline.Dispose();
        pipelines.Clear();

        foreach (var bindGroup in bindGroups.Values) bindGroup.Dispose();
        bindGroups.Clear();

        shaderModule?.Dispose();
        sampler?.Dispose();
        bindGroupLayout?.Dispose();
    }

    [MemberNotNull(nameof(sampler), nameof(bindGroupLayout), nameof(shaderModule))]
    void CreateSharedResources(IWGPUContext gpuContext)
    {
        sampler ??= gpuContext.CreateSampler(SamplerDescriptor.Default);
        bindGroupLayout ??= gpuContext.CreateBindGroupLayout(new()
        {
            Entries = [
                BindGroupLayoutEntry.TextureEntry(0, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
                BindGroupLayoutEntry.SamplerEntry(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
            ]
        });
        shaderModule ??= gpuContext.CreateShaderModule(new()
        {
            Label = """blit-shader""",
            Backend = ShaderBackend.WGSL,
            Content = BLIT_SHADER,
        });
    }

    GPURenderPipeline GetRenderPipeline(IWGPUContext gpuContext, GPUTextureView dest)
    {
        if (pipelines.TryGetValue(dest.Native, out var pipeline)) return pipeline;

        pipeline = gpuContext.CreateRenderPipeline(new()
        {
            Label = """blit-pipeline""",
            VertexState = new()
            {
                ShaderModule = shaderModule!,
                EntryPoint = """vs_main""",
            },
            FragmentState = new()
            {
                ShaderModule = shaderModule!,
                EntryPoint = """fs_main""",
                ColorTargets = [
                    ColorTargetState.Default with
                        {
                            Format = dest.Descriptor.Format,
                        }
                ]
            },
            MultisampleState = MultisampleState.Default,
            PrimitiveState = PrimitiveState.Default with
            {
                Topology = PrimitiveTopology.TriangleStrip,
                CullMode = CullMode.None,
                FrontFace = FrontFace.CW,
            },
            PipelineLayout = gpuContext.CreatePipelineLayout(new()
            {
                Label = """blit-pipeline-layout""",
                Layouts = [bindGroupLayout!]
            }),
        });
        pipelines.Add(dest.Native, pipeline);
        return pipeline;
    }

    GPUBindGroup GetBindGroup(IWGPUContext gpuContext, GPUTextureView source)
    {
        if (bindGroups.TryGetValue(source.Native, out var bindGroup)) return bindGroup;

        bindGroup = gpuContext.CreateBindGroup(new()
        {
            Layout = bindGroupLayout!,
            Entries = [BindGroupEntry.TextureEntry(0, source), BindGroupEntry.SamplerEntry(1, sampler!)]
        });
        bindGroups.Add(source.Native, bindGroup);
        return bindGroup;
    }

    public void BlitTexture(IWGPUContext gpuContext, GPUCommandEncoder encoder, GPUTextureView source, GPUTextureView dest, Color? clearValue = null)
    {
        CreateSharedResources(gpuContext);
        var pipeline = GetRenderPipeline(gpuContext, dest);
        var bindGroup = GetBindGroup(gpuContext, source);

        using var pass = encoder.BeginRenderPass(new()
        {
            ColorAttachments = stackalloc RenderPassColorAttachment[]
            {
                new()
                {
                    View = dest.Native,
                    LoadOp = clearValue is null ? LoadOp.Load : LoadOp.Clear,
                    StoreOp = StoreOp.Store,
                    ClearValue = clearValue ?? new(0.1f, 0.1f, 0.1f, 1.0f),
                }
            }
        });

        pass.SetPipeline(pipeline);
        pass.SetBindGroup(0, bindGroup);
        pass.Draw(4, 1, 0, 0);
    }
}