using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UINodeBuilderTests
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
    public void Panel_SpawnsWithUINodeAndUIStyle()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(entity));
        Assert.True(world.Store.HasComponent<UIStyle>(entity));
    }

    [Fact]
    public void Panel_Size_SetsWidthAndHeight()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Size(200, 100).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(200), style.Value.Size.Width);
        Assert.Equal(Length.Px(100), style.Value.Size.Height);
    }

    [Fact]
    public void Panel_FlexColumn_SetsDirection()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).FlexColumn().Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(FlexDirection.Column, style.Value.FlexDirection);
    }

    [Fact]
    public void Panel_FlexRow_SetsDirection()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).FlexRow().Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(FlexDirection.Row, style.Value.FlexDirection);
    }

    [Fact]
    public void Panel_Gap_SetsBothAxes()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Gap(8).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(8), style.Value.Gap.Width);
        Assert.Equal(Length.Px(8), style.Value.Gap.Height);
    }

    [Fact]
    public void Panel_Padding_SetsAllSides()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Padding(10).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(10), style.Value.Padding.Top);
        Assert.Equal(Length.Px(10), style.Value.Padding.Right);
        Assert.Equal(Length.Px(10), style.Value.Padding.Bottom);
        Assert.Equal(Length.Px(10), style.Value.Padding.Left);
    }

    [Fact]
    public void Panel_Padding_SetsIndividualSides()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Padding(1, 2, 3, 4).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(1), style.Value.Padding.Top);
        Assert.Equal(Length.Px(2), style.Value.Padding.Right);
        Assert.Equal(Length.Px(3), style.Value.Padding.Bottom);
        Assert.Equal(Length.Px(4), style.Value.Padding.Left);
    }

    [Fact]
    public void Panel_Background_AddsBackgroundColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Background(Color.RED).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
        var bg = world.Store.GetComponent<BackgroundColor>(entity);
        Assert.Equal(Color.RED, bg.Color);
    }

    [Fact]
    public void Panel_NoBackground_DoesNotAddBackgroundColor()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Spawn();
        world.Update();

        Assert.False(world.Store.HasComponent<BackgroundColor>(entity));
    }

    [Fact]
    public void Panel_BorderRadius_AddsComponent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).BorderRadius(5).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<BorderRadius>(entity));
        var br = world.Store.GetComponent<BorderRadius>(entity);
        Assert.Equal(5f, br.TopLeft);
        Assert.Equal(5f, br.TopRight);
        Assert.Equal(5f, br.BottomRight);
        Assert.Equal(5f, br.BottomLeft);
    }

    [Fact]
    public void Panel_ChildOf_SetsParent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var panel = Panel(commands).Size(200, 100).ChildOf(root).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(panel));
        var child = world.Store.GetComponent<Child>(panel);
        Assert.Equal(root, child.Parent);
    }

    [Fact]
    public void Panel_Children_AddsChildren()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var child1 = Panel(commands).Spawn();
        var child2 = Panel(commands).Spawn();

        var parent = Panel(commands).Children(child1, child2).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Parent>(parent));
        var parentComp = world.Store.GetComponent<Parent>(parent);
        Assert.Equal(2, parentComp.ChildCount);
        Assert.Equal(child1, parentComp.FirstChild);
    }

    [Fact]
    public void Panel_FluentChaining_AllMethodsWork()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands)
            .Size(200, 100)
            .FlexColumn()
            .Gap(8)
            .Padding(10)
            .Background(Color.GRAY)
            .BorderRadius(4)
            .Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(200), style.Value.Size.Width);
        Assert.Equal(Length.Px(100), style.Value.Size.Height);
        Assert.Equal(FlexDirection.Column, style.Value.FlexDirection);
        Assert.Equal(Length.Px(8), style.Value.Gap.Width);
        Assert.True(world.Store.HasComponent<BackgroundColor>(entity));
        Assert.True(world.Store.HasComponent<BorderRadius>(entity));
    }

    [Fact]
    public void Panel_WidthPercent_SetsPercentWidth()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).WidthPercent(0.5f).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Percent(0.5f), style.Value.Size.Width);
    }

    [Fact]
    public void Panel_AlignItems_SetsAlignment()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).AlignItems(AlignItems.Center).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(AlignItems.Center, style.Value.AlignItems);
    }

    [Fact]
    public void Panel_JustifyContent_SetsJustification()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).JustifyContent(JustifyContent.SpaceBetween).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(JustifyContent.SpaceBetween, style.Value.JustifyContent);
    }

    [Fact]
    public void Panel_PositionAbsolute_SetsAbsolute()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).PositionAbsolute().Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Position.Absolute, style.Value.Position);
    }

    [Fact]
    public void Panel_BorderColor_AddsComponent()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).BorderColor(Color.RED).Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Pollus.UI.BorderColor>(entity));
        var bc = world.Store.GetComponent<Pollus.UI.BorderColor>(entity);
        Assert.Equal(Color.RED, bc.Top);
        Assert.Equal(Color.RED, bc.Right);
    }

    [Fact]
    public void Panel_Margin_SetsAllSides()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var entity = Panel(commands).Margin(5).Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(entity);
        Assert.Equal(Length.Px(5), style.Value.Margin.Top);
        Assert.Equal(Length.Px(5), style.Value.Margin.Left);
    }
}
