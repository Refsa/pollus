using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UIRadioGroupBuilderTests
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
    public void RadioGroup_SpawnsContainerWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<UINode>(result.Entity));
        Assert.True(world.Store.HasComponent<UIStyle>(result.Entity));
    }

    [Fact]
    public void RadioGroup_CreatesCorrectNumberOfOptions()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .Option()
            .Spawn();
        world.Update();

        Assert.Equal(3, result.OptionEntities.Length);
    }

    [Fact]
    public void RadioGroup_OptionEntitiesAreRadioButtons()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 5)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<UIRadioButton>(opt));
            var rb = world.Store.GetComponent<UIRadioButton>(opt);
            Assert.Equal(5, rb.GroupId);
        }
    }

    [Fact]
    public void RadioGroup_Selected_SetsInitialSelection()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .Option()
            .Selected(1)
            .Spawn();
        world.Update();

        var rb0 = world.Store.GetComponent<UIRadioButton>(result.OptionEntities[0]);
        var rb1 = world.Store.GetComponent<UIRadioButton>(result.OptionEntities[1]);
        var rb2 = world.Store.GetComponent<UIRadioButton>(result.OptionEntities[2]);

        Assert.False(rb0.IsSelected);
        Assert.True(rb1.IsSelected);
        Assert.False(rb2.IsSelected);
    }

    [Fact]
    public void RadioGroup_CustomColors_AppliedToAllButtons()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .SelectedColor(Color.BLUE)
            .UnselectedColor(Color.GRAY)
            .Spawn();
        world.Update();

        foreach (var opt in result.OptionEntities)
        {
            var rb = world.Store.GetComponent<UIRadioButton>(opt);
            Assert.Equal(Color.BLUE, rb.SelectedColor);
            Assert.Equal(Color.GRAY, rb.UnselectedColor);
        }
    }

    [Fact]
    public void RadioGroup_OptionsAreChildrenOfContainer()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        // Each option row should be a child of the container
        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<Child>(opt));
            Assert.Equal(result.Entity, world.Store.GetComponent<Child>(opt).Parent);
        }
    }

    [Fact]
    public void RadioGroup_ChildOf_ParentsContainer()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .ChildOf(root)
            .Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(result.Entity));
        Assert.Equal(root, world.Store.GetComponent<Child>(result.Entity).Parent);
    }

    [Fact]
    public void RadioGroup_FlexColumn_SetsDirection()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
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
    public void RadioGroup_WithTextLabels_CreatesTextEntities()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option("Small")
            .Option("Large")
            .Spawn();
        world.Update();

        // Each option entity should be a row with a radio button child and a text child
        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<Parent>(opt));
            var parent = world.Store.GetComponent<Parent>(opt);

            // Walk children to find UIText
            bool hasText = false;
            bool hasRadioButton = false;
            var child = parent.FirstChild;
            while (!child.IsNull)
            {
                if (world.Store.HasComponent<UIText>(child))
                    hasText = true;
                if (world.Store.HasComponent<UIRadioButton>(child))
                    hasRadioButton = true;

                var childComp = world.Store.GetComponent<Child>(child);
                child = childComp.NextSibling;
            }

            Assert.True(hasRadioButton, "Option row should contain a radio button");
            Assert.True(hasText, "Option row should contain text");
        }
    }

    [Fact]
    public void RadioGroup_WithTextLabels_RadioButtonsHaveCorrectGroupId()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 7)
            .Option("A")
            .Option("B")
            .Selected(0)
            .Spawn();
        world.Update();

        // Find radio button children inside each option row
        foreach (var opt in result.OptionEntities)
        {
            var parent = world.Store.GetComponent<Parent>(opt);
            var child = parent.FirstChild;
            while (!child.IsNull)
            {
                if (world.Store.HasComponent<UIRadioButton>(child))
                {
                    var rb = world.Store.GetComponent<UIRadioButton>(child);
                    Assert.Equal(7, rb.GroupId);
                    break;
                }
                var childComp = world.Store.GetComponent<Child>(child);
                child = childComp.NextSibling;
            }
        }
    }

    [Fact]
    public void RadioGroup_BareOptions_RadioButtonIsDirectChild()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = RadioGroup(commands, groupId: 1)
            .Option()
            .Option()
            .Spawn();
        world.Update();

        // Bare options: the option entity IS the radio button entity
        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<UIRadioButton>(opt));
        }
    }

    [Fact]
    public void RadioGroup_ImplicitEntityConversion()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        RadioGroupResult result = RadioGroup(commands, groupId: 1).Option().Spawn();
        Entity entity = result;
        Assert.False(entity.IsNull);
    }

    [Fact]
    public void RadioGroup_WithFont_AppliesFontToTextLabels()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var font = new Handle(1, 42);
        var result = RadioGroup(commands, groupId: 1, font)
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
}
