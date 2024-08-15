namespace Pollus.ECS;

public interface IComponentWrapper : IComponent { }

public static class ComponentWrapper<TWrapper>
    where TWrapper : unmanaged, IComponent
{
    public interface Target<TWrapped> : IComponentWrapper
        where TWrapped : unmanaged, IComponent
    {
        public static void Init() => targetInfo = info;
        static Target() => info = Component.Register<TWrapped>();
        static readonly Component.Info info;
    }

    static Component.Info targetInfo;
    public static Component.Info Info => targetInfo;
}