using Pollus.Debugging;

namespace Pollus.Graphics.Rendering;

using System.Diagnostics.CodeAnalysis;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class Blit
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

    const string CLEAR_SHADER = """
                                @vertex
                                fn vs_main() -> @builtin(position) vec4f {
                                    return vec4f(0.0, 0.0, 0.0, 0.0);
                                }
                                @fragment
                                fn fs_main() -> @location(0) vec4f {
                                    return vec4f(0.0, 0.0, 0.0, 0.0);
                                }
                                """;

    Handle<GPUSampler>? samplerHandle;
    Handle<GPUBindGroupLayout>? bindGroupLayoutHandle;
    Handle<GPUShader>? blitShaderModuleHandle;
    Handle<GPUShader>? clearShaderModuleHandle;

    Dictionary<int, Handle<GPUBindGroup>> bindGroups = new();
    Dictionary<int, Handle<GPURenderPipeline>> pipelines = new();


    [MemberNotNull(nameof(samplerHandle), nameof(bindGroupLayoutHandle), nameof(blitShaderModuleHandle), nameof(clearShaderModuleHandle))]
    void CreateSharedResources(IWGPUContext gpuContext, IRenderAssets renderAssets)
    {
        if (samplerHandle is null)
        {
            var sampler = gpuContext.CreateSampler(SamplerDescriptor.Default);
            samplerHandle = renderAssets.Add(sampler);
        }

        if (bindGroupLayoutHandle is null)
        {
            var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
            {
                Entries =
                [
                    BindGroupLayoutEntry.TextureEntry(0, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
                    BindGroupLayoutEntry.SamplerEntry(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
                ]
            });
            bindGroupLayoutHandle = renderAssets.Add(bindGroupLayout);
        }

        if (blitShaderModuleHandle is null)
        {
            var blitShaderModule = gpuContext.CreateShaderModule(new()
            {
                Label = """blit-shader""",
                Backend = ShaderBackend.WGSL,
                Content = BLIT_SHADER,
            });
            blitShaderModuleHandle = renderAssets.Add(blitShaderModule);
        }

        if (clearShaderModuleHandle is null)
        {
            var clearShaderModule = gpuContext.CreateShaderModule(new()
            {
                Label = """clear-shader""",
                Backend = ShaderBackend.WGSL,
                Content = CLEAR_SHADER,
            });
            clearShaderModuleHandle = renderAssets.Add(clearShaderModule);
        }
    }

    GPURenderPipeline GetRenderPipeline(IWGPUContext gpuContext, IRenderAssets renderAssets, in GPUTextureView dest, in GPUTextureView? msaaResolve = null)
    {
        var pipelineHash = HashCode.Combine(dest.TextureDescriptor.GetHashCode(), msaaResolve.HasValue ? msaaResolve.Value.TextureDescriptor.GetHashCode() : 0);
        if (pipelines.TryGetValue(pipelineHash, out var pipelineHandle)) return renderAssets.Get(pipelineHandle);

        CreateSharedResources(gpuContext, renderAssets);
        var blitShaderModule = renderAssets.Get(blitShaderModuleHandle.Value);
        var bindGroupLayout = renderAssets.Get(bindGroupLayoutHandle.Value);

        var pipeline = gpuContext.CreateRenderPipeline(new()
        {
            Label = """blit-pipeline""",
            VertexState = new()
            {
                ShaderModule = blitShaderModule,
                EntryPoint = """vs_main""",
            },
            FragmentState = new()
            {
                ShaderModule = blitShaderModule,
                EntryPoint = """fs_main""",
                ColorTargets =
                [
                    ColorTargetState.Default with
                    {
                        Format = dest.Descriptor.Format,
                    }
                ]
            },
            MultisampleState = MultisampleState.Default with
            {
                Count = msaaResolve.HasValue ? msaaResolve.Value.TextureDescriptor.SampleCount : 1,
            },
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

        pipelineHandle = renderAssets.Add(pipeline);
        pipelines.Add(pipelineHash, pipelineHandle);
        return pipeline;
    }

    GPURenderPipeline GetClearRenderPipeline(IWGPUContext gpuContext, IRenderAssets renderAssets, TextureFormat targetFormat)
    {
        var pipelineHash = targetFormat.GetHashCode();
        if (pipelines.TryGetValue(pipelineHash, out var pipelineHandle)) return renderAssets.Get(pipelineHandle);

        CreateSharedResources(gpuContext, renderAssets);
        var clearShaderModule = renderAssets.Get(clearShaderModuleHandle.Value);

        var pipeline = gpuContext.CreateRenderPipeline(new()
        {
            Label = """clear-pipeline""",
            VertexState = new()
            {
                ShaderModule = clearShaderModule,
                EntryPoint = """vs_main""",
            },
            FragmentState = new()
            {
                ShaderModule = clearShaderModule,
                EntryPoint = """fs_main""",
                ColorTargets = [ColorTargetState.Default with { Format = targetFormat }]
            },
            MultisampleState = MultisampleState.Default,
            PrimitiveState = PrimitiveState.Default,
        });

        pipelineHandle = renderAssets.Add(pipeline);
        pipelines.Add(pipelineHash, pipelineHandle);
        return pipeline;
    }

    GPUBindGroup GetBindGroup(IWGPUContext gpuContext, IRenderAssets renderAssets, in GPUTextureView source)
    {
        var bindGroupHash = source.Descriptor.GetHashCode();
        if (bindGroups.TryGetValue(bindGroupHash, out var bindGroupHandle)) return renderAssets.Get(bindGroupHandle);

        CreateSharedResources(gpuContext, renderAssets);
        var bindGroupLayout = renderAssets.Get(bindGroupLayoutHandle.Value);
        var sampler = renderAssets.Get(samplerHandle.Value);

        var bindGroup = gpuContext.CreateBindGroup(new()
        {
            Layout = bindGroupLayout,
            Entries = [BindGroupEntry.TextureEntry(0, source), BindGroupEntry.SamplerEntry(1, sampler)]
        });

        bindGroupHandle = renderAssets.Add(bindGroup);
        bindGroups.Add(bindGroupHash, bindGroupHandle);
        return bindGroup;
    }

    public void BlitTexture(IWGPUContext gpuContext, IRenderAssets renderAssets, in GPUCommandEncoder encoder, in GPUTextureView source, in GPUTextureView dest, Color? clearValue = null, GPUTextureView? msaaResolve = null)
    {
        var pipeline = GetRenderPipeline(gpuContext, renderAssets, dest, msaaResolve);
        var bindGroup = GetBindGroup(gpuContext, renderAssets, source);

        using var pass = encoder.BeginRenderPass(new()
        {
            ColorAttachments = stackalloc RenderPassColorAttachment[]
            {
                new()
                {
                    View = msaaResolve?.Native ?? dest.Native,
                    ResolveTarget = msaaResolve.HasValue ? dest.Native : null,
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

    public void ClearTexture(IWGPUContext gpuContext, IRenderAssets renderAssets, in GPUCommandEncoder encoder, in GPUTextureView dest, Color clearValue)
    {
        var pipeline = GetClearRenderPipeline(gpuContext, renderAssets, dest.TextureDescriptor.Format);

        using var pass = encoder.BeginRenderPass(new()
        {
            ColorAttachments = stackalloc RenderPassColorAttachment[]
            {
                new()
                {
                    View = dest.Native,
                    LoadOp = LoadOp.Clear,
                    StoreOp = StoreOp.Store,
                    ClearValue = clearValue,
                }
            }
        });

        pass.SetPipeline(pipeline);
        pass.Draw(0, 1, 0, 0);
    }
}