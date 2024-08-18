namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Engine.Assets;

public class MaterialRenderData : IRenderData
{
    public required GPUShader Shader { get; init; }
    public required GPUBindGroupLayout[] BindGroupLayouts { get; init; }
    public required GPUBindGroup[] BindGroups { get; init; }

    public required GPURenderPipeline Pipeline { get; init; }
    public required GPUPipelineLayout PipelineLayout { get; init; }

    public void Dispose()
    {
        Shader.Dispose();
        foreach (var layout in BindGroupLayouts)
        {
            layout.Dispose();
        }
        foreach (var bindGroup in BindGroups)
        {
            bindGroup.Dispose();
        }
    }
}

public class MaterialRenderDataLoader<TMaterial> : IRenderDataLoader
    where TMaterial : IMaterial
{
    public int TargetType => AssetLookup.ID<TMaterial>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var material = assetServer.GetAssets<TMaterial>().Get(handle)
            ?? throw new InvalidOperationException("Material not found");

        var shaderAsset = assetServer.GetAssets<ShaderAsset>().Get(material.ShaderSource)
            ?? throw new InvalidOperationException("Shader asset not found");

        var shader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = shaderAsset.Name,
            Content = shaderAsset.Source,
        });

        var bindGroupLayouts = material.Bindings.Select((group, groupIndex) =>
            gpuContext.CreateBindGroupLayout(new()
            {
                Label = $"{TMaterial.Name}_BindGroupLayout_{groupIndex}",
                Entries = group.Select((binding, bindingIndex) => binding.Layout((uint)bindingIndex)).ToArray(),
            })).ToArray();

        var bindGroups = material.Bindings.Select((group, groupIndex) =>
            gpuContext.CreateBindGroup(new()
            {
                Label = $"{TMaterial.Name}_BindGroup_{groupIndex}",
                Layout = bindGroupLayouts[groupIndex],
                Entries = group.Select((binding, bindingIndex) => binding.Prepare(renderAssets, gpuContext, assetServer, (uint)bindingIndex)).ToArray(),
            })).ToArray();

        var pipelineLayout = gpuContext.CreatePipelineLayout(new()
        {
            Label = $"{TMaterial.Name}_PipelineLayout",
            Layouts = bindGroupLayouts,
        });
        var pipelineDescriptor = TMaterial.PipelineDescriptor;
        if (pipelineDescriptor.VertexState != null) pipelineDescriptor.VertexState = pipelineDescriptor.VertexState.Value with
        {
            ShaderModule = shader,
        };
        if (pipelineDescriptor.FragmentState != null) pipelineDescriptor.FragmentState = pipelineDescriptor.FragmentState.Value with
        {
            ShaderModule = shader,
            ColorTargets = [
                ColorTargetState.Default with
                {
                    Format = gpuContext.GetSurfaceFormat(),
                }
            ]
        };
        pipelineDescriptor.PipelineLayout = pipelineLayout;
        var pipeline = gpuContext.CreateRenderPipeline(pipelineDescriptor);

        renderAssets.Add(handle, new MaterialRenderData
        {
            Shader = shader,
            BindGroupLayouts = bindGroupLayouts,
            BindGroups = bindGroups,
            Pipeline = pipeline,
            PipelineLayout = pipelineLayout,
        });
    }
}