namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class ComponentTests
{
    [Fact]
    public void component_info()
    {
        Assert.Equal(0, (int)Component.GetInfo<TestComponent1>().ID);
        Assert.Equal(1, (int)Component.GetInfo<TestComponent2>().ID);
        Assert.Equal(2, (int)Component.GetInfo<TestComponent3>().ID);

        Assert.Equal(0, (int)Component.GetInfo<TestComponent1>().ID);
        Assert.Equal(1, (int)Component.GetInfo<TestComponent2>().ID);
        Assert.Equal(2, (int)Component.GetInfo<TestComponent3>().ID);
    }
}