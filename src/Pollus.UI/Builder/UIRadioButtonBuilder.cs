namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;

public struct UIRadioButtonBuilder : IUINodeBuilder<UIRadioButtonBuilder>
{
    static volatile int groupIdCounter = -1;
    public static int NextGroupId => Interlocked.Increment(ref groupIdCounter);

    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UIRadioButton radioButton;

    public UIRadioButtonBuilder(Commands commands, int? groupId)
    {
        state = new UINodeBuilderState(commands);
        radioButton = new() { GroupId = groupId ?? NextGroupId };
    }

    public UIRadioButtonBuilder IsSelected(bool value = true)
    {
        radioButton.IsSelected = value;
        return this;
    }

    public UIRadioButtonBuilder SelectedColor(Color color)
    {
        radioButton.SelectedColor = color;
        return this;
    }

    public UIRadioButtonBuilder UnselectedColor(Color color)
    {
        radioButton.UnselectedColor = color;
        return this;
    }

    public Entity Spawn()
    {
        state.interactable = true;
        state.focusable = true;
        state.backgroundColor ??= new Color();

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            radioButton,
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);

        return entity;
    }
}
