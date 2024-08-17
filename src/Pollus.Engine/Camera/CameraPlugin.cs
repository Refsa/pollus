namespace Pollus.Engine.Camera;

using Pollus.ECS;
using Pollus.Graphics.Windowing;
using static Pollus.ECS.SystemBuilder;

public class CameraPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, new[]
        {
            FnSystem("Camera::Update", (IWindow window, Query<Projection> query) =>
            {
                query.ForEach((ref Projection projection) =>
                {
                    projection.Update(window.Size);
                });
            })
        });
    }
}