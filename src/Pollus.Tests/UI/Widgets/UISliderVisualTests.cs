using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Widgets;

public class UISliderVisualTests
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

    static Entity SpawnSlider(World world, float min = 0, float max = 100, float value = 0)
    {
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var slider = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = true },
            new UISlider { Min = min, Max = max, Value = value },
            new UIStyle { Value = LayoutStyle.Default with { Size = new Size<Length>(Length.Px(200), Length.Px(20)) } },
            new BackgroundColor { Color = new Color(0.3f, 0.3f, 0.3f, 1f) }
        )).Entity;

        commands.AddChild(root, slider);
        world.Update();
        return slider;
    }

    [Fact]
    public void SpawnsOverlayEntities()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        Assert.False(sliderComp.FillEntity.IsNull, "FillEntity should be spawned");
        Assert.False(sliderComp.ThumbEntity.IsNull, "ThumbEntity should be spawned");
    }

    [Fact]
    public void FillEntity_HasComputedNodeAndBackgroundColor()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        Assert.True(world.Store.HasComponent<ComputedNode>(sliderComp.FillEntity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(sliderComp.FillEntity));
    }

    [Fact]
    public void ThumbEntity_HasComputedNodeAndBackgroundColorAndCircleShape()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        Assert.True(world.Store.HasComponent<ComputedNode>(sliderComp.ThumbEntity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(sliderComp.ThumbEntity));
        Assert.True(world.Store.HasComponent<UIShape>(sliderComp.ThumbEntity));
        Assert.Equal(UIShapeType.Circle, world.Store.GetComponent<UIShape>(sliderComp.ThumbEntity).Type);
    }

    [Fact]
    public void FillSize_MatchesSliderRatio_AtMidpoint()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, value: 50);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        var fillComputed = world.Store.GetComponent<ComputedNode>(sliderComp.FillEntity);

        // Slider is 200px wide, 50% → fill should be ~100px wide
        Assert.True(fillComputed.Size.X > 99f && fillComputed.Size.X < 101f,
            $"Expected fill width ~100, got {fillComputed.Size.X}");
        Assert.True(fillComputed.Size.Y > 19f && fillComputed.Size.Y < 21f,
            $"Expected fill height ~20, got {fillComputed.Size.Y}");
    }

    [Fact]
    public void FillSize_ZeroWhenValueAtMin()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, value: 0);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        var fillComputed = world.Store.GetComponent<ComputedNode>(sliderComp.FillEntity);

        // ratio=0, fillW=0 which is < 0.5 → size should be zero
        Assert.Equal(0f, fillComputed.Size.X);
        Assert.Equal(0f, fillComputed.Size.Y);
    }

    [Fact]
    public void FillSize_FullWidthWhenValueAtMax()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, value: 100);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        var fillComputed = world.Store.GetComponent<ComputedNode>(sliderComp.FillEntity);

        // ratio=1, fill should be full width (~200px)
        Assert.True(fillComputed.Size.X > 199f && fillComputed.Size.X < 201f,
            $"Expected fill width ~200, got {fillComputed.Size.X}");
    }

    [Fact]
    public void ThumbPosition_AtMidpoint()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, value: 50);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        var thumbComputed = world.Store.GetComponent<ComputedNode>(sliderComp.ThumbEntity);

        // height=20, diameter=28, position.X = 200*0.5 - 28*0.5 = 86
        var d = 20f * 1.4f; // 28
        var expectedX = 200f * 0.5f - d * 0.5f; // 86
        var expectedY = (20f - d) * 0.5f; // -4

        Assert.True(System.Math.Abs(thumbComputed.Position.X - expectedX) < 1f,
            $"Expected thumb X ~{expectedX}, got {thumbComputed.Position.X}");
        Assert.True(System.Math.Abs(thumbComputed.Position.Y - expectedY) < 1f,
            $"Expected thumb Y ~{expectedY}, got {thumbComputed.Position.Y}");
        Assert.True(System.Math.Abs(thumbComputed.Size.X - d) < 0.1f,
            $"Expected thumb diameter ~{d}, got {thumbComputed.Size.X}");
    }

    [Fact]
    public void SyncsFillColor()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, value: 50);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        var fillBg = world.Store.GetComponent<BackgroundColor>(sliderComp.FillEntity);

        Assert.Equal(sliderComp.FillColor.R, fillBg.Color.R, 0.01f);
        Assert.Equal(sliderComp.FillColor.G, fillBg.Color.G, 0.01f);
        Assert.Equal(sliderComp.FillColor.B, fillBg.Color.B, 0.01f);
    }

    [Fact]
    public void SyncsThumbColor()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world, min: 0, max: 100, value: 50);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);
        var thumbBg = world.Store.GetComponent<BackgroundColor>(sliderComp.ThumbEntity);

        Assert.Equal(sliderComp.ThumbColor.R, thumbBg.Color.R, 0.01f);
        Assert.Equal(sliderComp.ThumbColor.G, thumbBg.Color.G, 0.01f);
        Assert.Equal(sliderComp.ThumbColor.B, thumbBg.Color.B, 0.01f);
    }

    [Fact]
    public void OverlayEntities_AreChildrenOfSlider()
    {
        using var world = CreateWorld();
        var slider = SpawnSlider(world);

        var sliderComp = world.Store.GetComponent<UISlider>(slider);

        // Fill should have a Child component pointing back to the slider
        Assert.True(world.Store.HasComponent<Child>(sliderComp.FillEntity));
        var fillChild = world.Store.GetComponent<Child>(sliderComp.FillEntity);
        Assert.Equal(slider, fillChild.Parent);

        // Thumb should have a Child component pointing back to the slider
        Assert.True(world.Store.HasComponent<Child>(sliderComp.ThumbEntity));
        var thumbChild = world.Store.GetComponent<Child>(sliderComp.ThumbEntity);
        Assert.Equal(slider, thumbChild.Parent);
    }
}
