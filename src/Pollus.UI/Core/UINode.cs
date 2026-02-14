namespace Pollus.UI;

using Pollus.ECS;

[Required<UIStyle>, Required<ComputedNode>]
public partial record struct UINode() : IComponent
{
}
