namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;

public struct UISliderBuilder : IUINodeBuilder<UISliderBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UISlider slider;

    public UISliderBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        slider = new();
    }

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

    public SliderResult Spawn()
    {
        state.interactable = true;
        state.focusable = true;
        state.backgroundColor ??= new Color();

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            slider,
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);

        return new SliderResult { Entity = entity };
    }
}
