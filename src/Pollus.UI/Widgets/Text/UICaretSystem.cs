namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

[SystemSet]
public partial class UICaretSystem
{
    [System(nameof(Update))]
    public static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UITextInputSystem::PerformTextInput"],
    };

    static void Update(
        Time time,
        UIFocusState focusState,
        Query<UITextInput> qInputs)
    {
        var focused = focusState.FocusedEntity;
        if (focused.IsNull) return;
        if (!qInputs.Has<UITextInput>(focused)) return;

        ref var input = ref qInputs.GetTracked<UITextInput>(focused);

        // Tick blink timer
        input.CaretBlinkTimer += time.DeltaTimeF;
        if (input.CaretBlinkTimer >= input.CaretBlinkRate)
        {
            input.CaretBlinkTimer -= input.CaretBlinkRate;
            input.CaretVisible = !input.CaretVisible;
        }
    }

    [System(nameof(SetupCaret))]
    static readonly SystemBuilderDescriptor SetupCaretDescriptor = new()
    {
        Stage = CoreStage.Update,
    };

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

    [System(nameof(UpdateVisual))]
    public static readonly SystemBuilderDescriptor UpdateVisualDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UICaretSystem::Update"],
    };

    internal static void UpdateVisual(
        UIFocusState focusState,
        Query<ComputedNode> qComputed,
        Query<UITextInput, ComputedNode>.Filter<All<UIInteraction>> qInputs)
    {
        var focused = focusState.FocusedEntity;

        foreach (var row in qInputs)
        {
            var entity = row.Entity;
            ref readonly var input = ref row.Component0;

            if (input.CaretEntity.IsNull) continue;
            if (!qComputed.Has<ComputedNode>(input.CaretEntity)) continue;

            ref var caretComputed = ref qComputed.GetTracked<ComputedNode>(input.CaretEntity);

            bool isFocused = !focused.IsNull && focused == entity;
            if (isFocused && input.CaretVisible && input.CaretHeight > 0
                && !input.TextEntity.IsNull
                && qComputed.Has<ComputedNode>(input.TextEntity))
            {
                ref readonly var textComputed = ref qComputed.Get<ComputedNode>(input.TextEntity);
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
