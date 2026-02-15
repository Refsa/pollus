namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public class UISliderBuilder : UINodeBuilder<UISliderBuilder>
{
    UISlider slider = new();

    public UISliderBuilder(Commands commands) : base(commands) { }

    public UISliderBuilder Value(float value)
    {
        slider.Value = value;
        return this;
    }

    public UISliderBuilder Range(float min, float max)
    {
        slider.Min = min;
        slider.Max = max;
        return this;
    }

    public UISliderBuilder Step(float step)
    {
        slider.Step = step;
        return this;
    }

    public UISliderBuilder TrackColor(Color color)
    {
        slider.TrackColor = color;
        return this;
    }

    public UISliderBuilder FillColor(Color color)
    {
        slider.FillColor = color;
        return this;
    }

    public UISliderBuilder ThumbColor(Color color)
    {
        slider.ThumbColor = color;
        return this;
    }

    public new SliderResult Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            slider,
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

        return new SliderResult { Entity = entity };
    }
}
