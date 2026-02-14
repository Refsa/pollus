namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Graphics.Rendering;

public class ImagePlugin : IPlugin
{
    static ImagePlugin()
    {
        AssetsFetch<Texture2D>.Register();
    }

    public void Apply(World world)
    {
        var assetServer = world.Resources.Get<AssetServer>();
        assetServer.AddLoader<ImageAssetLoader>();
    }
}