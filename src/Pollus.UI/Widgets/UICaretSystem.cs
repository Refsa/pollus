namespace Pollus.UI;

using Pollus.ECS;

[SystemSet]
public partial class UICaretSystem
{
    [System(nameof(Update))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UITextInputSystem::Update"],
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
}
