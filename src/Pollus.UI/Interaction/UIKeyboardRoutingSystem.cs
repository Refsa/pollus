namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;

[SystemSet]
public partial class UIKeyboardRoutingSystem
{
    [System(nameof(PerformRouting))]
    static readonly SystemBuilderDescriptor RouteKeysDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::FocusNavigation"],
    };

    internal static void PerformRouting(
        CurrentDevice<Keyboard> keyboard,
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
        var modifiers = keyboard.Value switch
        {
            not null => GetModifiers(keyboard.Value),
            null => ModifierKey.None,
        };

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
                    Modifier = modifiers,
                });
            }
            else if (keyEvent.State == ButtonState.JustReleased)
            {
                keyUpWriter.Write(new UIInteractionEvents.UIKeyUpEvent
                {
                    Entity = focused,
                    Key = (int)keyEvent.Button,
                    Modifier = modifiers,
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

    static ModifierKey GetModifiers(Keyboard keyboard)
    {
        var modifiers = ModifierKey.None;
        if (keyboard.Pressed(Key.LeftShift)) modifiers |= ModifierKey.LeftShift;
        if (keyboard.Pressed(Key.RightShift)) modifiers |= ModifierKey.RightShift;
        if (keyboard.Pressed(Key.LeftControl)) modifiers |= ModifierKey.LeftControl;
        if (keyboard.Pressed(Key.RightControl)) modifiers |= ModifierKey.RightControl;
        if (keyboard.Pressed(Key.LeftAlt)) modifiers |= ModifierKey.LeftAlt;
        if (keyboard.Pressed(Key.RightAlt)) modifiers |= ModifierKey.RightAlt;
        if (keyboard.Pressed(Key.LeftMeta)) modifiers |= ModifierKey.LeftMeta;
        if (keyboard.Pressed(Key.RightMeta)) modifiers |= ModifierKey.RightMeta;
        return modifiers;
    }
}
