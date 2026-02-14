namespace Pollus.UI;

using Pollus.ECS;

public static class UICaretSystem
{
    public const string Label = "UICaretSystem::Update";

    public static SystemBuilder Create() => FnSystem.Create(
        new(Label) { RunsAfter = [UITextInputSystem.Label] },
        static (
            Time time,
            UIFocusState focusState,
            Query query) =>
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
    );
}
