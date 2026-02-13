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
        world.Resources.Add(new UITextBuffers());

        world.Events.InitEvent<UIToggleEvents.UIToggleEvent>();
        world.Events.InitEvent<UICheckBoxEvents.UICheckBoxEvent>();
        world.Events.InitEvent<UIRadioButtonEvents.UIRadioButtonEvent>();
        world.Events.InitEvent<UITextInputEvents.UITextInputValueChanged>();
        world.Events.InitEvent<UINumberInputEvents.UINumberInputValueChanged>();
        world.Events.InitEvent<UISliderEvents.UISliderValueChanged>();
        world.Events.InitEvent<UIDropdownEvents.UIDropdownSelectionChanged>();

        world.Schedule.AddSystems(CoreStage.PostUpdate,
            UIWidgetSystems.ButtonVisual(),
            UIWidgetSystems.Toggle(),
            UIWidgetSystems.CheckBox(),
            UIWidgetSystems.RadioButton(),
            UITextInputSystem.Create(),
            UICaretSystem.Create(),
            UINumberInputSystem.Create(),
            UISliderSystem.Create(),
            UIDropdownSystem.Create(),
            UIScrollSystem.Create()
        );
    }
}
