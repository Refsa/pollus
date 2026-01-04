namespace Pollus.ECS;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class BundleAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public sealed class RequiredAttribute<C> : Attribute
    where C : unmanaged, IComponent
{
    public readonly Type ComponentType = typeof(C);
}

