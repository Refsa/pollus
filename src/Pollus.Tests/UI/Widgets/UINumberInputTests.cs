using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UINumberInputTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UISystemsPlugin(), addDependencies: true);
        world.Resources.Add(new CurrentDevice<Mouse>());
        world.Resources.Add(new ButtonInput<MouseButton>());
        world.Resources.Add(new ButtonInput<Key>());
        world.Prepare();
        return world;
    }

    static (Entity numberInput, Entity textInput) SpawnNumberInput(World world, float min = 0, float max = 100, float step = 1, NumberInputType type = NumberInputType.Float)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var textInput = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UITextInput { Filter = type == NumberInputType.Int ? UIInputFilterType.Integer : UIInputFilterType.Decimal },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(200), Length.Px(30)) } }
        )).Entity;

        var numberInput = commands.Spawn(Entity.With(
            new UINode(),
            new UINumberInput { Min = min, Max = max, Step = step, Type = type, TextInputEntity = textInput },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(200), Length.Px(30)) } }
        )).Entity;

        commands.AddChild(root, numberInput);
        commands.AddChild(root, textInput);
        world.Update();
        return (numberInput, textInput);
    }

    [Fact]
    public void Value_ClampsToMinMax()
    {
        using var world = CreateWorld();
        var (numEntity, textEntity) = SpawnNumberInput(world, min: 0, max: 100);
        var textBuffers = world.Resources.Get<UITextBuffers>();
        textBuffers.Set(textEntity, "150");

        // Simulate text change event
        var textChangedWriter = world.Events.GetWriter<UITextInputEvents.UITextInputValueChanged>();
        textChangedWriter.Write(new UITextInputEvents.UITextInputValueChanged { Entity = textEntity });

        var qNumInput = new Query<UINumberInput>(world);
        var viewTextInput = new View<UITextInput>(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UITextInputEvents.UITextInputValueChanged>()!;

        UINumberInputSystem.PerformUpdate(qNumInput, viewTextInput, textBuffers, keyReader, textReader, world.Events);

        var state = world.Store.GetComponent<UINumberInput>(numEntity);
        Assert.Equal(100f, state.Value, 0.01f);
    }

    [Fact]
    public void UpDown_IncrementsDecrementsByStep()
    {
        using var world = CreateWorld();
        var (numEntity, textEntity) = SpawnNumberInput(world, min: 0, max: 100, step: 5);
        var textBuffers = world.Resources.Get<UITextBuffers>();

        // Set initial value
        ref var numInput = ref world.Store.GetComponent<UINumberInput>(numEntity);
        numInput.Value = 50;

        var qNumInput = new Query<UINumberInput>(world);
        var viewTextInput = new View<UITextInput>(world);

        // Arrow up
        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = textEntity, Key = (int)Key.ArrowUp });

        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UITextInputEvents.UITextInputValueChanged>()!;

        UINumberInputSystem.PerformUpdate(qNumInput, viewTextInput, textBuffers, keyReader, textReader, world.Events);

        numInput = world.Store.GetComponent<UINumberInput>(numEntity);
        Assert.Equal(55f, numInput.Value, 0.01f);

        // Arrow down
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = textEntity, Key = (int)Key.ArrowDown });
        UINumberInputSystem.PerformUpdate(qNumInput, viewTextInput, textBuffers, keyReader, textReader, world.Events);

        numInput = world.Store.GetComponent<UINumberInput>(numEntity);
        Assert.Equal(50f, numInput.Value, 0.01f);
    }

    [Fact]
    public void IntegerMode_RejectsDecimalPoint()
    {
        // This is handled by UITextInput filter, test the filter directly
        Assert.False(UITextInputSystem.PassesFilter('.', UIInputFilterType.Integer, "5", 1));
        Assert.True(UITextInputSystem.PassesFilter('3', UIInputFilterType.Integer, "5", 1));
    }

    [Fact]
    public void FloatMode_AcceptsDecimal()
    {
        Assert.True(UITextInputSystem.PassesFilter('.', UIInputFilterType.Decimal, "5", 1));
        Assert.True(UITextInputSystem.PassesFilter('3', UIInputFilterType.Decimal, "5.", 2));
    }

    [Fact]
    public void ValueParsesCorrectlyFromText()
    {
        using var world = CreateWorld();
        var (numEntity, textEntity) = SpawnNumberInput(world, min: -100, max: 100);
        var textBuffers = world.Resources.Get<UITextBuffers>();
        textBuffers.Set(textEntity, "42.5");

        var textChangedWriter = world.Events.GetWriter<UITextInputEvents.UITextInputValueChanged>();
        textChangedWriter.Write(new UITextInputEvents.UITextInputValueChanged { Entity = textEntity });

        var qNumInput = new Query<UINumberInput>(world);
        var viewTextInput = new View<UITextInput>(world);
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var textReader = world.Events.GetReader<UITextInputEvents.UITextInputValueChanged>()!;

        UINumberInputSystem.PerformUpdate(qNumInput, viewTextInput, textBuffers, keyReader, textReader, world.Events);

        var state = world.Store.GetComponent<UINumberInput>(numEntity);
        Assert.Equal(42.5f, state.Value, 0.01f);
    }

    [Fact]
    public void FormatValue_IntegerMode()
    {
        Assert.Equal("42", UINumberInputSystem.FormatValue(42.7f, NumberInputType.Int));
        Assert.Equal("0", UINumberInputSystem.FormatValue(0f, NumberInputType.Int));
    }

    [Fact]
    public void FormatValue_FloatMode()
    {
        var result = UINumberInputSystem.FormatValue(42.5f, NumberInputType.Float);
        Assert.Equal("42.5", result);
    }
}
