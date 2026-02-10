using Pollus.UI;

namespace Pollus.Tests.UI.Interaction;

public class UIInteractionComponentTests
{
    [Fact]
    public void Default_StateIsNone()
    {
        var interaction = new UIInteraction();
        Assert.Equal(InteractionState.None, interaction.State);
    }

    [Fact]
    public void Default_TabIndexIsZero()
    {
        var interaction = new UIInteraction();
        Assert.Equal(0, interaction.TabIndex);
    }

    [Fact]
    public void Default_FocusableIsFalse()
    {
        var interaction = new UIInteraction();
        Assert.False(interaction.Focusable);
    }

    [Fact]
    public void Flags_CombineCorrectly()
    {
        var interaction = new UIInteraction
        {
            State = InteractionState.Hovered | InteractionState.Pressed,
        };

        Assert.True(interaction.State.HasFlag(InteractionState.Hovered));
        Assert.True(interaction.State.HasFlag(InteractionState.Pressed));
        Assert.False(interaction.State.HasFlag(InteractionState.Focused));
    }

    [Fact]
    public void IsHovered_ReturnsTrueWhenHoveredFlagSet()
    {
        var interaction = new UIInteraction { State = InteractionState.Hovered };
        Assert.True(interaction.IsHovered);
    }

    [Fact]
    public void IsHovered_ReturnsFalseWhenNotSet()
    {
        var interaction = new UIInteraction { State = InteractionState.Pressed };
        Assert.False(interaction.IsHovered);
    }

    [Fact]
    public void IsPressed_ReturnsTrueWhenPressedFlagSet()
    {
        var interaction = new UIInteraction { State = InteractionState.Pressed };
        Assert.True(interaction.IsPressed);
    }

    [Fact]
    public void IsFocused_ReturnsTrueWhenFocusedFlagSet()
    {
        var interaction = new UIInteraction { State = InteractionState.Focused };
        Assert.True(interaction.IsFocused);
    }

    [Fact]
    public void IsDisabled_ReturnsTrueWhenDisabledFlagSet()
    {
        var interaction = new UIInteraction { State = InteractionState.Disabled };
        Assert.True(interaction.IsDisabled);
    }

    [Fact]
    public void IsDisabled_DoesNotBlockOtherFlags()
    {
        var interaction = new UIInteraction
        {
            State = InteractionState.Disabled | InteractionState.Hovered,
        };

        Assert.True(interaction.IsDisabled);
        Assert.True(interaction.IsHovered);
    }
}
