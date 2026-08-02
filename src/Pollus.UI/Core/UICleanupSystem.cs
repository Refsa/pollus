namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;

[SystemSet]
public partial class UICleanupSystem
{
    [System(CoreStage.PostUpdate, RunsAfter = ["UILayoutSystem::SyncTree"])]
    static void CleanupRemovedEntities(UITreeAdapter adapter)
    {
        /* foreach (var entity in adapter.LastRemovedEntities)
        {
            
        } */
    }
}
