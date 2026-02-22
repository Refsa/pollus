namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;

public struct UIImageBuilder : IUINodeBuilder<UIImageBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UIImage image;

    public UIImageBuilder(Commands commands, Handle texture)
    {
        state = new UINodeBuilderState(commands);
        image = new UIImage { Texture = texture };
    }

    public UIImageBuilder Sampler(Handle sampler)
    {
        image.Sampler = sampler;
        return this;
    }

    public UIImageBuilder Slice(Rect slice)
    {
        image.Slice = slice;
        return this;
    }

    public Entity Spawn()
    {
        state.backgroundColor ??= Color.WHITE;

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            image,
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);

        return entity;
    }
}
