namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

[SystemSet]
public partial class UIFocusVisualSystem
{
    [System(nameof(ApplyFocusOutline))]
    static readonly SystemBuilderDescriptor ApplyFocusOutlineDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::FocusNavigation"],
    };

    static void ApplyFocusOutline(
        UIFocusVisualStyle style,
        UIFocusState focusState,
        Query query,
        EventReader<UIInteractionEvents.UIFocusEvent> focusReader,
        EventReader<UIInteractionEvents.UIBlurEvent> blurReader)
    {
        foreach (var blur in blurReader.Read())
        {
            var target = ResolveTarget(query, blur.Entity);
            if (!query.Has<Outline>(target)) continue;

            ref var outline = ref query.Get<Outline>(target);
            outline.Width = 0f;
            outline.Color = Color.TRANSPARENT;
        }

        foreach (var focus in focusReader.Read())
        {
            var entity = focus.Entity;

            if (style.KeyboardOnly && focusState.FocusSource != FocusSource.Keyboard) continue;

            Color color = style.Color;
            float width = style.Width;
            float offset = style.Offset;

            if (query.Has<UIFocusVisual>(entity))
            {
                ref readonly var focusVisual = ref query.Get<UIFocusVisual>(entity);
                if (focusVisual.Disabled) continue;

                if (focusVisual.HasColor) color = focusVisual.Color;
                if (focusVisual.HasWidth) width = focusVisual.Width;
                if (focusVisual.HasOffset) offset = focusVisual.Offset;
            }

            var target = ResolveTarget(query, entity);
            if (!query.Has<Outline>(target)) continue;

            ref var outline = ref query.Get<Outline>(target);
            outline.Color = color;
            outline.Width = width;
            outline.Offset = offset;
        }
    }

    static Entity ResolveTarget(Query query, Entity entity)
    {
        if (query.Has<UIFocusVisual>(entity))
        {
            ref readonly var focusVisual = ref query.Get<UIFocusVisual>(entity);
            if (!focusVisual.Target.IsNull)
                return focusVisual.Target;
        }
        return entity;
    }
}
