using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UICheckBoxGroupBuilderTests
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
    public void CheckBoxGroup_SpawnsContainerWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(result.Entity));
        Assert.True(world.Store.HasComponent<UIStyle>(result.Entity));
    }

    [Fact]
    public void CheckBoxGroup_CreatesCorrectNumberOfOptions()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Option()
            .Spawn();
        world.Update();

        Assert.Equal(3, result.OptionEntities.Length);
    }

    [Fact]
    public void CheckBoxGroup_OptionEntitiesAreCheckBoxes()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<UICheckBox>(opt));
        }
    }

    [Fact]
    public void CheckBoxGroup_Checked_SetsInitialCheckedState()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Option()
            .Checked(0)
            .Checked(2)
            .Spawn();
        world.Update();

        var cb0 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[0]);
        var cb1 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[1]);
        var cb2 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[2]);

        Assert.True(cb0.IsChecked);
        Assert.False(cb1.IsChecked);
        Assert.True(cb2.IsChecked);
    }

    [Fact]
    public void CheckBoxGroup_CustomColors_AppliedToAllCheckBoxes()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .CheckedColor(Color.BLUE)
            .UncheckedColor(Color.GRAY)
            .CheckmarkColor(Color.RED)
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            var cb = world.Store.GetComponent<UICheckBox>(opt);
            Assert.Equal(Color.BLUE, cb.CheckedColor);
            Assert.Equal(Color.GRAY, cb.UncheckedColor);
            Assert.Equal(Color.RED, cb.CheckmarkColor);
        }
    }

    [Fact]
    public void CheckBoxGroup_OptionsAreChildrenOfContainer()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<Child>(opt));
            Assert.Equal(result.Entity, world.Store.GetComponent<Child>(opt).Parent);
        }
    }

    [Fact]
    public void CheckBoxGroup_ChildOf_ParentsContainer()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .ChildOf(root)
            .Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(result.Entity));
        Assert.Equal(root, world.Store.GetComponent<Child>(result.Entity).Parent);
    }

    [Fact]
    public void CheckBoxGroup_FlexColumn_SetsDirection()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .FlexColumn()
            .Gap(4)
            .Spawn();
        world.Update();

        var style = world.Store.GetComponent<UIStyle>(result.Entity);
        Assert.Equal(FlexDirection.Column, style.Value.FlexDirection);
        Assert.Equal(Length.Px(4), style.Value.Gap.Width);
    }

    [Fact]
    public void CheckBoxGroup_WithTextLabels_CreatesTextEntities()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option("Option A")
            .Option("Option B")
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<Parent>(opt));
            var parent = world.Store.GetComponent<Parent>(opt);

            bool hasText = false;
            bool hasCheckBox = false;
            var child = parent.FirstChild;
            while (!child.IsNull)
            {
                if (world.Store.HasComponent<UIText>(child))
                    hasText = true;
                if (world.Store.HasComponent<UICheckBox>(child))
                    hasCheckBox = true;

                var childComp = world.Store.GetComponent<Child>(child);
                child = childComp.NextSibling;
            }

            Assert.True(hasCheckBox, "Option row should contain a checkbox");
            Assert.True(hasText, "Option row should contain text");
        }
    }

    [Fact]
    public void CheckBoxGroup_BareOptions_CheckBoxIsDirectChild()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<UICheckBox>(opt));
        }
    }

    [Fact]
    public void CheckBoxGroup_ImplicitEntityConversion()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        CheckBoxGroupResult result = CheckBoxGroup(commands).Option().Spawn();
        Entity entity = result;
        Assert.False(entity.IsNull);
    }

    [Fact]
    public void CheckBoxGroup_WithFont_AppliesFontToTextLabels()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var font = new Handle(1, 42);
        var result = CheckBoxGroup(commands, font)
            .Option("A")
            .Option("B")
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            var parent = world.Store.GetComponent<Parent>(opt);
            var child = parent.FirstChild;
            while (!child.IsNull)
            {
                if (world.Store.HasComponent<UIText>(child))
                {
                    Assert.True(world.Store.HasComponent<UITextFont>(child));
                    var textFont = world.Store.GetComponent<UITextFont>(child);
                    Assert.Equal(font, textFont.Font);
                    break;
                }
                var childComp = world.Store.GetComponent<Child>(child);
                child = childComp.NextSibling;
            }
        }
    }

    [Fact]
    public void CheckBoxGroup_MultipleChecked_AllowsMultipleSelections()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = CheckBoxGroup(commands)
            .Option()
            .Option()
            .Option()
            .Option()
            .Checked(0)
            .Checked(1)
            .Checked(3)
            .Spawn();
        world.Update();

        var cb0 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[0]);
        var cb1 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[1]);
        var cb2 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[2]);
        var cb3 = world.Store.GetComponent<UICheckBox>(result.OptionEntities[3]);

        Assert.True(cb0.IsChecked);
        Assert.True(cb1.IsChecked);
        Assert.False(cb2.IsChecked);
        Assert.True(cb3.IsChecked);
    }
}
