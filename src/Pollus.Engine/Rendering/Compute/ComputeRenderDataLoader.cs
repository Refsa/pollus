namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class ComputeRenderData
{
    public required Handle<GPUComputePipeline> Pipeline { get; init; }
    public required Handle<GPUBindGroup>[] BindGroups { get; init; }
}

public class ComputeRenderDataLoader<TCompute> : IRenderDataLoader
    where TCompute : IComputeShader, IAsset
{
    public TypeID TargetType => TypeLookup.ID<TCompute>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var compute = assetServer.GetAssets<TCompute>().Get(handle)
            ?? throw new InvalidOperationException("Compute shader not found");

        var shaderAsset = assetServer.GetAssets<ShaderAsset>().Get(compute.Shader)
            ?? throw new InvalidOperationException("Shader asset not found");

        using var shader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = shaderAsset.Name,
            Content = shaderAsset.Source,
        });

        var bindGroupLayouts = compute.Bindings.Select((group, groupIndex) =>
            gpuContext.CreateBindGroupLayout(new()
            {
                Label = $"""{compute.Label}_BindGroupLayout_{groupIndex}""",
                Entries = group.Select((binding, bindingIndex) => binding.Layout((uint)bindingIndex)).ToArray(),
            })).ToArray();

        var bindGroups = compute.Bindings.Select((group, groupIndex) =>
            gpuContext.CreateBindGroup(new()
            {
                Label = $"""{compute.Label}_BindGroup_{groupIndex}""",
                Layout = bindGroupLayouts[groupIndex],
                Entries = group.Select((binding, bindingIndex) => binding.Binding(renderAssets, gpuContext, assetServer, (uint)bindingIndex)).ToArray(),
            })).ToArray();

        using var pipelineLayout = gpuContext.CreatePipelineLayout(new()
        {
            Label = $"""{compute.Label}_PipelineLayout""",
            Layouts = bindGroupLayouts,
        });

        var pipeline = gpuContext.CreateComputePipeline(new()
        {
            Label = $"""{compute.Label}_Pipeline""",
            Layout = pipelineLayout,
            Compute = new()
            {
                Shader = shader,
                EntryPoint = compute.EntryPoint,
            },
        });

        var output = new ComputeRenderData()
        {
            Pipeline = renderAssets.Add(pipeline),
            BindGroups = bindGroups.Select(bg => renderAssets.Add(bg)).ToArray(),
        };
        renderAssets.Add(handle, output);

        foreach (var bgl in bindGroupLayouts)
        {
            bgl.Dispose();
        }
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var compute = renderAssets.Get<ComputeRenderData>(handle);
        renderAssets.Unload(compute.Pipeline);
        foreach (var bindGroup in compute.BindGroups)
        {
            renderAssets.Unload(bindGroup);
        }
    }
}
