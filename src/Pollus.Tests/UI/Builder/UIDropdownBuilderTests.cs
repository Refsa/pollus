using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using static Pollus.UI.UI;

namespace Pollus.Tests.UI.Builder;

public class UIDropdownBuilderTests
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
    public void Dropdown_SpawnsWithRequiredComponents()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A").Option("B")
            .Spawn();
        world.Update();

        Entity trigger = result;
        Assert.True(world.Store.HasComponent<UINode>(trigger));
        Assert.True(world.Store.HasComponent<UIDropdown>(trigger));
        Assert.True(world.Store.HasComponent<UIInteraction>(trigger));
        Assert.True(world.Store.HasComponent<BackgroundColor>(trigger));
    }

    [Fact]
    public void Dropdown_CreatesDisplayTextEntity()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Placeholder("Select...")
            .Option("A")
            .Spawn();
        world.Update();

        Assert.False(result.DisplayTextEntity.IsNull);
        Assert.True(world.Store.HasComponent<UIText>(result.DisplayTextEntity));
        Assert.True(world.Store.HasComponent<ContentSize>(result.DisplayTextEntity));

        var text = world.Store.GetComponent<UIText>(result.DisplayTextEntity);
        Assert.Equal("Select...", text.Text.ToString().TrimEnd('\0'));
    }

    [Fact]
    public void Dropdown_DisplayTextIsChildOfTrigger()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A")
            .Spawn();
        world.Update();

        Assert.True(world.Store.HasComponent<Child>(result.DisplayTextEntity));
        Assert.Equal(result.Entity, world.Store.GetComponent<Child>(result.DisplayTextEntity).Parent);
    }

    [Fact]
    public void Dropdown_CreatesCorrectNumberOfOptions()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A").Option("B").Option("C")
            .Spawn();
        world.Update();

        Assert.Equal(3, result.OptionEntities.Length);
    }

    [Fact]
    public void Dropdown_OptionEntitiesHaveCorrectTags()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A").Option("B").Option("C")
            .Spawn();
        world.Update();

        for (int i = 0; i < result.OptionEntities.Length; i++)
        {
            var opt = result.OptionEntities[i];
            Assert.True(world.Store.HasComponent<UIDropdownOptionTag>(opt));

            var tag = world.Store.GetComponent<UIDropdownOptionTag>(opt);
            Assert.Equal(result.Entity, tag.DropdownEntity);
            Assert.Equal(i, tag.OptionIndex);
        }
    }

    [Fact]
    public void Dropdown_OptionsHaveTextChildren()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("Alpha").Option("Beta")
            .Spawn();
        world.Update();

        // Each option should have a child with UIText
        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<Parent>(opt), "Option should have children");
            var parent = world.Store.GetComponent<Parent>(opt);
            var textChild = parent.FirstChild;
            Assert.False(textChild.IsNull);
            Assert.True(world.Store.HasComponent<UIText>(textChild));
        }
    }

    [Fact]
    public void Dropdown_PopupPanelStartsHidden()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A").Option("B")
            .Spawn();
        world.Update();

        var panelStyle = world.Store.GetComponent<UIStyle>(result.PopupPanelEntity);
        Assert.Equal(Display.None, panelStyle.Value.Display);
        Assert.Equal(Position.Absolute, panelStyle.Value.Position);
    }

    [Fact]
    public void Dropdown_DropdownComponentReferencesDisplayText()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A")
            .Spawn();
        world.Update();

        var dd = world.Store.GetComponent<UIDropdown>(result.Entity);
        Assert.Equal(result.DisplayTextEntity, dd.DisplayTextEntity);
    }

    [Fact]
    public void Dropdown_StartsWithNoSelection()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A")
            .Spawn();
        world.Update();

        var dd = world.Store.GetComponent<UIDropdown>(result.Entity);
        Assert.Equal(-1, dd.SelectedIndex);
    }

    [Fact]
    public void Dropdown_IsFocusable()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands).Size(200, 30).Option("A").Spawn();
        world.Update();

        var interaction = world.Store.GetComponent<UIInteraction>(result.Entity);
        Assert.True(interaction.Focusable);
    }

    [Fact]
    public void Dropdown_ImplicitEntityConversion()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        DropdownResult result = Dropdown(commands).Size(200, 30).Option("A").Spawn();
        Entity entity = result; // implicit conversion
        Assert.False(entity.IsNull);
    }

    [Fact]
    public void Dropdown_ChildOf_ParentsAllEntities()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var root = commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(800, 600) }
        )).Entity;

        var result = Dropdown(commands)
            .Size(200, 30)
            .Option("A").Option("B")
            .ChildOf(root)
            .Spawn();
        world.Update();

        // Trigger is child of root
        Assert.True(world.Store.HasComponent<Child>(result.Entity));
        Assert.Equal(root, world.Store.GetComponent<Child>(result.Entity).Parent);

        // Popup panel is child of trigger
        Assert.True(world.Store.HasComponent<Child>(result.PopupPanelEntity));
        Assert.Equal(result.Entity, world.Store.GetComponent<Child>(result.PopupPanelEntity).Parent);

        // Options are children of the popup panel
        foreach (var opt in result.OptionEntities)
        {
            Assert.True(world.Store.HasComponent<Child>(opt));
            Assert.Equal(result.PopupPanelEntity, world.Store.GetComponent<Child>(opt).Parent);
        }
    }

    [Fact]
    public void Dropdown_Options_BulkAdd()
    {
        using var world = CreateWorld();
        var commands = world.GetCommands();

        var result = Dropdown(commands)
            .Size(200, 30)
            .Options("X", "Y", "Z")
            .Spawn();
        world.Update();

        Assert.Equal(3, result.OptionEntities.Length);
    }
}
