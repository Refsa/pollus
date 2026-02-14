namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.ECS;
using Pollus.Assets;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IComputeShader
{
    public string Label { get; }
    public Handle<ShaderAsset> Shader { get; }
    public string EntryPoint { get; }
    IBinding[][] Bindings { get; }
}

[Asset]
public partial class ComputeShader : IComputeShader
{
    public required string Label { get; init; }
    public required Handle<ShaderAsset> Shader { get; init; }
    public required string EntryPoint { get; init; }
    public IBinding[][] Bindings { get; init; } = [];
}

public class ComputePlugin<TCompute> : IPlugin
    where TCompute : IComputeShader, IAsset
{
    public const string SetupSystem = $"ComputePlugin<{nameof(TCompute)}>::Setup";

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
        PluginDependency.From<RenderingPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Resources.Get<RenderAssets>().AddLoader(new ComputeRenderDataLoader<TCompute>());

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(SetupSystem,
            static (IWGPUContext gpuContext, AssetServer assetServer, RenderAssets renderAssets, EventReader<AssetEvent<TCompute>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is AssetEventType.Unloaded) continue;

                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                }
            }));
    }
}