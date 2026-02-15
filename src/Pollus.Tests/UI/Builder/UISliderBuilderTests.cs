using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UISliderBuilderTests
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

    [Fact]
    public void Slider_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Slider(commands).Size(200, 20).Spawn();
        world.Update();

        Entity entity = result;
        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UISlider>(entity));
        Assert.True(world.Store.HasComponent<UIInteraction>(entity));
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void Slider_SetsValueAndRange()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Slider(commands)
            .Value(0.5f)
            .Range(0, 10)
            .Step(0.1f)
            .Spawn();
        world.Update();

        var slider = world.Store.GetComponent<UISlider>(result.Entity);
        Assert.Equal(0.5f, slider.Value);
        Assert.Equal(0f, slider.Min);
        Assert.Equal(10f, slider.Max);
        Assert.Equal(0.1f, slider.Step);
    }

    [Fact]
    public void Slider_CustomColors()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Slider(commands)
            .TrackColor(Color.GRAY)
            .FillColor(Color.BLUE)
            .ThumbColor(Color.WHITE)
            .Spawn();
        world.Update();

        var slider = world.Store.GetComponent<UISlider>(result.Entity);
        Assert.Equal(Color.GRAY, slider.TrackColor);
        Assert.Equal(Color.BLUE, slider.FillColor);
        Assert.Equal(Color.WHITE, slider.ThumbColor);
    }

    [Fact]
    public void Slider_ImplicitEntityConversion()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        SliderResult result = Slider(commands).Spawn();
        Entity entity = result; // implicit conversion

        Assert.False(entity.IsNull);
    }

    [Fact]
    public void Slider_FillAndThumbCreatedBySystem()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var result = Slider(commands).Size(200, 20).ChildOf(root).Spawn();
        world.Update();

        // Fill and thumb are created by the slider system during Update
        var slider = world.Store.GetComponent<UISlider>(result.Entity);
        Assert.False(slider.FillEntity.IsNull);
        Assert.False(slider.ThumbEntity.IsNull);
    }
}
