namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Assets;
using Pollus.Utils;

public class MaterialRenderData
{
    public required Handle<GPURenderPipeline> Pipeline { get; init; }
    public required Handle<GPUBindGroup>[] BindGroups { get; init; }
}

public class MaterialRenderDataLoader<TMaterial> : IRenderDataLoader
    where TMaterial : IMaterial, IAsset
{
    public TypeID TargetType => TypeLookup.ID<TMaterial>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var material = assetServer.GetAssets<TMaterial>().Get(handle)
            ?? throw new InvalidOperationException("Material not found");

        var shaderAsset = assetServer.GetAssets<ShaderAsset>().Get(material.ShaderSource)
            ?? throw new InvalidOperationException("Shader asset not found");

        using var shader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = shaderAsset.Name,
            Content = shaderAsset.Source,
        });

        var bindGroupLayouts = material.Bindings.Select((group, groupIndex) =>
            gpuContext.CreateBindGroupLayout(new()
            {
                Label = $"""{TMaterial.Name}_BindGroupLayout_{groupIndex}""",
                Entries = group.Select((binding, bindingIndex) => binding.Layout((uint)bindingIndex)).ToArray(),
            })).ToArray();

        var bindGroups = material.Bindings.Select((group, groupIndex) =>
            gpuContext.CreateBindGroup(new()
            {
                Label = $"""{TMaterial.Name}_BindGroup_{groupIndex}""",
                Layout = bindGroupLayouts[groupIndex],
                Entries = group.Select((binding, bindingIndex) => binding.Binding(renderAssets, gpuContext, assetServer, (uint)bindingIndex)).ToArray(),
            })).ToArray();

        using var pipelineLayout = gpuContext.CreatePipelineLayout(new()
        {
            Label = $"""{TMaterial.Name}_PipelineLayout""",
            Layouts = bindGroupLayouts,
        });
        var pipelineDescriptor = TMaterial.PipelineDescriptor;
        pipelineDescriptor.VertexState = pipelineDescriptor.VertexState with
        {
            ShaderModule = shader,
        };
        pipelineDescriptor.FragmentState = pipelineDescriptor.FragmentState with
        {
            ShaderModule = shader,
            ColorTargets = [
                ColorTargetState.Default with
                {
                    Format = gpuContext.GetSurfaceFormat(),
                    Blend = TMaterial.Blend ?? BlendState.Default,
                }
            ]
        };
        pipelineDescriptor.PipelineLayout = pipelineLayout;
        var pipeline = gpuContext.CreateRenderPipeline(pipelineDescriptor);

        renderAssets.Add(handle, new MaterialRenderData
        {
            Pipeline = renderAssets.Add(pipeline),
            BindGroups = bindGroups.Select(renderAssets.Add).ToArray(),
        });

        foreach (var bindGroupLayout in bindGroupLayouts)
        {
            bindGroupLayout.Dispose();
        }
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var material = renderAssets.Get<MaterialRenderData>(handle);
        renderAssets.Unload(material.Pipeline);
        foreach (var bindGroup in material.BindGroups)
        {
            renderAssets.Unload(bindGroup);
        }
    }
}