namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;

public class ImagePlugin : IPlugin
{
    static ImagePlugin()
    {
        AssetsFetch<ImageAsset>.Register();
    }

    public void Apply(World world)
    {
        var assetServer = world.Resources.Get<AssetServer>();
        assetServer.AddLoader<ImageSharpAssetLoader>();
    }
}