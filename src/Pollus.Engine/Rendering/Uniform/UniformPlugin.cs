namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class UniformPlugin<TUniform, TExtractParam> : IPlugin
    where TUniform : unmanaged, IShaderType
    where TExtractParam : ISystemParam
{
    public delegate void ExtractDelegate(in TExtractParam param, ref TUniform uniform);

    public PluginDependency[] Dependencies => [
        PluginDependency.From<RenderingPlugin>(),
    ];

    public required ExtractDelegate Extract { get; init; }

    public void Apply(World world)
    {
        world.Resources.Get<AssetServer>().GetAssets<Uniform<TUniform>>().Add(new Uniform<TUniform>());
        world.Resources.Get<RenderAssets>().AddLoader(new UniformRenderDataLoader<TUniform>());

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(
            new($"{typeof(TUniform).Name}::UpdateSystem")
            {
                Locals = [Local.From(Extract)]
            },
            static (Local<ExtractDelegate> extract, Assets<Uniform<TUniform>> uniformAssets, TExtractParam extractParams) =>
            {
                var handle = new Handle<Uniform<TUniform>>(0);
                var uniformAsset = uniformAssets.Get(handle)!;
                extract.Value(extractParams, ref uniformAsset.Data);
            }
        ));

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(
            $"{typeof(TUniform).Name}::PrepareSystem",
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, Assets<Uniform<TUniform>> uniformAssets) =>
            {
                var handle = new Handle<Uniform<TUniform>>(0);
                var uniformAsset = uniformAssets.Get(handle)!;
                renderAssets.Prepare(gpuContext, assetServer, handle);
                var renderAsset = renderAssets.Get<UniformRenderData>(handle);
                renderAssets.Get(renderAsset.UniformBuffer).Write(uniformAsset.Data, 0);
            }
        ));
    }
}