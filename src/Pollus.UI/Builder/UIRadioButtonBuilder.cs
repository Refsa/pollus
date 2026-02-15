namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public class UIRadioButtonBuilder : UINodeBuilder<UIRadioButtonBuilder>
{
    UIRadioButton radioButton = new();

    public UIRadioButtonBuilder(Commands commands, int groupId) : base(commands)
    {
        radioButton.GroupId = groupId;
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
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            radioButton,
            backgroundColor.HasValue
                ? new BackgroundColor { Color = backgroundColor.Value }
                : new BackgroundColor(),
            new UIStyle { Value = style }
        )).Entity;

        if (borderColor.HasValue)
            commands.AddComponent(entity, borderColor.Value);

        if (borderRadius.HasValue)
            commands.AddComponent(entity, borderRadius.Value);

        if (boxShadow.HasValue)
            commands.AddComponent(entity, boxShadow.Value);

        SetupHierarchy(entity);

        return entity;
    }
}
