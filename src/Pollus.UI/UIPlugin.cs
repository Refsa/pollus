using Pollus.ECS;
using Pollus.UI.Layout;

namespace Pollus.UI;

public class UIPlugin : IPlugin
{
    public PluginDependency[] Dependencies => [PluginDependency.From<HierarchyPlugin>()];

    public void Apply(World world)
    {
        world.Resources.Add(new UITreeAdapter());

        world.Schedule.AddSystems(CoreStage.PostUpdate,
            UILayoutSystem.SyncTree(),
            UILayoutSystem.ComputeLayout(),
            UILayoutSystem.WriteBack()
        );
    }
}
