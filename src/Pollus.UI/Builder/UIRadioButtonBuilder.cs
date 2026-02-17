namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public class UIRadioButtonBuilder : UINodeBuilder<UIRadioButtonBuilder>
{
    static volatile int groupIdCounter = -1;
    public static int NextGroupId => Interlocked.Increment(ref groupIdCounter);

    UIRadioButton radioButton = new();

    public UIRadioButtonBuilder(Commands commands, int? groupId) : base(commands)
    {
        radioButton.GroupId = groupId ?? NextGroupId;
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

    public override Entity Spawn()
    {
        interactable = true;
        focusable = true;
        backgroundColor ??= new Color();

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            radioButton,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return entity;
    }
}
