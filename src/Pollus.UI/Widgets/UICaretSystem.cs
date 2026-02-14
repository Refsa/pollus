namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

[SystemSet]
public partial class UICaretSystem
{
    [System(nameof(Update))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UITextInputSystem::PerformTextInput"],
    };

    static void Update(
        Time time,
        UIFocusState focusState,
        Query query)
    {
        var focused = focusState.FocusedEntity;
        if (focused.IsNull) return;
        if (!query.Has<UITextInput>(focused)) return;

        ref var input = ref query.Get<UITextInput>(focused);

        // Tick blink timer
        input.CaretBlinkTimer += time.DeltaTimeF;
        if (input.CaretBlinkTimer >= input.CaretBlinkRate)
        {
            input.CaretBlinkTimer -= input.CaretBlinkRate;
            input.CaretVisible = !input.CaretVisible;
        }
    }

    [System(nameof(UpdateVisual))]
    static readonly SystemBuilderDescriptor UpdateVisualDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UITextPlugin::CaretMeasure", "UICaretSystem::Update"],
    };

    internal static void UpdateVisual(
        Commands commands,
        UIFocusState focusState,
        Query query,
        Query<UITextInput, ComputedNode>.Filter<All<UIInteraction>> qInputs)
    {
        var focused = focusState.FocusedEntity;

        foreach (var row in qInputs)
        {
            var entity = row.Entity;
            ref var input = ref row.Component0;

            // Spawn caret entity if needed
            if (input.CaretEntity.IsNull)
            {
                var caret = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = new Color(1f, 1f, 1f, 1f) }
                )).Entity;
                commands.AddChild(entity, caret);
                input.CaretEntity = caret;
                continue; // Entity not materialized until commands flush
            }

            if (!query.Has<ComputedNode>(input.CaretEntity)) continue;

            ref var caretComputed = ref query.Get<ComputedNode>(input.CaretEntity);

            bool isFocused = !focused.IsNull && focused == entity;
            if (isFocused && input.CaretVisible && input.CaretHeight > 0
                && !input.TextEntity.IsNull)
            {
                var textRef = query.GetEntity(input.TextEntity);
                if (textRef.Has<ComputedNode>())
                {
                    ref readonly var textComputed = ref textRef.Get<ComputedNode>();
                    var caretW = 2f;
                    var caretH = input.CaretHeight;
                    var caretX = textComputed.Position.X + input.CaretXOffset;
                    var caretY = textComputed.Position.Y + (textComputed.Size.Y - caretH) * 0.5f;

                    caretComputed.Position = new Vec2f(caretX, caretY);
                    caretComputed.Size = new Vec2f(caretW, caretH);
                    continue;
                }
            }

            // Hide caret
            caretComputed.Size = Vec2f.Zero;
        }
    }
}
