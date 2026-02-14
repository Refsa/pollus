namespace Pollus.Engine.UI;

using ECS;
using Rendering;

public class UIPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.AddPlugins(true, [
            new UIRenderPlugin(),
            new UITextPlugin(),
            new Pollus.UI.UIPlugin()
        ]);
    }
}