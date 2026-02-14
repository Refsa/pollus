namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UIStyle() : IComponent
{
    public Layout.Style Value = Layout.Style.Default;
}
