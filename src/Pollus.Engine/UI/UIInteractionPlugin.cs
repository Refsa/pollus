namespace Pollus.Engine.UI;

using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.UI;

public class UIInteractionPlugin : IPlugin
{
    public PluginDependency[] Dependencies => [
        PluginDependency.From<InputPlugin>(),
        PluginDependency.From<UIPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Events.InitEvent<UIInteractionEvents.UIClickEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIHoverEnterEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIHoverExitEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIPressEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIReleaseEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIFocusEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIBlurEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIKeyDownEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIKeyUpEvent>();
        world.Events.InitEvent<UIInteractionEvents.UITextInputEvent>();
        world.Events.InitEvent<UIInteractionEvents.UIDragEvent>();

        world.Resources.Add(new UIHitTestResult());
        world.Resources.Add(new UIFocusState());

        world.Schedule.AddSystems(CoreStage.PostUpdate,
            UIInteractionSystem.HitTest(),
            UIInteractionSystem.UpdateState(),
            UIInteractionSystem.FocusNavigation(),
            UIKeyboardRoutingSystem.Create()
        );
    }
}
