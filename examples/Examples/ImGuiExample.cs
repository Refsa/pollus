namespace Pollus.Examples;

using Pollus.Engine.Input;
using Pollus.Engine;
using Pollus.Engine.Imgui;
using Pollus.Engine.Rendering;
using Pollus.Engine.Assets;
using Pollus.ECS;
using Pollus.Engine.Camera;

public class ImGuiExample : IExample
{
    public string Name => "imgui";

    IApplication? application;
    public void Stop() => application?.Shutdown();

    public void Run() => (application = Application.Builder
        .AddPlugins([
            new AssetPlugin { RootPath = "assets" },
            new RenderingPlugin(),
            new InputPlugin(),
            new ImguiPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn", (Commands commands) =>
        {
            commands.Spawn(Camera2D.Bundle);
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("ImGui", () =>
        {
            ImGuiNET.ImGui.ShowDemoWindow();
        }))
        .Build())
        .Run();
}