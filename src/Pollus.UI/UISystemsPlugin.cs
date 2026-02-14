namespace Pollus.UI;

using System.Runtime.CompilerServices;
using Input;
using Pollus.ECS;
using Pollus.UI.Layout;

public class UISystemsPlugin : IPlugin
{
    public PluginDependency[] Dependencies => [
        PluginDependency.From<InputPlugin>(),
        PluginDependency.From<HierarchyPlugin>(),
        PluginDependency.From<TimePlugin>(),
    ];

    public void Apply(World world)
    {
        RuntimeHelpers.RunClassConstructor(typeof(ContentSize).TypeHandle);

        world.Resources.Add(new UITreeAdapter());

        // Layout
        world.Schedule.AddSystemSet<UILayoutSystem>();

        // Interaction events
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

        // Interaction resources
        world.Resources.Add(new UIHitTestResult());
        world.Resources.Add(new UIFocusState());

        // Interaction systems
        world.Schedule.AddSystemSet<UIInteractionSystem>();
        world.Schedule.AddSystemSet<UIKeyboardRoutingSystem>();

        // Widget events
        world.Events.InitEvent<UIToggleEvents.UIToggleEvent>();
        world.Events.InitEvent<UICheckBoxEvents.UICheckBoxEvent>();
        world.Events.InitEvent<UIRadioButtonEvents.UIRadioButtonEvent>();
        world.Events.InitEvent<UITextInputEvents.UITextInputValueChanged>();
        world.Events.InitEvent<UINumberInputEvents.UINumberInputValueChanged>();
        world.Events.InitEvent<UISliderEvents.UISliderValueChanged>();
        world.Events.InitEvent<UIDropdownEvents.UIDropdownSelectionChanged>();

        // Widget resources
        world.Resources.Add(new UITextBuffers());

        // Widget systems
        world.Schedule.AddSystemSet<UIButtonSystem>();
        world.Schedule.AddSystemSet<UIToggleSystem>();
        world.Schedule.AddSystemSet<UICheckBoxSystem>();
        world.Schedule.AddSystemSet<UIRadioButtonSystem>();
        world.Schedule.AddSystemSet<UITextInputSystem>();
        world.Schedule.AddSystemSet<UICaretSystem>();
        world.Schedule.AddSystemSet<UINumberInputSystem>();
        world.Schedule.AddSystemSet<UISliderSystem>();
        world.Schedule.AddSystemSet<UIDropdownSystem>();
        world.Schedule.AddSystemSet<UIScrollSystem>();
    }
}
