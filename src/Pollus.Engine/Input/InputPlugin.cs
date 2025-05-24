namespace Pollus.Engine.Input;

using System.Diagnostics;
using Pollus.ECS;
using Pollus.Emscripten;
using Pollus.Engine.Platform;

public class InputPlugin : IPlugin
{
    public const string PostInitSystem = "Input::PostInit";
    public const string UpdateSystem = "Input::Update";
    public const string UpdateCurrentDeviceSystem = "Input::UpdateCurrentDevice";

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

        world.Resources.Add(new CurrentDevice<Mouse>());
        world.Resources.Add(new CurrentDevice<Gamepad>());
        world.Resources.Add(new CurrentDevice<Keyboard>());

        world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create(PostInitSystem,
        (InputManager input, CurrentDevice<Mouse> currentMouse, CurrentDevice<Gamepad> currentGamepad, CurrentDevice<Keyboard> currentKeyboard) =>
        {
            foreach (var device in input.Devices)
            {
                if (device is Mouse mouse) currentMouse.Value = mouse;
                else if (device is Gamepad gamepad) currentGamepad.Value = gamepad;
                else if (device is Keyboard keyboard) currentKeyboard.Value = keyboard;
            }
        }));

        world.Schedule.AddSystems(CoreStage.First, FnSystem.Create(UpdateSystem,
        (InputManager input, PlatformEvents platform, Events events,
         ButtonInput<MouseButton> mButtons, AxisInput<MouseAxis> mAxes,
         ButtonInput<GamepadButton> gButtons, AxisInput<GamepadAxis> gAxes,
         ButtonInput<Key> kButtons
        ) =>
        {
            input.Update(platform, events);

            foreach (var device in input.Devices)
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

        world.Schedule.AddSystems(CoreStage.First, FnSystem.Create(UpdateCurrentDeviceSystem,
        (InputManager input, CurrentDevice<Mouse> currentMouse, CurrentDevice<Gamepad> currentGamepad, CurrentDevice<Keyboard> currentKeyboard) =>
        {
            foreach (var device in input.Devices)
            {
                if (device is Mouse mouse)
                {
                    if (mouse.IsActive) currentMouse.Value = mouse;
                }
                else if (device is Gamepad gamepad)
                {
                    if (gamepad.IsActive) currentGamepad.Value = gamepad;
                }
                else if (device is Keyboard keyboard)
                {
                    if (keyboard.IsActive) currentKeyboard.Value = keyboard;
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