namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UINumberInputBuilder : IUINodeBuilder<UINumberInputBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UINumberInput numberInput;
    float fontSize;
    Color textColor;
    Handle font;

    public UINumberInputBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        state.focusable = true;
        numberInput = new();
        fontSize = 16f;
        textColor = Color.WHITE;
        font = Handle.Null;
    }

    public UINumberInputBuilder Value(float value)
    {
        numberInput.Value = value;
        return this;
    }

    public UINumberInputBuilder Range(float min, float max)
    {
        numberInput.Min = min;
        numberInput.Max = max;
        return this;
    }

    public UINumberInputBuilder Step(float step)
    {
        numberInput.Step = step;
        return this;
    }

    public UINumberInputBuilder Type(NumberInputType type)
    {
        numberInput.Type = type;
        return this;
    }

    public UINumberInputBuilder FontSize(float size)
    {
        fontSize = size;
        return this;
    }

    public UINumberInputBuilder TextColor(Color color)
    {
        textColor = color;
        return this;
    }

    public UINumberInputBuilder Font(Handle font)
    {
        this.font = font;
        return this;
    }

    public NumberInputResult Spawn()
    {
        // Create text input child (with its own text child)
        var formatted = UINumberInputSystem.FormatValue(numberInput.Value, numberInput.Type);

        var textEntity = state.commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize(),
            new UIText { Color = textColor, Size = fontSize, Text = new NativeUtf8(formatted) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                }
            }
        )).Entity;

        if (!font.IsNull())
            state.commands.AddComponent(textEntity, new UITextFont { Font = font });

        var textInput = new UITextInput
        {
            TextEntity = textEntity,
            Filter = numberInput.Type == NumberInputType.Int
                ? UIInputFilterType.Integer
                : UIInputFilterType.Decimal,
        };

        var textInputEntity = state.commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = state.focusable },
            textInput,
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                }
            }
        )).Entity;

        state.commands.AddChild(textInputEntity, textEntity);

        numberInput.TextInputEntity = textInputEntity;

        state.backgroundColor ??= new Color();

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            new Outline(),
            numberInput,
            new UIStyle { Value = state.style }
        )).Entity;

        // Redirect focus outline from inner text input to outer container
        state.commands.AddComponent(textInputEntity, new UIFocusVisual { Target = entity });

        state.commands.AddChild(entity, textInputEntity);

        // Defer text buffer initialization
        var capturedTextInputEntity = textInputEntity;
        var capturedFormatted = formatted;
        state.commands.Defer(world =>
        {
            var bufs = world.Resources.Get<UITextBuffers>();
            bufs.Set(capturedTextInputEntity, capturedFormatted);
        });

        state.AddVisualComponents(entity);
        state.SetupHierarchy(entity);

        return new NumberInputResult
        {
            Entity = entity,
            TextInputEntity = textInputEntity,
            TextEntity = textEntity,
        };
    }
}
