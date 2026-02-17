namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial struct UITextFont : IComponent, IDefault<UITextFont>
{
    public static UITextFont Default { get; } = new() { Font = Handle.Null, Material = Handle.Null };

    public required Handle Font;
    public Handle Material;
}
