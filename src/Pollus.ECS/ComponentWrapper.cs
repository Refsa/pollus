#pragma warning disable IDE1006
namespace Pollus.ECS;

public interface IComponentWrapper : IComponent
{
}

public static class ComponentWrapper<TWrapper>
    where TWrapper : unmanaged, IComponent
{
    public readonly record struct OverrideInfo
    {
        public ComponentID? ID { get; init; }
        public int? SizeInBytes { get; init; }
        public Type? Type { get; init; }
        public bool? Read { get; init; }
        public bool? Write { get; init; }
    }

    public interface Target<TWrapped> : IComponentWrapper
        where TWrapped : unmanaged, IComponent
    {
        public static void Init(OverrideInfo? overrideInfo = null) => targetInfo = info with
        {
            ID = overrideInfo?.ID ?? info.ID,
            SizeInBytes = overrideInfo?.SizeInBytes ?? info.SizeInBytes,
            Type = overrideInfo?.Type ?? info.Type,
            Read = overrideInfo?.Read ?? info.Read,
            Write = overrideInfo?.Write ?? info.Write,
        };

        static Target()
        {
            info = Component.Register<TWrapped>();
        }

        static readonly Component.Info info;
    }

    static Component.Info? targetInfo;
    public static Component.Info Info => targetInfo ?? throw new InvalidOperationException($"Target<TWrapped>.Init() must be called before using ComponentWrapper<{typeof(TWrapper).FullName}>.Info");
}
#pragma warning restore IDE1006