namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial struct UIMaterial() : IComponent
{
    public Handle Material = Handle.Null;
}
