namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Input;

[SystemSet]
public partial class UITextInputSystem
{
    [System(nameof(PerformTextInput))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIKeyboardRoutingSystem::RouteKeys"],
    };

    internal static void PerformTextInput(
        Query query,
        UITextBuffers textBuffers,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        EventReader<UIInteractionEvents.UITextInputEvent> textInputReader,
        Events events)
    {
        var valueChangedWriter = events.GetWriter<UITextInputEvents.UITextInputValueChanged>();

        // Handle text input (character insertion)
        foreach (var textEvent in textInputReader.Read())
        {
            var entity = textEvent.Entity;
            if (!query.Has<UITextInput>(entity)) continue;

            ref var input = ref query.Get<UITextInput>(entity);
            var text = textBuffers.Get(entity);
            var inputText = textEvent.Text;

            bool changed = false;
            foreach (var ch in inputText)
            {
                if (PassesFilter(ch, input.Filter, text, input.CursorPosition))
                {
                    text = text.Insert(input.CursorPosition, ch.ToString());
                    input.CursorPosition++;
                    changed = true;
                }
            }

            if (changed)
            {
                textBuffers.Set(entity, text);
                SyncTextEntity(query, ref input, text);
                ResetCaret(ref input);
                valueChangedWriter.Write(new UITextInputEvents.UITextInputValueChanged { Entity = entity });
            }
        }

        // Handle key events (control keys)
        foreach (var keyEvent in keyDownReader.Read())
        {
            var entity = keyEvent.Entity;
            if (!query.Has<UITextInput>(entity)) continue;

            ref var input = ref query.Get<UITextInput>(entity);
            var key = (Key)keyEvent.Key;
            var text = textBuffers.Get(entity);

            switch (key)
            {
                case Key.Backspace:
                    if (input.CursorPosition > 0)
                    {
                        text = text.Remove(input.CursorPosition - 1, 1);
                        input.CursorPosition--;
                        textBuffers.Set(entity, text);
                        SyncTextEntity(query, ref input, text);
                        ResetCaret(ref input);
                        valueChangedWriter.Write(new UITextInputEvents.UITextInputValueChanged { Entity = entity });
                    }
                    break;

                case Key.Delete:
                    if (input.CursorPosition < text.Length)
                    {
                        text = text.Remove(input.CursorPosition, 1);
                        textBuffers.Set(entity, text);
                        SyncTextEntity(query, ref input, text);
                        ResetCaret(ref input);
                        valueChangedWriter.Write(new UITextInputEvents.UITextInputValueChanged { Entity = entity });
                    }
                    break;

                case Key.ArrowLeft:
                    if (input.CursorPosition > 0)
                    {
                        input.CursorPosition--;
                        ResetCaret(ref input);
                    }
                    break;

                case Key.ArrowRight:
                    if (input.CursorPosition < text.Length)
                    {
                        input.CursorPosition++;
                        ResetCaret(ref input);
                    }
                    break;

                case Key.Home:
                    input.CursorPosition = 0;
                    ResetCaret(ref input);
                    break;

                case Key.End:
                    input.CursorPosition = text.Length;
                    ResetCaret(ref input);
                    break;
            }
        }
    }

    static void SyncTextEntity(Query query, ref UITextInput input, string text)
    {
        if (input.TextEntity.IsNull) return;
        if (!query.Has<UIText>(input.TextEntity)) return;
        ref var uiText = ref query.Get<UIText>(input.TextEntity);
        uiText.Text = new NativeUtf8(text);
    }

    static void ResetCaret(ref UITextInput input)
    {
        input.CaretBlinkTimer = 0f;
        input.CaretVisible = true;
    }

    internal static bool PassesFilter(char ch, UIInputFilterType filter, string currentText, int cursorPos)
    {
        return filter switch
        {
            UIInputFilterType.Any => !char.IsControl(ch),
            UIInputFilterType.Integer => ch is >= '0' and <= '9' || (ch == '-' && cursorPos == 0 && !currentText.Contains('-')),
            UIInputFilterType.Decimal => ch is >= '0' and <= '9' || (ch == '-' && cursorPos == 0 && !currentText.Contains('-')) || (ch == '.' && !currentText.Contains('.')),
            UIInputFilterType.Alphanumeric => char.IsLetterOrDigit(ch),
            _ => true,
        };
    }
}
