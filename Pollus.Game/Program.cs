using Pollus.ECS;

var entity = new Entity(0, 0);

partial struct TestComponent : IComponent
{
    public static ComponentLookup.Info Info => ComponentLookup.Register<TestComponent>();
}