namespace Pollus.Engine.Rendering;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Engine.Assets;

public class MaterialRenderData : IRenderData
{
    public required GPUShader Shader { get; init; }
    public required GPUBindGroupLayout[] BindGroupLayouts { get; init; }
    public required GPUBindGroup[] BindGroups { get; init; }

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

    public static MaterialRenderData Create<TMaterial>(IWGPUContext gpuContext, TMaterial material, AssetServer assetServer)
        where TMaterial : IMaterial
    {
        var shaderAsset = assetServer.GetAssets<ShaderAsset>().Get(material.ShaderSource);
        if (shaderAsset is null) throw new InvalidOperationException("Shader asset not found");
        var shader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = TMaterial.Name,
            Content = shaderAsset.Source,
        });

        var bindGroupLayouts = material.Bindings.Select((group, index) =>
            gpuContext.CreateBindGroupLayout(new()
            {
                Label = $"{TMaterial.Name}BindGroupLayout_{index}",
                Entries = group.Select(e => e.Layout with { Binding = (uint)index }).ToArray(),
            })).ToArray();

        var bindGroups = bindGroupLayouts.Select((layout, index) =>
            gpuContext.CreateBindGroup(new()
            {
                Label = $"{TMaterial.Name}BindGroup_{index}",
                Layout = layout,
                Entries = [

                ],
            })).ToArray();

        return new MaterialRenderData
        {
            Shader = shader,
            BindGroupLayouts = bindGroupLayouts,
            BindGroups = bindGroups
        };
    }
}
