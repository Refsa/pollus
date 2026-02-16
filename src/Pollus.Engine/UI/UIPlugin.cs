namespace Pollus.Engine.UI;

using ECS;
using Pollus.Graphics.Windowing;
using Pollus.UI;
using Pollus.UI.Layout;
using Rendering;

public class UIPlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<UIRenderPlugin>(),
        PluginDependency.From<UITextPlugin>(),
        PluginDependency.From<UISystemsPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create(
            new("UI::SyncViewportSize")
            {
                RunsBefore = ["UILayoutSystem::SyncTree"],
            },
            static (Local<Size<float>> prevSize, IWindow window, Query<UILayoutRoot, UIAutoResize> roots) =>
            {
                var size = new Size<float>(window.Size.X, window.Size.Y);
                if (prevSize.Value == size) return;
                prevSize.Value = size;

                roots.ForEach(size, static (in sz, ref root, ref _) =>
                {
                    root.Size = sz;
                });
            }));
    }
}
