namespace Pollus.UI;

using System.Diagnostics.CodeAnalysis;

public interface IUINodeBuilder<TSelf> where TSelf : struct, IUINodeBuilder<TSelf>
{
    [UnscopedRef]
    ref UINodeBuilderState State { get; }
}
