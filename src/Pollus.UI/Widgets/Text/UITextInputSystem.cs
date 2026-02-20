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
        Query<UITextInput, UIInteraction> qInput,
        Query<UIText> qText,
        UITextBuffers textBuffers,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        EventReader<UIInteractionEvents.UITextInputEvent> textInputReader,
        EventWriter<UITextInputEvents.UITextInputValueChanged> valueChanged)
    {
        // Handle text input (character insertion)
        foreach (var textEvent in textInputReader.Read())
        {
            var entity = textEvent.Entity;
            if (!qInput.Has<UITextInput>(entity)) continue;

            if (qInput.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref qInput.Get<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var input = ref qInput.GetTracked<UITextInput>(entity);
            var text = textBuffers.Get(entity);
            var inputText = textEvent.Text;

            bool changed = false;
            foreach (var ch in inputText)
            {
                if (PassesFilter(ch, input.Filter, text, input.CursorPosition))
                {
                    int pos = input.CursorPosition;
                    text = string.Create(text.Length + 1, (text, pos, ch),
                        static (span, state) =>
                        {
                            state.text.AsSpan(0, state.pos).CopyTo(span);
                            span[state.pos] = state.ch;
                            state.text.AsSpan(state.pos).CopyTo(span[(state.pos + 1)..]);
                        });
                    input.CursorPosition++;
                    changed = true;
                }
            }

            if (changed)
            {
                textBuffers.Set(entity, text);
                SyncTextEntity(qText, ref input, text);
                ResetCaret(ref input);
                valueChanged.Write(new UITextInputEvents.UITextInputValueChanged { Entity = entity });
            }
        }

        // Handle key events (control keys)
        foreach (var keyEvent in keyDownReader.Read())
        {
            var entity = keyEvent.Entity;
            if (!qInput.Has<UITextInput>(entity)) continue;

            ref var input = ref qInput.GetTracked<UITextInput>(entity);
            var key = (Key)keyEvent.Key;
            var text = textBuffers.Get(entity);

            switch (key)
            {
                case Key.Backspace:
                    if (input.CursorPosition > 0)
                    {
                        var toRemove = keyEvent.Modifier.HasFlag(ModifierKey.LeftControl) switch
                        {
                            true => GetPrevOffset(text, input.CursorPosition),
                            false => 1
                        };

                        text = text.Remove(input.CursorPosition - toRemove, toRemove);
                        input.CursorPosition -= toRemove;
                        textBuffers.Set(entity, text);
                        SyncTextEntity(qText, ref input, text);
                        ResetCaret(ref input);
                        valueChanged.Write(new UITextInputEvents.UITextInputValueChanged { Entity = entity });
                    }
                    break;

                case Key.Delete:
                    if (input.CursorPosition < text.Length)
                    {
                        var toRemove = keyEvent.Modifier.HasFlag(ModifierKey.LeftControl) switch
                        {
                            true => GetNextOffset(text, input.CursorPosition),
                            false => 1
                        };

                        text = text.Remove(input.CursorPosition, toRemove);
                        textBuffers.Set(entity, text);
                        SyncTextEntity(qText, ref input, text);
                        ResetCaret(ref input);
                        valueChanged.Write(new UITextInputEvents.UITextInputValueChanged { Entity = entity });
                    }
                    break;

                case Key.ArrowLeft:
                    if (input.CursorPosition > 0)
                    {
                        var toJump = keyEvent.Modifier.HasFlag(ModifierKey.LeftControl) switch
                        {
                            true => GetPrevOffset(text, input.CursorPosition),
                            false => 1
                        };

                        input.CursorPosition -= toJump;
                        ResetCaret(ref input);
                    }
                    break;

                case Key.ArrowRight:
                    if (input.CursorPosition < text.Length)
                    {
                        var toJump = keyEvent.Modifier.HasFlag(ModifierKey.LeftControl) switch
                        {
                            true => GetNextOffset(text, input.CursorPosition),
                            false => 1
                        };

                        input.CursorPosition += toJump;
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

    internal static int GetPrevOffset(string text, int cursorPos)
    {
        var slice = text.AsSpan(0, cursorPos);
        var i = cursorPos - 1;
        while (i > 0 && slice[i - 1] == ' ') i--;
        while (i > 0 && slice[i - 1] != ' ') i--;
        return cursorPos - i;
    }

    internal static int GetNextOffset(string text, int cursorPos)
    {
        var slice = text.AsSpan(cursorPos, text.Length - cursorPos);
        var i = 0;
        while (i < slice.Length && slice[i] == ' ') i++;
        while (i < slice.Length && slice[i] != ' ') i++;
        return i;
    }

    static void SyncTextEntity(Query<UIText> qText, ref UITextInput input, string text)
    {
        if (input.TextEntity.IsNull) return;
        if (!qText.Has<UIText>(input.TextEntity)) return;
        ref var uiText = ref qText.GetTracked<UIText>(input.TextEntity);
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
