namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UIStyle() : IComponent, IDefault<UIStyle>
{
    public Layout.Style Value = Layout.Style.Default;
    public static UIStyle Default => new();
}
