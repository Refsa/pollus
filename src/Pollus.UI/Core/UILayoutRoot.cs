namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;

public partial record struct UILayoutRoot() : IComponent
{
    public Size<float> Size;
}
