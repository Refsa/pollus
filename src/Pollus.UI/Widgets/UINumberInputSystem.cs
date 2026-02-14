namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;

public class UINumberInputSystem : ISystemSet
{
    public static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Label = new SystemLabel("UINumberInputSystem::Update"),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UITextInputSystem.Label],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(UpdateDescriptor.Stage, FnSystem.Create(UpdateDescriptor,
            (SystemDelegate<EventReader<UIInteractionEvents.UIKeyDownEvent>, EventReader<UITextInputEvents.UITextInputValueChanged>, UITextBuffers, Events, Query>)Update));
    }

    public static void Update(
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        EventReader<UITextInputEvents.UITextInputValueChanged> textChangedReader,
        UITextBuffers textBuffers,
        Events events,
        Query query)
    {
        PerformUpdate(query, textBuffers, keyDownReader, textChangedReader, events);
    }

    internal static void PerformUpdate(
        Query query,
        UITextBuffers textBuffers,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        EventReader<UITextInputEvents.UITextInputValueChanged> textChangedReader,
        Events events)
    {
        var valueChangedWriter = events.GetWriter<UINumberInputEvents.UINumberInputValueChanged>();

        // Handle text changes from the linked TextInput
        foreach (var textEvent in textChangedReader.Read())
        {
            // Find the NumberInput that owns this TextInput
            query.Filtered<All<UINumberInput>>().ForEach((query, textEvent, textBuffers, valueChangedWriter),
                static (in (Query q, UITextInputEvents.UITextInputValueChanged evt, UITextBuffers bufs, EventWriter<UINumberInputEvents.UINumberInputValueChanged> writer) ctx, in Entity entity) =>
                {
                    ref var numInput = ref ctx.q.Get<UINumberInput>(entity);
                    if (numInput.TextInputEntity != ctx.evt.Entity) return;

                    var text = ctx.bufs.Get(ctx.evt.Entity);
                    var prevValue = numInput.Value;
                    if (float.TryParse(text, out var parsed))
                    {
                        numInput.Value = Math.Clamp(parsed, numInput.Min, numInput.Max);
                        if (numInput.Value != prevValue)
                        {
                            ctx.writer.Write(new UINumberInputEvents.UINumberInputValueChanged
                            {
                                Entity = entity,
                                Value = numInput.Value,
                                PreviousValue = prevValue,
                            });
                        }
                    }
                });
        }

        // Handle arrow key increment/decrement
        foreach (var keyEvent in keyDownReader.Read())
        {
            var key = (Key)keyEvent.Key;
            if (key != Key.ArrowUp && key != Key.ArrowDown) continue;

            query.Filtered<All<UINumberInput>>().ForEach((query, keyEvent, key, textBuffers, valueChangedWriter),
                static (in (Query q, UIInteractionEvents.UIKeyDownEvent evt, Key key, UITextBuffers bufs, EventWriter<UINumberInputEvents.UINumberInputValueChanged> writer) ctx, in Entity entity) =>
                {
                    ref var numInput = ref ctx.q.Get<UINumberInput>(entity);
                    if (numInput.TextInputEntity != ctx.evt.Entity) return;

                    var prevValue = numInput.Value;
                    var delta = ctx.key == Key.ArrowUp ? numInput.Step : -numInput.Step;
                    numInput.Value = Math.Clamp(numInput.Value + delta, numInput.Min, numInput.Max);

                    if (numInput.Value != prevValue)
                    {
                        // Update linked TextInput text
                        var formatted = FormatValue(numInput.Value, numInput.Type);
                        ctx.bufs.Set(numInput.TextInputEntity, formatted);

                        if (ctx.q.Has<UITextInput>(numInput.TextInputEntity))
                        {
                            ref var textInput = ref ctx.q.Get<UITextInput>(numInput.TextInputEntity);
                            textInput.CursorPosition = formatted.Length;
                        }

                        ctx.writer.Write(new UINumberInputEvents.UINumberInputValueChanged
                        {
                            Entity = entity,
                            Value = numInput.Value,
                            PreviousValue = prevValue,
                        });
                    }
                });
        }
    }

    internal static string FormatValue(float value, NumberInputType type)
    {
        return type == NumberInputType.Int
            ? ((int)value).ToString()
            : value.ToString("G");
    }
}
