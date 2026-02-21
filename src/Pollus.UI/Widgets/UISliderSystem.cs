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
        RunsAfter = ["UILayoutSystem::WriteBack", "UISliderSystem::Update"],
    };

    internal static void PerformUpdate(
        View<UISlider> viewSlider,
        View<ComputedNode, Child> viewNodeTree,
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
            if (!viewSlider.Has<UISlider>(entity)) continue;
            if (!viewNodeTree.Has<ComputedNode>(entity)) continue;

            ref var slider = ref viewSlider.GetTracked<UISlider>(entity);
            ref readonly var computed = ref viewNodeTree.Read<ComputedNode>(entity);
            var absPos = LayoutHelpers.ComputeAbsolutePosition(viewNodeTree, entity);

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
            if (!viewSlider.Has<UISlider>(entity)) continue;
            if (!viewNodeTree.Has<ComputedNode>(entity)) continue;

            ref var slider = ref viewSlider.GetTracked<UISlider>(entity);
            ref readonly var computed = ref viewNodeTree.Read<ComputedNode>(entity);
            var absPos = LayoutHelpers.ComputeAbsolutePosition(viewNodeTree, entity);

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
            if (!viewSlider.Has<UISlider>(entity)) continue;

            var key = (Key)keyEvent.Key;
            ref var slider = ref viewSlider.GetTracked<UISlider>(entity);
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
        View<UISlider, ComputedNode, BorderRadius, BackgroundColor> view)
    {
        foreach (var ev in eValueChanged.Read())
        {
            UpdateVisual(ev.Entity, commands, view);
        }

        foreach (var ev in eSliderReady.Read())
        {
            UpdateVisual(ev.Entity, commands, view);
        }

        static void UpdateVisual(
            Entity entity,
            Commands commands,
            View<UISlider, ComputedNode, BorderRadius, BackgroundColor> view)
        {
            var slider = view.Read<UISlider>(entity);
            var computed = view.Read<ComputedNode>(entity);

            var width = computed.Size.X;
            var height = computed.Size.Y;

            // Compute ratio
            var range = slider.Max - slider.Min;
            var ratio = range > 0 ? Math.Clamp((slider.Value - slider.Min) / range, 0f, 1f) : 0f;

            // Update fill entity
            if (view.Has<ComputedNode>(slider.FillEntity))
            {
                ref var fillComputed = ref view.GetTracked<ComputedNode>(slider.FillEntity);
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
                if (view.Has<BorderRadius>(entity))
                {
                    var parentBr = view.Read<BorderRadius>(entity);
                    if (view.Has<BorderRadius>(slider.FillEntity))
                        view.GetTracked<BorderRadius>(slider.FillEntity) = parentBr;
                    else
                        commands.AddComponent(slider.FillEntity, parentBr);
                }

                // Sync fill color
                if (view.Has<BackgroundColor>(slider.FillEntity))
                    view.GetTracked<BackgroundColor>(slider.FillEntity).Color = slider.FillColor;
            }

            // Update thumb entity
            if (view.Has<ComputedNode>(slider.ThumbEntity))
            {
                ref var thumbComputed = ref view.GetTracked<ComputedNode>(slider.ThumbEntity);
                var d = height * 1.4f;
                thumbComputed.Position = new Vec2f(width * ratio - d * 0.5f, (height - d) * 0.5f);
                thumbComputed.Size = new Vec2f(d, d);

                // Sync thumb color
                if (view.Has<BackgroundColor>(slider.ThumbEntity))
                    view.GetTracked<BackgroundColor>(slider.ThumbEntity).Color = slider.ThumbColor;
            }
        }
    }
}
