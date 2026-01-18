namespace Pollus.Examples;

using Pollus.Engine;
using Pollus.Engine.Imgui;
using Pollus.ECS;
using Pollus.Engine.Camera;

public class ImGuiExample : IExample
{
    public string Name => "imgui";

    IApplication? application;
    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new ImguiPlugin(),
        ])
        .AddSystems(CoreStage.PostInit, FnSystem.Create("Spawn", (Commands commands) =>
        {
            commands.Spawn(Camera2D.Bundle);
        }))
        .AddSystems(CoreStage.Update, FnSystem.Create("ImGui", () =>
        {
            ImGuiNET.ImGui.ShowDemoWindow();
        }))
        .Build())
        .Run();
}