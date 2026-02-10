using Pollus.ECS;
using Pollus.Utils;

namespace Pollus.UI;

[Flags]
public enum InteractionState : byte
{
    None     = 0,
    Hovered  = 1 << 0,
    Pressed  = 1 << 1,
    Focused  = 1 << 2,
    Disabled = 1 << 3,
}

public partial record struct UIInteraction() : IComponent, IDefault<UIInteraction>
{
    public InteractionState State;
    public int TabIndex;
    public bool Focusable;

    public readonly bool IsHovered => (State & InteractionState.Hovered) != 0;
    public readonly bool IsPressed => (State & InteractionState.Pressed) != 0;
    public readonly bool IsFocused => (State & InteractionState.Focused) != 0;
    public readonly bool IsDisabled => (State & InteractionState.Disabled) != 0;

    public static UIInteraction Default { get; } = default;

    static UIInteraction()
    {
        Component.Register<UIInteraction>();
        RequiredComponents.Init<UIInteraction>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UIInteraction>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
