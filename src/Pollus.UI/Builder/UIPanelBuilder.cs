namespace Pollus.UI;

using Pollus.ECS;
using System.Diagnostics.CodeAnalysis;

public struct UIPanelBuilder : IUINodeBuilder<UIPanelBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    public UIPanelBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
    }

    public Entity Spawn()
    {
        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);

        return entity;
    }
}
