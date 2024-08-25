namespace Pollus.Engine.Input;

using System.Diagnostics;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Engine.Platform;
using static Pollus.ECS.SystemBuilder;

public class InputPlugin : IPlugin
{
    static InputPlugin()
    {
        ResourceFetch<ButtonInput<MouseButton>>.Register();
        ResourceFetch<AxisInput<MouseAxis>>.Register();
        ResourceFetch<ButtonInput<GamepadButton>>.Register();
        ResourceFetch<AxisInput<GamepadAxis>>.Register();
        ResourceFetch<ButtonInput<Key>>.Register();
    }

    public void Apply(World world)
    {
        SetupBrowser();
        world.Resources.Add(new InputManager());

        world.Events.InitEvent<ButtonEvent<MouseButton>>();
        world.Events.InitEvent<AxisEvent<MouseAxis>>();
        world.Events.InitEvent<ButtonEvent<GamepadButton>>();
        world.Events.InitEvent<AxisEvent<GamepadAxis>>();
        world.Events.InitEvent<ButtonEvent<Key>>();
        world.Events.InitEvent<MouseMovedEvent>();

        world.Resources.Add(new ButtonInput<MouseButton>());
        world.Resources.Add(new AxisInput<MouseAxis>());
        world.Resources.Add(new ButtonInput<GamepadButton>());
        world.Resources.Add(new AxisInput<GamepadAxis>());
        world.Resources.Add(new ButtonInput<Key>());

        world.Schedule.AddSystems(CoreStage.First, FnSystem("InputUpdate",
        (InputManager input, PlatformEvents platform, Events events,
         ButtonInput<MouseButton> mButtons, AxisInput<MouseAxis> mAxes,
         ButtonInput<GamepadButton> gButtons, AxisInput<GamepadAxis> gAxes,
         ButtonInput<Key> kButtons
        ) =>
        {
            input.Update(platform, events);

            foreach (var device in input.ConnectedDevices)
            {
                if (device is Mouse mouse)
                {
                    mButtons.AddDevice(device.Id, mouse);
                    mAxes.AddDevice(device.Id, mouse);
                }
                else if (device is Gamepad gamepad)
                {
                    gButtons.AddDevice(device.Id, gamepad);
                    gAxes.AddDevice(device.Id, gamepad);
                }
                else if (device is Keyboard keyboard)
                {
                    kButtons.AddDevice(device.Id, keyboard);
                }
            }
        }));
    }

    [Conditional("BROWSER")]
    void SetupBrowser()
    {
        var sdlFlags = SDLInitFlags.InitJoystick;
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

        EmscriptenSDL.JoystickEventState(1);
        EmscriptenSDL.GameControllerEventState(1);
    }
}