using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UICaretVisualTests
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

    static (Entity inputEntity, Entity textEntity) SpawnTextInput(World world)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var textEntity = commands.Spawn(Entity.With(
            new UINode(),
            new ComputedNode { Size = new Vec2f(100, 20), Position = new Vec2f(5, 5) }
        )).Entity;

        var textInput = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UITextInput { TextEntity = textEntity },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(200), Length.Px(30)) } }
        )).Entity;

        commands.AddChild(root, textInput);
        commands.AddChild(textInput, textEntity);
        // First update: layout + caret system spawns entity
        world.Update();
        // Second update: caret entity materialized
        world.Update();
        return (textInput, textEntity);
    }

    [Fact]
    public void SpawnsCaretEntity()
    {
        using var world = CreateWorld();
        var (inputEntity, _) = SpawnTextInput(world);

        var input = world.Store.GetComponent<UITextInput>(inputEntity);
        Assert.False(input.CaretEntity.IsNull, "CaretEntity should be spawned");
        Assert.True(world.Store.HasComponent<ComputedNode>(input.CaretEntity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(input.CaretEntity));
    }

    [Fact]
    public void CaretEntity_IsChildOfInput()
    {
        using var world = CreateWorld();
        var (inputEntity, _) = SpawnTextInput(world);

        var input = world.Store.GetComponent<UITextInput>(inputEntity);
        Assert.True(world.Store.HasComponent<Child>(input.CaretEntity));
        var child = world.Store.GetComponent<Child>(input.CaretEntity);
        Assert.Equal(inputEntity, child.Parent);
    }

    [Fact]
    public void CaretHidden_WhenNotFocused()
    {
        using var world = CreateWorld();
        var (inputEntity, _) = SpawnTextInput(world);

        // Not focused — caret should have zero size
        var input = world.Store.GetComponent<UITextInput>(inputEntity);
        var caretComputed = world.Store.GetComponent<ComputedNode>(input.CaretEntity);

        Assert.Equal(0f, caretComputed.Size.X);
        Assert.Equal(0f, caretComputed.Size.Y);
    }

    [Fact]
    public void CaretPositioned_WhenFocusedAndVisible()
    {
        using var world = CreateWorld();
        var (inputEntity, textEntity) = SpawnTextInput(world);

        // Set focus
        var focusState = world.Resources.Get<UIFocusState>();
        focusState.FocusedEntity = inputEntity;

        // Set caret measurement data
        ref var input = ref world.Store.GetComponent<UITextInput>(inputEntity);
        input.CaretVisible = true;
        input.CaretHeight = 16f;
        input.CaretXOffset = 30f;

        // Run a tick to update caret visual
        world.Update();

        input = world.Store.GetComponent<UITextInput>(inputEntity);
        var caretComputed = world.Store.GetComponent<ComputedNode>(input.CaretEntity);

        // Caret should be visible (non-zero size)
        Assert.Equal(2f, caretComputed.Size.X);
        Assert.Equal(16f, caretComputed.Size.Y);

        // Position should be relative to text entity's position + offset
        var textComputed = world.Store.GetComponent<ComputedNode>(textEntity);
        var expectedX = textComputed.Position.X + 30f;
        var expectedY = textComputed.Position.Y + (textComputed.Size.Y - 16f) * 0.5f;

        Assert.True(System.Math.Abs(caretComputed.Position.X - expectedX) < 1f,
            $"Expected caret X ~{expectedX}, got {caretComputed.Position.X}");
        Assert.True(System.Math.Abs(caretComputed.Position.Y - expectedY) < 1f,
            $"Expected caret Y ~{expectedY}, got {caretComputed.Position.Y}");
    }

    [Fact]
    public void CaretHidden_WhenBlinkOff()
    {
        using var world = CreateWorld();
        var (inputEntity, _) = SpawnTextInput(world);

        // Focus but set caret invisible (blink off)
        var focusState = world.Resources.Get<UIFocusState>();
        focusState.FocusedEntity = inputEntity;

        ref var input = ref world.Store.GetComponent<UITextInput>(inputEntity);
        input.CaretVisible = false;
        input.CaretHeight = 16f;
        input.CaretXOffset = 30f;

        world.Update();

        input = world.Store.GetComponent<UITextInput>(inputEntity);
        var caretComputed = world.Store.GetComponent<ComputedNode>(input.CaretEntity);

        Assert.Equal(0f, caretComputed.Size.X);
        Assert.Equal(0f, caretComputed.Size.Y);
    }
}
