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
        interactable = true;
        backgroundColor ??= new Color();

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            slider,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return new SliderResult { Entity = entity };
    }
}
