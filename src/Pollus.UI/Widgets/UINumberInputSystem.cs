namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;

[SystemSet]
public partial class UINumberInputSystem
{
    [System(nameof(PerformUpdate))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UITextInputSystem::Update"],
    };

    internal static void PerformUpdate(
        Query<UINumberInput> qNumInput,
        View<UITextInput> viewTextInput,
        UITextBuffers textBuffers,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        EventReader<UITextInputEvents.UITextInputValueChanged> textChangedReader,
        Events events)
    {
        var valueChangedWriter = events.GetWriter<UINumberInputEvents.UINumberInputValueChanged>();

        foreach (var textEvent in textChangedReader.Read())
        {
            foreach (var row in qNumInput)
            {
                ref var numInput = ref qNumInput.GetTracked<UINumberInput>(row.Entity);
                if (numInput.TextInputEntity != textEvent.Entity) continue;

                var text = textBuffers.Get(textEvent.Entity);
                var prevValue = numInput.Value;
                if (float.TryParse(text, out var parsed))
                {
                    numInput.Value = Math.Clamp(parsed, numInput.Min, numInput.Max);
                    if (numInput.Value != prevValue)
                    {
                        valueChangedWriter.Write(new UINumberInputEvents.UINumberInputValueChanged
                        {
                            Entity = row.Entity,
                            Value = numInput.Value,
                            PreviousValue = prevValue,
                        });
                    }
                }
            }
        }

        // Handle arrow key increment/decrement
        foreach (var keyEvent in keyDownReader.Read())
        {
            var key = (Key)keyEvent.Key;
            if (key != Key.ArrowUp && key != Key.ArrowDown) continue;

            foreach (var row in qNumInput)
            {
                ref var numInput = ref qNumInput.GetTracked<UINumberInput>(row.Entity);
                if (numInput.TextInputEntity != keyEvent.Entity) continue;

                var prevValue = numInput.Value;
                var delta = key == Key.ArrowUp ? numInput.Step : -numInput.Step;
                numInput.Value = Math.Clamp(numInput.Value + delta, numInput.Min, numInput.Max);

                if (numInput.Value != prevValue)
                {
                    // Update linked TextInput text
                    var formatted = FormatValue(numInput.Value, numInput.Type);
                    textBuffers.Set(numInput.TextInputEntity, formatted);

                    if (viewTextInput.Has<UITextInput>(numInput.TextInputEntity))
                    {
                        ref var textInput = ref viewTextInput.GetTracked<UITextInput>(numInput.TextInputEntity);
                        textInput.CursorPosition = formatted.Length;
                    }

                    valueChangedWriter.Write(new UINumberInputEvents.UINumberInputValueChanged
                    {
                        Entity = row.Entity,
                        Value = numInput.Value,
                        PreviousValue = prevValue,
                    });
                }
            }
        }
    }

    public static string FormatValue(float value, NumberInputType type)
    {
        return type == NumberInputType.Int
            ? ((int)value).ToString()
            : value.ToString("G");
    }
}
