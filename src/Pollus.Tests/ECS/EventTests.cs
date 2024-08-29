namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class EventTests
{
    record struct TestEvent(int Value);

    [Fact]
    public void Event_OneFrame_Lifetime()
    {
        using var world = new World();
        world.Events.InitEvent<TestEvent>();
        var reader = world.Events.GetReader<TestEvent>()!;

        var writer = world.Events.GetWriter<TestEvent>();
        for (int i = 0; i < 10; i++)
        {
            writer.Write(new TestEvent(i));
        }
        world.Update();

        var events = reader.Read();
        Assert.Equal(10, events.Length);
        for (int i = 0; i < events.Length; i++)
        {
            Assert.Equal(i, events[i].Value);
        }

        world.Update();
        events = reader.Read();
        Assert.Equal(0, events.Length);
    }
}