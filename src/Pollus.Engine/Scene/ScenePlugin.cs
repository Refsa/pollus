namespace Pollus.Engine;

using Pollus.Engine.Assets;
using Pollus.ECS;
using Utils;

public partial struct SceneRoot : IComponent
{
    public required Handle<Scene> Scene;
}

public class ScenePlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => AssetPlugin.Default),
    ];

    public void Apply(World world)
    {
        world.Resources.Get<AssetServer>().AddLoader<SceneAssetLoader>();
    }
}