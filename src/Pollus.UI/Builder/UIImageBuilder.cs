namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

public class UIImageBuilder : UINodeBuilder<UIImageBuilder>
{
    UIImage image;

    public UIImageBuilder(Commands commands, Handle texture) : base(commands)
    {
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

    public override Entity Spawn()
    {
        backgroundColor ??= Color.WHITE;

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            image,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return entity;
    }
}
