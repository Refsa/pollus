namespace Pollus.Engine.Camera;

using Pollus.ECS;
using Pollus.Engine.Transform;
using Pollus.Graphics.Windowing;
using static Pollus.ECS.SystemBuilder;

public class CameraPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, new[]
        {
            FnSystem("Camera2D", (IWindow window, Query<OrthographicProjection>.Filter<All<Camera2D>> query) =>
            {
                query.ForEach((ref OrthographicProjection projection) =>
                {
                    projection.Update(window.Size);
                });
            })
        });
    }
}