namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;

public class UIKeyboardRoutingSystem : ISystemSet
{
    public const string Label = "UIKeyboardRoutingSystem::RouteKeys";

    public static readonly SystemBuilderDescriptor RouteKeysDescriptor = new()
    {
        Label = new SystemLabel(Label),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UIInteractionSystem.FocusNavigationLabel],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(RouteKeysDescriptor.Stage, FnSystem.Create(RouteKeysDescriptor,
            (SystemDelegate<EventReader<ButtonEvent<Key>>, EventReader<TextInputEvent>, UIFocusState, Events>)RouteKeys));
    }

    public static void RouteKeys(
        EventReader<ButtonEvent<Key>> keyReader,
        EventReader<TextInputEvent> textReader,
        UIFocusState focusState,
        Events events)
    {
        PerformRouting(keyReader, textReader, focusState, events);
    }

    internal static void PerformRouting(
        EventReader<ButtonEvent<Key>> keyReader,
        EventReader<TextInputEvent> textReader,
        UIFocusState focusState,
        Events events)
    {
        if (focusState.FocusedEntity.IsNull) return;

        var focused = focusState.FocusedEntity;
        var keyDownWriter = events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        var keyUpWriter = events.GetWriter<UIInteractionEvents.UIKeyUpEvent>();
        var textWriter = events.GetWriter<UIInteractionEvents.UITextInputEvent>();

        foreach (var keyEvent in keyReader.Read())
        {
            // Don't route Tab - consumed by focus navigation
            if (keyEvent.Button == Key.Tab) continue;

            if (keyEvent.State == ButtonState.JustPressed)
            {
                keyDownWriter.Write(new UIInteractionEvents.UIKeyDownEvent
                {
                    Entity = focused,
                    Key = (int)keyEvent.Button,
                });
            }
            else if (keyEvent.State == ButtonState.JustReleased)
            {
                keyUpWriter.Write(new UIInteractionEvents.UIKeyUpEvent
                {
                    Entity = focused,
                    Key = (int)keyEvent.Button,
                });
            }
        }

        foreach (var textEvent in textReader.Read())
        {
            textWriter.Write(new UIInteractionEvents.UITextInputEvent
            {
                Entity = focused,
                Text = textEvent.Text,
            });
        }
    }
}
