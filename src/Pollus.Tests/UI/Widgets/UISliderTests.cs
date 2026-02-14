using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UISliderTests
{
    static World CreateWorld()
    {
        var world = new World();
        world.AddPlugin(new UIPlugin(), addDependencies: true);
        world.Prepare();
        return world;
    }

    static Entity SpawnSlider(World world, float min = 0, float max = 100, float step = 0)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var slider = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UISlider { Min = min, Max = max, Step = step },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(20)) } }
        )).Entity;

        commands.AddChild(root, slider);
        world.Update();
        return slider;
    }

    [Fact]
    public void ComputeValueFromPosition_ProportionalToTrack()
    {
        // Track at absolute position 0, width 200, range 0-100
        var slider = new UISlider { Min = 0, Max = 100, Step = 0 };

        // Click at midpoint
        var value = UISliderSystem.ComputeValueFromPosition(100f, 0f, 200f, slider);
        Assert.Equal(50f, value, 0.5f);

        // Click at start
        value = UISliderSystem.ComputeValueFromPosition(0f, 0f, 200f, slider);
        Assert.Equal(0f, value, 0.5f);

        // Click at end
        value = UISliderSystem.ComputeValueFromPosition(200f, 0f, 200f, slider);
        Assert.Equal(100f, value, 0.5f);
    }

    [Fact]
    public void Value_ClampsToMinMax()
    {
        var slider = new UISlider { Min = 0, Max = 100, Step = 0 };

        // Beyond right edge
        var value = UISliderSystem.ComputeValueFromPosition(300f, 0f, 200f, slider);
        Assert.Equal(100f, value, 0.5f);

        // Beyond left edge
        value = UISliderSystem.ComputeValueFromPosition(-50f, 0f, 200f, slider);
        Assert.Equal(0f, value, 0.5f);
    }

    [Fact]
    public void StepSnapping_Works()
    {
        var slider = new UISlider { Min = 0, Max = 100, Step = 10 };

        // Click at 33% → should snap to 30
        var value = UISliderSystem.ComputeValueFromPosition(66f, 0f, 200f, slider);
        Assert.Equal(30f, value, 5f);

        // Click at 75% → should snap to 70 or 80
        value = UISliderSystem.ComputeValueFromPosition(150f, 0f, 200f, slider);
        Assert.True(value is 70f or 80f, $"Expected 70 or 80, got {value}");
    }

    [Fact]
    public void ComputeValueFromPosition_WithOffset_UsesAbsolutePosition()
    {
        // Slider track starts at absolute X=300, width 200, range 0-100
        var slider = new UISlider { Min = 0, Max = 100, Step = 0 };

        // Click at absolute X=400 → midpoint of the track → should be 50
        var value = UISliderSystem.ComputeValueFromPosition(400f, 300f, 200f, slider);
        Assert.Equal(50f, value, 0.5f);

        // Click at absolute X=300 → start of the track → should be 0
        value = UISliderSystem.ComputeValueFromPosition(300f, 300f, 200f, slider);
        Assert.Equal(0f, value, 0.5f);

        // Click at absolute X=500 → end of the track → should be 100
        value = UISliderSystem.ComputeValueFromPosition(500f, 300f, 200f, slider);
        Assert.Equal(100f, value, 0.5f);
    }

    [Fact]
    public void Click_NestedSlider_UsesAbsolutePosition()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        // Root at (0,0) 800x600
        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(800), Dimension.Px(600)) } }
        )).Entity;

        // Container with padding that offsets children
        var container = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Dimension>(Dimension.Px(800), Dimension.Px(600)),
                    Padding = new Rect<LengthPercentage>(
                        LengthPercentage.Px(100), LengthPercentage.Px(100),
                        LengthPercentage.Px(50), LengthPercentage.Px(50)),
                }
            }
        )).Entity;

        var slider = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UISlider { Min = 0, Max = 100, Step = 0 },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(20)) } }
        )).Entity;

        commands.AddChild(root, container);
        commands.AddChild(container, slider);
        world.Update();

        // Slider should be at absolute X=100 (container's left padding)
        var query = new Query(world);
        var hitResult = world.Resources.Get<UIHitTestResult>();

        // Simulate click at absolute X=200 → midpoint of 200px track starting at X=100 → value should be 50
        hitResult.MousePosition = new Vec2f(200f, 60f);
        var clickWriter = world.Events.GetWriter<UIInteractionEvents.UIClickEvent>();
        clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = slider });

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var dragReader = world.Events.GetReader<UIInteractionEvents.UIDragEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        UISliderSystem.PerformUpdate(query, clickReader, dragReader, keyReader, hitResult, world.Events);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        Assert.Equal(50f, sliderComp.Value, 1f);
    }

    [Fact]
    public void LeftRightArrowKeys_AdjustValue()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, step: 10);

        ref var sliderComp = ref world.Store.GetComponent<UISlider>(slider);
        sliderComp.Value = 50;

        var query = new Query(world);
        var hitResult = world.Resources.Get<UIHitTestResult>();

        // Right arrow
        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = slider, Key = (int)Key.ArrowRight });

        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var dragReader = world.Events.GetReader<UIInteractionEvents.UIDragEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;

        UISliderSystem.PerformUpdate(query, clickReader, dragReader, keyReader, hitResult, world.Events);

        sliderComp = world.Store.GetComponent<UISlider>(slider);
        Assert.Equal(60f, sliderComp.Value, 0.01f);

        // Left arrow
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = slider, Key = (int)Key.ArrowLeft });
        UISliderSystem.PerformUpdate(query, clickReader, dragReader, keyReader, hitResult, world.Events);

        sliderComp = world.Store.GetComponent<UISlider>(slider);
        Assert.Equal(50f, sliderComp.Value, 0.01f);
    }

    [Fact]
    public void UISliderValueChanged_Emitted()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, step: 10);

        ref var sliderComp = ref world.Store.GetComponent<UISlider>(slider);
        sliderComp.Value = 50;

        var keyWriter = world.Events.GetWriter<UIInteractionEvents.UIKeyDownEvent>();
        keyWriter.Write(new UIInteractionEvents.UIKeyDownEvent { Entity = slider, Key = (int)Key.ArrowRight });

        var query = new Query(world);
        var hitResult = world.Resources.Get<UIHitTestResult>();
        var clickReader = world.Events.GetReader<UIInteractionEvents.UIClickEvent>()!;
        var dragReader = world.Events.GetReader<UIInteractionEvents.UIDragEvent>()!;
        var keyReader = world.Events.GetReader<UIInteractionEvents.UIKeyDownEvent>()!;
        var sliderReader = world.Events.GetReader<UISliderEvents.UISliderValueChanged>()!;

        UISliderSystem.PerformUpdate(query, clickReader, dragReader, keyReader, hitResult, world.Events);

        var events = sliderReader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(slider, events[0].Entity);
        Assert.Equal(60f, events[0].Value, 0.01f);
        Assert.Equal(50f, events[0].PreviousValue, 0.01f);
    }
}
