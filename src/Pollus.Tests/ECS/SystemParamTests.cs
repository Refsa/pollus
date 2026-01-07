namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class SystemParamTests
{
    [Fact]
    public void Test_UniformPlugin()
    {
        using var world = new World();

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create("TestSysParam1",
            static (Param<Query<TestComponent1>> param) =>
            {
            }
        ));

        world.Schedule.AddSystems(CoreStage.Update, FnSystem.Create("TestSysParam2",
            static (Param<Query<TestComponent1, TestComponent2>> param) =>
            {
            }
        ));

        world.Prepare();
        world.Update();

        Assert.Single(world.Schedule.GetStage(CoreStage.Update)!.StageGraph.First!.Systems);
        Assert.NotNull(world.Schedule.GetStage(CoreStage.Update)!.StageGraph.First!.Next);
        Assert.Single(world.Schedule.GetStage(CoreStage.Update)!.StageGraph.First!.Next!.Systems);
    }
}