namespace Pollus.Engine.UI;

using ECS;
using Input;
using Pollus.UI;
using Rendering;

public class UIPlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<InputPlugin>(),
        PluginDependency.From<UIRenderPlugin>(),
        PluginDependency.From<UITextPlugin>(),
        PluginDependency.From<UISystemsPlugin>(),
    ];

    public void Apply(World world)
    {
    }
}