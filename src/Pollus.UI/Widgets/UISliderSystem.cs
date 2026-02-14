namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;

[SystemSet]
public partial class UISliderSystem
{
    [System(nameof(PerformUpdate))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

    [System(nameof(UpdateVisuals))]
    static readonly SystemBuilderDescriptor UpdateVisualsDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UILayoutSystem::WriteBack", "UISliderSystem::PerformUpdate"],
    };

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

    internal static void UpdateVisuals(
        Commands commands,
        Query query,
        Query<UISlider, ComputedNode> qSliders)
    {
        foreach (var row in qSliders)
        {
            var entity = row.Entity;
            ref var slider = ref row.Component0;
            ref readonly var computed = ref row.Component1;

            var width = computed.Size.X;
            var height = computed.Size.Y;

            bool justSpawned = false;

            // Spawn fill entity if needed
            if (slider.FillEntity.IsNull)
            {
                var fill = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = slider.FillColor }
                )).Entity;
                commands.AddChild(entity, fill);
                slider.FillEntity = fill;
                justSpawned = true;
            }

            // Spawn thumb entity if needed
            if (slider.ThumbEntity.IsNull)
            {
                var thumb = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = slider.ThumbColor },
                    new UIShape { Type = UIShapeType.Circle }
                )).Entity;
                commands.AddChild(entity, thumb);
                slider.ThumbEntity = thumb;
                justSpawned = true;
            }

            // Entities are not materialized until commands flush — skip positioning on spawn frame
            if (justSpawned) continue;

            // Compute ratio
            var range = slider.Max - slider.Min;
            var ratio = range > 0 ? Math.Clamp((slider.Value - slider.Min) / range, 0f, 1f) : 0f;

            // Update fill entity
            if (query.Has<ComputedNode>(slider.FillEntity))
            {
                ref var fillComputed = ref query.Get<ComputedNode>(slider.FillEntity);
                var fillW = width * ratio;
                if (fillW >= 0.5f)
                {
                    fillComputed.Position = Vec2f.Zero;
                    fillComputed.Size = new Vec2f(fillW, height);
                }
                else
                {
                    fillComputed.Size = Vec2f.Zero;
                }

                // Copy parent's border radius
                if (query.Has<BorderRadius>(entity))
                {
                    var parentBr = query.Get<BorderRadius>(entity);
                    if (query.Has<BorderRadius>(slider.FillEntity))
                        query.Get<BorderRadius>(slider.FillEntity) = parentBr;
                    else
                        commands.AddComponent(slider.FillEntity, parentBr);
                }

                // Sync fill color
                if (query.Has<BackgroundColor>(slider.FillEntity))
                {
                    query.Get<BackgroundColor>(slider.FillEntity).Color = slider.FillColor;
                }
            }

            // Update thumb entity
            if (query.Has<ComputedNode>(slider.ThumbEntity))
            {
                ref var thumbComputed = ref query.Get<ComputedNode>(slider.ThumbEntity);
                var d = height * 1.4f;
                thumbComputed.Position = new Vec2f(width * ratio - d * 0.5f, (height - d) * 0.5f);
                thumbComputed.Size = new Vec2f(d, d);

                // Sync thumb color
                if (query.Has<BackgroundColor>(slider.ThumbEntity))
                {
                    query.Get<BackgroundColor>(slider.ThumbEntity).Color = slider.ThumbColor;
                }
            }
        }
    }
}
