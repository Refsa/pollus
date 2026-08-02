namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

[SystemSet]
public partial class UICaretSystem
{
    public const string UpdateLabel = "UICaretSystem::Update";
    public const string UpdateVisualLabel = "UICaretSystem::UpdateVisual";

    [System(CoreStage.PostUpdate, RunsAfter = ["UITextInputSystem::PerformTextInput"])]
    static void Update(
        Time time,
        UIFocusState focusState,
        View<UITextInput> viewInputs)
    {
        var focused = focusState.FocusedEntity;
        if (focused.IsNull) return;
        if (!viewInputs.Has<UITextInput>(focused)) return;

        ref var input = ref viewInputs.GetTracked<UITextInput>(focused);

        // Tick blink timer
        input.CaretBlinkTimer += time.DeltaTimeF;
        if (input.CaretBlinkTimer >= input.CaretBlinkRate)
        {
            input.CaretBlinkTimer -= input.CaretBlinkRate;
            input.CaretVisible = !input.CaretVisible;
        }
    }

    [System(CoreStage.Update)]
    internal static void SetupCaret(
        Commands commands,
        Query<UITextInput>.Filter<(Added<UITextInput>, All<UIInteraction>)> qNew)
    {
        foreach (var row in qNew)
        {
            ref var input = ref row.Component0;
            if (!input.CaretEntity.IsNull) continue;

            var caret = commands.Spawn(Entity.With(
                new ComputedNode(),
                new BackgroundColor { Color = new Color(1f, 1f, 1f, 1f) }
            )).Entity;
            commands.AddChild(row.Entity, caret);
            input.CaretEntity = caret;
        }
    }

    [System(CoreStage.PostUpdate, RunsAfter = [UpdateLabel])]
    internal static void UpdateVisual(
        UIFocusState focusState,
        View<ComputedNode> viewComputed,
        Query<UITextInput, ComputedNode>.Filter<All<UIInteraction>> qInputs)
    {
        var focused = focusState.FocusedEntity;

        foreach (var row in qInputs)
        {
            var entity = row.Entity;
            ref readonly var input = ref row.Component0;

            if (input.CaretEntity.IsNull) continue;
            if (!viewComputed.Has<ComputedNode>(input.CaretEntity)) continue;

            ref var caretComputed = ref viewComputed.GetTracked<ComputedNode>(input.CaretEntity);

            bool isFocused = !focused.IsNull && focused == entity;
            if (isFocused && input.CaretVisible && input.CaretHeight > 0
                && !input.TextEntity.IsNull
                && viewComputed.Has<ComputedNode>(input.TextEntity))
            {
                ref readonly var textComputed = ref viewComputed.Read<ComputedNode>(input.TextEntity);
                var caretW = 2f;
                var caretH = input.CaretHeight;
                var caretX = textComputed.Position.X + input.CaretXOffset;
                var caretY = textComputed.Position.Y + (textComputed.Size.Y - caretH) * 0.5f;

                caretComputed.Position = new Vec2f(caretX, caretY);
                caretComputed.Size = new Vec2f(caretW, caretH);
                continue;
            }

            // Hide caret
            caretComputed.Size = Vec2f.Zero;
        }
    }
}
