namespace Pollus.Engine.UI;

using Pollus.ECS;
using Pollus.UI;

public class UIWidgetPlugin : IPlugin
{
    public PluginDependency[] Dependencies => [
        PluginDependency.From<UIInteractionPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Events.InitEvent<UIToggleEvents.UIToggleEvent>();

        world.Schedule.AddSystems(CoreStage.PostUpdate,
            UIWidgetSystems.ButtonVisual(),
            UIWidgetSystems.Toggle()
        );
    }
}
