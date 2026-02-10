using System.Runtime.CompilerServices;
using Pollus.ECS;
using Pollus.UI.Layout;

namespace Pollus.UI;

public class UIPlugin : IPlugin
{
    public PluginDependency[] Dependencies => [PluginDependency.From<HierarchyPlugin>()];

    public void Apply(World world)
    {
        // Force ContentSize static constructor before systems access it
        // through Lookup<T>, avoiding circular static initialization.
        RuntimeHelpers.RunClassConstructor(typeof(ContentSize).TypeHandle);

        world.Resources.Add(new UITreeAdapter());

        world.Schedule.AddSystems(CoreStage.PostUpdate,
            UILayoutSystem.SyncTree(),
            UILayoutSystem.ComputeLayout(),
            UILayoutSystem.WriteBack()
        );
    }
}
