namespace Pollus.Engine;

using Pollus.ECS;
using Pollus.Jobs;

public class JobsPlugin : IPlugin
{
    public void Apply(World world)
    {
        ResourceFetch<JobScheduler>.Register();
        world.Resources.Add(new JobScheduler());

        world.Schedule.AddSystem(CoreStage.Last, FnSystem.Create("Jobs::Flush", static (JobScheduler jobs) =>
        {
            jobs.Flush();
        }));
    }
}