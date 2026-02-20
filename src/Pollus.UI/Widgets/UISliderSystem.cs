namespace Pollus.UI;

using Layout;
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

    [System(nameof(SetupSlider))]
    static readonly SystemBuilderDescriptor SetupSliderDescriptor = new()
    {
        Stage = CoreStage.Update,
    };

    [System(nameof(UpdateVisuals))]
    static readonly SystemBuilderDescriptor UpdateVisualsDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UILayoutSystem::WriteBack", "UISliderSystem::PerformUpdate"],
    };

    internal static void PerformUpdate(
        Query<UISlider> qSlider,
        Query<ComputedNode, Child> qNodeTree,
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
            if (!qSlider.Has<UISlider>(entity)) continue;
            if (!qNodeTree.Has<ComputedNode>(entity)) continue;

            ref var slider = ref qSlider.GetTracked<UISlider>(entity);
            ref readonly var computed = ref qNodeTree.Get<ComputedNode>(entity);
            var absPos = LayoutHelpers.ComputeAbsolutePosition(qNodeTree, entity);

            var prevValue = slider.Value;
            slider.Value = LayoutHelpers.ComputeValueFromPosition(hitResult.MousePosition.X, absPos.X, computed.Size.X, slider);

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
            if (!qSlider.Has<UISlider>(entity)) continue;
            if (!qNodeTree.Has<ComputedNode>(entity)) continue;

            ref var slider = ref qSlider.GetTracked<UISlider>(entity);
            ref readonly var computed = ref qNodeTree.Get<ComputedNode>(entity);
            var absPos = LayoutHelpers.ComputeAbsolutePosition(qNodeTree, entity);

            var prevValue = slider.Value;
            slider.Value = LayoutHelpers.ComputeValueFromPosition(drag.PositionX, absPos.X, computed.Size.X, slider);

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
            if (!qSlider.Has<UISlider>(entity)) continue;

            var key = (Key)keyEvent.Key;
            ref var slider = ref qSlider.GetTracked<UISlider>(entity);
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

    internal static void SetupSlider(
        Commands commands,
        Query<UISlider>.Filter<Added<UISlider>> qNew,
        EventWriter<UISliderEvents.UISliderReady> eReadyWriter)
    {
        foreach (var row in qNew)
        {
            ref var slider = ref row.Component0;
            if (slider.FillEntity.IsNull)
            {
                var fill = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = slider.FillColor }
                )).Entity;
                commands.AddChild(row.Entity, fill);
                slider.FillEntity = fill;
            }

            if (slider.ThumbEntity.IsNull)
            {
                var thumb = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = slider.ThumbColor },
                    new UIShape { Type = UIShapeType.Circle }
                )).Entity;
                commands.AddChild(row.Entity, thumb);
                slider.ThumbEntity = thumb;
            }

            eReadyWriter.Write(new() { Entity = row.Entity });
        }
    }

    internal static void UpdateVisuals(
        Commands commands,
        EventReader<UISliderEvents.UISliderValueChanged> eValueChanged,
        EventReader<UISliderEvents.UISliderReady> eSliderReady,
        Query<ComputedNode, BorderRadius, BackgroundColor> qFill,
        Query<ComputedNode, BackgroundColor> qKnob,
        Query<UISlider, ComputedNode> qSliders)
    {
        foreach (var ev in eValueChanged.Read())
        {
            UpdateVisual(ev.Entity, commands, qFill, qKnob, qSliders);
        }

        foreach (var ev in eSliderReady.Read())
        {
            UpdateVisual(ev.Entity, commands, qFill, qKnob, qSliders);
        }

        static void UpdateVisual(
            Entity entity,
            Commands commands,
            Query<ComputedNode, BorderRadius, BackgroundColor> qFill,
            Query<ComputedNode, BackgroundColor> qKnob,
            Query<UISlider, ComputedNode> qSliders)
        {
            var slider = qSliders.Get<UISlider>(entity);
            var computed = qSliders.Get<ComputedNode>(entity);

            var width = computed.Size.X;
            var height = computed.Size.Y;

            // Compute ratio
            var range = slider.Max - slider.Min;
            var ratio = range > 0 ? Math.Clamp((slider.Value - slider.Min) / range, 0f, 1f) : 0f;

            // Update fill entity
            if (qFill.Has<ComputedNode>(slider.FillEntity))
            {
                ref var fillComputed = ref qFill.GetTracked<ComputedNode>(slider.FillEntity);
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
                if (qFill.Has<BorderRadius>(entity))
                {
                    var parentBr = qFill.Get<BorderRadius>(entity);
                    if (qFill.Has<BorderRadius>(slider.FillEntity))
                        qFill.GetTracked<BorderRadius>(slider.FillEntity) = parentBr;
                    else
                        commands.AddComponent(slider.FillEntity, parentBr);
                }

                // Sync fill color
                if (qFill.Has<BackgroundColor>(slider.FillEntity))
                    qFill.GetTracked<BackgroundColor>(slider.FillEntity).Color = slider.FillColor;
            }

            // Update thumb entity
            if (qKnob.Has<ComputedNode>(slider.ThumbEntity))
            {
                ref var thumbComputed = ref qKnob.GetTracked<ComputedNode>(slider.ThumbEntity);
                var d = height * 1.4f;
                thumbComputed.Position = new Vec2f(width * ratio - d * 0.5f, (height - d) * 0.5f);
                thumbComputed.Size = new Vec2f(d, d);

                // Sync thumb color
                if (qKnob.Has<BackgroundColor>(slider.ThumbEntity))
                    qKnob.GetTracked<BackgroundColor>(slider.ThumbEntity).Color = slider.ThumbColor;
            }
        }
    }
}
