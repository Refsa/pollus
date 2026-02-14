namespace Pollus.UI;

using Pollus.ECS;

public class UICaretSystem : ISystemSet
{
    public const string Label = "UICaretSystem::Update";

    public static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Label = new SystemLabel(Label),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UITextInputSystem.Label],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(UpdateDescriptor.Stage, FnSystem.Create(UpdateDescriptor,
            (SystemDelegate<Time, UIFocusState, Query>)Update));
    }

    public static void Update(
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
