namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public static class UI
{
    public static PanelBuilder Panel(Commands commands) => new(commands);

    public static UITextBuilder Text(Commands commands, string text) => new(commands, text);

    public static UITextBuilder Text(Commands commands, string text, Handle font) => new UITextBuilder(commands, text).Font(font);

    public static UIButtonBuilder Button(Commands commands) => new(commands);

    public static UIToggleBuilder Toggle(Commands commands) => new(commands);

    public static UICheckBoxBuilder CheckBox(Commands commands) => new(commands);

    public static UIRadioButtonBuilder RadioButton(Commands commands, int groupId) => new(commands, groupId);

    public static UIRadioGroupBuilder RadioGroup(Commands commands, int groupId) => new(commands, groupId);

    public static UIRadioGroupBuilder RadioGroup(Commands commands, int groupId, Handle font) => new UIRadioGroupBuilder(commands, groupId).Font(font);

    public static UISliderBuilder Slider(Commands commands) => new(commands);

    public static UITextInputBuilder TextInput(Commands commands) => new(commands);

    public static UITextInputBuilder TextInput(Commands commands, Handle font) => new UITextInputBuilder(commands).Font(font);

    public static UINumberInputBuilder NumberInput(Commands commands) => new(commands);

    public static UINumberInputBuilder NumberInput(Commands commands, Handle font) => new UINumberInputBuilder(commands).Font(font);

    public static UIDropdownBuilder Dropdown(Commands commands) => new(commands);

    public static UIDropdownBuilder Dropdown(Commands commands, Handle font) => new UIDropdownBuilder(commands).Font(font);

    public static UIRootBuilder Root(Commands commands, float width, float height) => new(commands, width, height);
}
