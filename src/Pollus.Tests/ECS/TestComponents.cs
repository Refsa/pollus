namespace Pollus.Tests.ECS;

using Pollus.ECS;

public partial struct TestComponent1 : IComponent, IDefault<TestComponent1>
{
    public static TestComponent1 Default => new() { Value = 111 };

    public int Value;
}

public partial struct TestComponent2 : IComponent
{
    public int Value;
}

public partial struct TestComponent3 : IComponent
{
    public int Value;
}

[Required<TestComponent1>, Required<TestComponent2>(nameof(GetTestComponent2))]
public partial struct TestComponent4 : IComponent
{
    public int Value;

    public static TestComponent2 GetTestComponent2()
    {
        return new() { Value = 222 };
    }
}