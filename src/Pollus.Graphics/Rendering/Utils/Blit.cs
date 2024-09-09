namespace Pollus.Graphics.Rendering;

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

    GPURenderPipeline? pipeline;
    GPUSampler? sampler;
    GPUBindGroupLayout? bindGroupLayout;

    // TODO: should probably move this out of here, lifetime of BindGroup is tied to the texture view
    Dictionary<nint, GPUBindGroup> bindGroups = new();

    public void Dispose()
    {
        foreach (var bindGroup in bindGroups.Values) bindGroup.Dispose();
        bindGroups.Clear();
        pipeline?.Dispose();
        sampler?.Dispose();
        bindGroupLayout?.Dispose();
    }

    public void BlitTexture(IWGPUContext gpuContext, GPUCommandEncoder encoder, GPUTextureView source, GPUTextureView dest)
    {
        bindGroupLayout ??= gpuContext.CreateBindGroupLayout(new()
        {
            Entries = [
                BindGroupLayoutEntry.TextureEntry(0, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
                BindGroupLayoutEntry.SamplerEntry(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
            ]
        });
        sampler ??= gpuContext.CreateSampler(SamplerDescriptor.Default);
        if (pipeline is null)
        {
            using var shaderModule = gpuContext.CreateShaderModule(new()
            {
                Label = """blit-shader""",
                Backend = ShaderBackend.WGSL,
                Content = BLIT_SHADER,
            });

            pipeline = gpuContext.CreateRenderPipeline(new()
            {
                Label = """blit-pipeline""",
                VertexState = new()
                {
                    ShaderModule = shaderModule,
                    EntryPoint = """vs_main""",
                },
                FragmentState = new()
                {
                    ShaderModule = shaderModule,
                    EntryPoint = """fs_main""",
                    ColorTargets = [
                        ColorTargetState.Default with
                        {
                            Format = TextureFormat.Rgba8UnormSrgb,
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
                    Layouts = [bindGroupLayout]
                }),
            });
        }

        if (!bindGroups.TryGetValue(source.Native, out var bindGroup))
        {
            bindGroup = gpuContext.CreateBindGroup(new()
            {
                Layout = bindGroupLayout,
                Entries = [BindGroupEntry.TextureEntry(0, source), BindGroupEntry.SamplerEntry(1, sampler)]
            });
            bindGroups.Add(source.Native, bindGroup);
        }

        using var pass = encoder.BeginRenderPass(new()
        {
            ColorAttachments = stackalloc RenderPassColorAttachment[]
            {
                new()
                {
                    View = dest.Native,
                    LoadOp = LoadOp.Load,
                    StoreOp = StoreOp.Store,
                    ClearValue = new(0.1f, 0.1f, 0.1f, 1.0f),
                }
            }
        });

        pass.SetPipeline(pipeline);
        pass.SetBindGroup(0, bindGroup);
        pass.Draw(4, 1, 0, 0);
    }
}