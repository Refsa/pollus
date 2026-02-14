namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;

public static class UISliderSystem
{
    public const string Label = "UISliderSystem::Update";

    public static SystemBuilder Create() => FnSystem.Create(
        new(Label) { RunsAfter = [UIInteractionSystem.UpdateStateLabel] },
        static (
            EventReader<UIInteractionEvents.UIClickEvent> clickReader,
            EventReader<UIInteractionEvents.UIDragEvent> dragReader,
            EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
            UIHitTestResult hitResult,
            Events events,
            Query query) =>
        {
            PerformUpdate(query, clickReader, dragReader, keyDownReader, hitResult, events);
        }
    );

    internal static void PerformUpdate(
        Query query,
        EventReader<UIInteractionEvents.UIClickEvent> clickReader,
        EventReader<UIInteractionEvents.UIDragEvent> dragReader,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        UIHitTestResult hitResult,
        Events events)
    {
        var valueChangedWriter = events.GetWriter<UISliderEvents.UISliderValueChanged>();

        // Handle click on slider track
        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UISlider>(entity)) continue;
            if (!query.Has<ComputedNode>(entity)) continue;

            ref var slider = ref query.Get<UISlider>(entity);
            ref readonly var computed = ref query.Get<ComputedNode>(entity);
            var absPos = ComputeAbsolutePosition(query, entity);

            var prevValue = slider.Value;
            slider.Value = ComputeValueFromPosition(hitResult.MousePosition.X, absPos.X, computed.Size.X, slider);

            if (slider.Value != prevValue)
            {
                valueChangedWriter.Write(new UISliderEvents.UISliderValueChanged
                {
                    Entity = entity,
                    Value = slider.Value,
                    PreviousValue = prevValue,
                });
            }
        }

        // Handle drag on slider
        foreach (var drag in dragReader.Read())
        {
            var entity = drag.Entity;
            if (!query.Has<UISlider>(entity)) continue;
            if (!query.Has<ComputedNode>(entity)) continue;

            ref var slider = ref query.Get<UISlider>(entity);
            ref readonly var computed = ref query.Get<ComputedNode>(entity);
            var absPos = ComputeAbsolutePosition(query, entity);

            var prevValue = slider.Value;
            slider.Value = ComputeValueFromPosition(drag.PositionX, absPos.X, computed.Size.X, slider);

            if (slider.Value != prevValue)
            {
                valueChangedWriter.Write(new UISliderEvents.UISliderValueChanged
                {
                    Entity = entity,
                    Value = slider.Value,
                    PreviousValue = prevValue,
                });
            }
        }

        // Handle keyboard (left/right arrow)
        foreach (var keyEvent in keyDownReader.Read())
        {
            var entity = keyEvent.Entity;
            if (!query.Has<UISlider>(entity)) continue;

            var key = (Key)keyEvent.Key;
            ref var slider = ref query.Get<UISlider>(entity);
            var prevValue = slider.Value;

            switch (key)
            {
                case Key.ArrowLeft:
                    {
                        var step = slider.Step > 0 ? slider.Step : (slider.Max - slider.Min) * 0.01f;
                        slider.Value = Math.Clamp(slider.Value - step, slider.Min, slider.Max);
                    }
                    break;
                case Key.ArrowRight:
                    {
                        var step = slider.Step > 0 ? slider.Step : (slider.Max - slider.Min) * 0.01f;
                        slider.Value = Math.Clamp(slider.Value + step, slider.Min, slider.Max);
                    }
                    break;
                case Key.Home:
                    slider.Value = slider.Min;
                    break;
                case Key.End:
                    slider.Value = slider.Max;
                    break;
            }

            if (slider.Value != prevValue)
            {
                valueChangedWriter.Write(new UISliderEvents.UISliderValueChanged
                {
                    Entity = entity,
                    Value = slider.Value,
                    PreviousValue = prevValue,
                });
            }
        }
    }

    internal static Vec2f ComputeAbsolutePosition(Query query, Entity entity)
    {
        var pos = Vec2f.Zero;
        var current = entity;
        while (!current.IsNull)
        {
            if (query.Has<ComputedNode>(current))
                pos += query.Get<ComputedNode>(current).Position;

            if (query.Has<Child>(current))
                current = query.Get<Child>(current).Parent;
            else
                break;
        }
        return pos;
    }

    internal static float ComputeValueFromPosition(float mouseX, float trackAbsX, float trackWidth, in UISlider slider)
    {
        if (trackWidth <= 0) return slider.Min;

        var relativeX = mouseX - trackAbsX;
        var ratio = Math.Clamp(relativeX / trackWidth, 0f, 1f);
        var value = slider.Min + ratio * (slider.Max - slider.Min);

        // Snap to step
        if (slider.Step > 0)
        {
            value = MathF.Round(value / slider.Step) * slider.Step;
        }

        return Math.Clamp(value, slider.Min, slider.Max);
    }
}
