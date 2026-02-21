namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;

[SystemSet]
public partial class UICleanupSystem
{
    [System(nameof(CleanupRemovedEntities))]
    static readonly SystemBuilderDescriptor CleanupRemovedEntitiesDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UILayoutSystem::SyncTree"],
    };

    static void CleanupRemovedEntities(
        UITreeAdapter adapter)
    {
        /* foreach (var entity in adapter.LastRemovedEntities)
        {
            
        } */
    }
}
