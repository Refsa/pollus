namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IComputeShader
{
    public string Label { get; }
    public Handle<ShaderAsset> Shader { get; }
    public string EntryPoint { get; }
    BindGroupLayoutEntry[][]? Bindings { get; }
}

public class ComputeShader : IComputeShader
{
    public required string Label { get; init; }
    public required Handle<ShaderAsset> Shader { get; init; }
    public required string EntryPoint { get; init; }
    public BindGroupLayoutEntry[][]? Bindings { get; init; }
}

public class ComputeRenderData
{
    public required Handle<GPUComputePipeline> Pipeline { get; init; }
    public required GPUBindGroupLayout[] BindGroupLayouts { get; init; }
}

public class ComputeRenderDataLoader<TCompute> : IRenderDataLoader
    where TCompute : IComputeShader
{
    public int TargetType => TypeLookup.ID<TCompute>();

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

        var bindGroupLayouts = new GPUBindGroupLayout[compute.Bindings?.Length ?? 0];
        GPUPipelineLayout? pipelineLayout = null;
        if (compute.Bindings != null)
        {
            for (int g = 0; g < compute.Bindings.Length; g++)
            {
                var group = compute.Bindings[g];
                bindGroupLayouts[g] = gpuContext.CreateBindGroupLayout(new()
                {
                    Label = $"""{compute.Label}_BindGroupLayout_{g}""",
                    Entries = group,
                });
            }

            pipelineLayout = gpuContext.CreatePipelineLayout(new()
            {
                Label = $"""{compute.Label}_PipelineLayout""",
                Layouts = bindGroupLayouts,
            });
        }

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
            BindGroupLayouts = bindGroupLayouts,
        };
        renderAssets.Add(handle, output);

        pipelineLayout?.Dispose();
    }
}

public class ComputePlugin<TCompute> : IPlugin
    where TCompute : IComputeShader
{
    public const string SetupSystem = $"ComputePlugin<{nameof(TCompute)}>::Setup";

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new ComputeRenderDataLoader<TCompute>());

        world.Schedule.AddSystems(CoreStage.PreRender, SystemBuilder.FnSystem(SetupSystem,
        static (IWGPUContext gpuContext, AssetServer assetServer, Assets<TCompute> computeShaders, RenderAssets renderAssets) =>
        {
            foreach (var info in computeShaders.AssetInfos)
            {
                renderAssets.Prepare(gpuContext, assetServer, info.Handle);
            }
        }));
    }
}