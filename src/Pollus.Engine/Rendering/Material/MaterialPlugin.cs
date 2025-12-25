namespace Pollus.Engine.Rendering;

using Graphics.Rendering;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.WGPU;

public class MaterialPlugin<TMaterial> : IPlugin
    where TMaterial : IMaterial
{
    public static readonly string PrepareSystem = $"MaterialPlugin<{typeof(TMaterial).Name}>::PrepareSystem";
    public static readonly string ReloadSystem = $"MaterialPlugin<{typeof(TMaterial).Name}>::ReloadSystem";

    public static MaterialPlugin<TMaterial> Default => new MaterialPlugin<TMaterial>();

    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<RenderingPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Resources.Get<AssetServer>().InitAsset<TMaterial>();

        world.Resources.Get<RenderAssets>().AddLoader(new MaterialRenderDataLoader<TMaterial>());

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new(PrepareSystem)
            {
                RunsAfter = [RenderingPlugin.BeginFrameSystem],
                RunCriteria = EventRunCriteria<AssetEvent<TMaterial>>.Create,
            },
            static (RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, EventReader<AssetEvent<TMaterial>> assetEvents) =>
            {
                foreach (scoped ref readonly var assetEvent in assetEvents.Read())
                {
                    if (assetEvent.Type is AssetEventType.Removed) continue;

                    renderAssets.Prepare(gpuContext, assetServer, assetEvent.Handle, assetEvent.Type is AssetEventType.Changed);
                }
            }));

        world.Schedule.AddSystems(CoreStage.PreRender, FnSystem.Create(new(ReloadSystem)
            {
                RunsAfter = [PrepareSystem],
            },
            static (RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, Assets<TMaterial> materialAssets) =>
            {
                var shaderAssets = assetServer.GetAssets<ShaderAsset>();

                foreach (var material in materialAssets.AssetInfos)
                {
                    if (material.Status is not AssetStatus.Loaded || material.Asset is null) continue;
                    var shouldReload = false;

                    var materialInfo = renderAssets.GetInfo(material.Handle);
                    if (materialInfo is null) continue;

                    var shaderAsset = shaderAssets.GetInfo(material.Asset.ShaderSource)
                                      ?? throw new InvalidOperationException("Shader asset not found");

                    if ((shaderAsset.LastModified - materialInfo.LastModified).TotalMilliseconds > 300)
                    {
                        shouldReload = true;
                    }

                    if (shouldReload is false)
                    {
                        foreach (var group in material.Asset.Bindings)
                        {
                            if (shouldReload) break;
                            foreach (var binding in group)
                            {
                                if (binding is TextureBinding textureBinding)
                                {
                                    var textureAsset = renderAssets.GetInfo(textureBinding.Image)
                                                       ?? throw new InvalidOperationException("Texture asset not found");
                                    if ((textureAsset.LastModified - materialInfo.LastModified).TotalMilliseconds > 300)
                                    {
                                        shouldReload = true;
                                        break;
                                    }
                                }
                                else if (binding is SamplerBinding samplerBinding)
                                {
                                    var samplerAsset = renderAssets.GetInfo(samplerBinding.Sampler)
                                                       ?? throw new InvalidOperationException("Sampler asset not found");
                                    if ((samplerAsset.LastModified - materialInfo.LastModified).TotalMilliseconds > 300)
                                    {
                                        shouldReload = true;
                                        break;
                                    }
                                }
                                else if (binding is IStorageBufferBinding storageBufferBinding)
                                {
                                    var storageBufferAsset = renderAssets.GetInfo(storageBufferBinding.Buffer)
                                                             ?? throw new InvalidOperationException("Storage buffer asset not found");
                                    if ((storageBufferAsset.LastModified - materialInfo.LastModified).TotalMilliseconds > 300)
                                    {
                                        shouldReload = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (shouldReload)
                    {
                        renderAssets.Prepare(gpuContext, assetServer, material.Handle, true);
                        material.LastModified = DateTime.UtcNow;
                    }
                }
            }
        ));
    }

    public static MaterialPlugin<TMaterial> Create()
    {
        return new MaterialPlugin<TMaterial>();
    }
}