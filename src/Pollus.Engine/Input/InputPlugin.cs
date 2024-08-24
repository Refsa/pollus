namespace Pollus.Engine.Input;

using System.Runtime.InteropServices;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Engine.Platform;
using static Pollus.ECS.SystemBuilder;

public class InputPlugin : IPlugin
{
    public void Apply(World world)
    {
        SetupBrowser();
        world.Resources.Add(new InputManager());

        world.Events.InitEvent<ButtonEvent<MouseButton>>();
        world.Events.InitEvent<AxisEvent<MouseAxis>>();
        world.Events.InitEvent<ButtonEvent<GamepadButton>>();
        world.Events.InitEvent<AxisEvent<GamepadAxis>>();
        world.Events.InitEvent<ButtonEvent<Key>>();

        world.Schedule.AddSystems(CoreStage.First, FnSystem("InputUpdate",
        (InputManager input, PlatformEvents platform, Events events) =>
        {
            input.Update(platform, events);
        }));
    }

    void SetupBrowser()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            var sdlFlags = SDLInitFlags.InitEvents | SDLInitFlags.InitGamecontroller | SDLInitFlags.InitJoystick;
            if (EmscriptenSDL.WasInit(sdlFlags) is false)
            {
                if (EmscriptenSDL.AnyInitialized())
                {
                    EmscriptenSDL.InitSubSystem(sdlFlags);
                }
                else
                {
                    EmscriptenSDL.Init(sdlFlags);
                }
            }
        }
    }
}