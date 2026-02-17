namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UINumberInputBuilder : UINodeBuilder<UINumberInputBuilder>
{
    UINumberInput numberInput = new();
    float fontSize = 16f;
    Color textColor = Color.WHITE;
    Handle font = Handle.Null;

    public UINumberInputBuilder(Commands commands) : base(commands)
    {
        focusable = true;
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

    public new NumberInputResult Spawn()
    {
        // Create text input child (with its own text child)
        var formatted = UINumberInputSystem.FormatValue(numberInput.Value, numberInput.Type);

        var textEntity = commands.Spawn(Entity.With(
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
            commands.AddComponent(textEntity, new UITextFont { Font = font });

        var textInput = new UITextInput
        {
            TextEntity = textEntity,
            Filter = numberInput.Type == NumberInputType.Int
                ? UIInputFilterType.Integer
                : UIInputFilterType.Decimal,
        };

        var textInputEntity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            textInput,
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                }
            }
        )).Entity;

        commands.AddChild(textInputEntity, textEntity);

        numberInput.TextInputEntity = textInputEntity;

        backgroundColor ??= new Color();

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new Outline(),
            numberInput,
            new UIStyle { Value = style }
        )).Entity;

        // Redirect focus outline from inner text input to outer container
        commands.AddComponent(textInputEntity, new UIFocusVisual { Target = entity });

        commands.AddChild(entity, textInputEntity);

        // Defer text buffer initialization
        var capturedTextInputEntity = textInputEntity;
        var capturedFormatted = formatted;
        commands.Defer(world =>
        {
            var bufs = world.Resources.Get<UITextBuffers>();
            bufs.Set(capturedTextInputEntity, capturedFormatted);
        });

        AddVisualComponents(entity);
        SetupHierarchy(entity);

        return new NumberInputResult
        {
            Entity = entity,
            TextInputEntity = textInputEntity,
            TextEntity = textEntity,
        };
    }
}
