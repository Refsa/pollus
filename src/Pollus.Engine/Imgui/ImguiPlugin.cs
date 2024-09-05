namespace Pollus.Engine.Imgui;

using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Engine.Platform;
using Pollus.Engine.Rendering;
using Pollus.Graphics.Imgui;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

class ImguiDraw
{
    public RenderStep2D Stage => RenderStep2D.UI;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var imguiRenderer = resources.Get<ImguiRenderer>();
        imguiRenderer.Render(encoder);
    }
}

public class ImguiPlugin : IPlugin
{
    public const string SetupSystem = "ImGui::Setup";
    public const string UpdateSystem = "ImGui::UpdateIO";
    public const string BeginFrameSystem = "ImGui::BeginFrame";

    public void Apply(World world)
    {
        world.AddPlugins([
            new RenderingPlugin(),
            new InputPlugin(),
        ]);

        world.Resources.Init<ImguiRenderer>();

        world.Schedule.AddSystems(CoreStage.Init, SystemBuilder.FnSystem(
            SetupSystem,
            static (Resources resources, IWGPUContext gpuContext, IWindow window, RenderSteps renderGraph) =>
            {
                var imguiRenderer = new ImguiRenderer(gpuContext, gpuContext.GetSurfaceFormat(), window.Size);
                resources.Add(imguiRenderer);
            }
        ));

        world.Schedule.AddSystems(CoreStage.First, SystemBuilder.FnSystem(
            UpdateSystem,
            static (
                PlatformEvents platformEvents,
                EventReader<ButtonEvent<Key>> eKeys,
                EventReader<ButtonEvent<MouseButton>> eMouseButtons,
                EventReader<AxisEvent<MouseAxis>> eMouseAxes,
                EventReader<MouseMovedEvent> eMouseMoved
            ) =>
            {
                var io = ImGui.GetIO();
                Span<char> textInput = stackalloc char[32];

                foreach (var ev in platformEvents.Events)
                {
                    if (ev.Type is (int)Silk.NET.SDL.EventType.Textinput)
                    {
                        unsafe
                        {
                            var textSpan = new Span<byte>(ev.Text.Text, 32);
                            int count = Encoding.UTF8.GetChars(textSpan, textInput);
                            foreach (var c in textInput)
                            {
                                if (c == '\0') break;
                                io.AddInputCharacter(c);
                            }
                        }
                    }
                }

                foreach (var key in eKeys.Read())
                {
                    var state = key.State == ButtonState.JustPressed;
                    io.AddKeyEvent(MapKey(key.Button), state);
                }

                foreach (var button in eMouseButtons.Read())
                {
                    var state = button.State == ButtonState.JustPressed;
                    io.AddMouseButtonEvent((int)button.Button - 1, state);
                }

                foreach (var axis in eMouseAxes.Read())
                {
                    if (axis.Axis == MouseAxis.ScrollY)
                    {
                        io.AddMouseWheelEvent(0f, axis.Value);
                    }
                    else if (axis.Axis == MouseAxis.ScrollX)
                    {
                        io.AddMouseWheelEvent(axis.Value, 0f);
                    }
                }

                foreach (var moved in eMouseMoved.Read())
                {
                    io.AddMousePosEvent(moved.Position.X, moved.Position.Y);
                }
            }
        ));

        world.Schedule.AddSystems(CoreStage.First, SystemBuilder.FnSystem(
            BeginFrameSystem,
            static (ImguiRenderer imguiRenderer, Time time, IWindow window, PlatformEvents platformEvents) =>
            {
                imguiRenderer.Resized(window.Size);
                imguiRenderer.Update((float)time.DeltaTime);
            }
        ).After(UpdateSystem));
    }

    static ImGuiKey MapKey(Key key)
    {
        return key switch
        {
            Key.KeyA => ImGuiKey.A,
            Key.KeyB => ImGuiKey.B,
            Key.KeyC => ImGuiKey.C,
            Key.KeyD => ImGuiKey.D,
            Key.KeyE => ImGuiKey.E,
            Key.KeyF => ImGuiKey.F,
            Key.KeyG => ImGuiKey.G,
            Key.KeyH => ImGuiKey.H,
            Key.KeyI => ImGuiKey.I,
            Key.KeyJ => ImGuiKey.J,
            Key.KeyK => ImGuiKey.K,
            Key.KeyL => ImGuiKey.L,
            Key.KeyM => ImGuiKey.M,
            Key.KeyN => ImGuiKey.N,
            Key.KeyO => ImGuiKey.O,
            Key.KeyP => ImGuiKey.P,
            Key.KeyQ => ImGuiKey.Q,
            Key.KeyR => ImGuiKey.R,
            Key.KeyS => ImGuiKey.S,
            Key.KeyT => ImGuiKey.T,
            Key.KeyU => ImGuiKey.U,
            Key.KeyV => ImGuiKey.V,
            Key.KeyW => ImGuiKey.W,
            Key.KeyX => ImGuiKey.X,
            Key.KeyY => ImGuiKey.Y,
            Key.KeyZ => ImGuiKey.Z,
            Key.Digit1 => ImGuiKey._1,
            Key.Digit2 => ImGuiKey._2,
            Key.Digit3 => ImGuiKey._3,
            Key.Digit4 => ImGuiKey._4,
            Key.Digit5 => ImGuiKey._5,
            Key.Digit6 => ImGuiKey._6,
            Key.Digit7 => ImGuiKey._7,
            Key.Digit8 => ImGuiKey._8,
            Key.Digit9 => ImGuiKey._9,
            Key.Digit0 => ImGuiKey._0,
            Key.Enter => ImGuiKey.Enter,
            Key.Escape => ImGuiKey.Escape,
            Key.Backspace => ImGuiKey.Backspace,
            Key.Tab => ImGuiKey.Tab,
            Key.Space => ImGuiKey.Space,
            Key.Minus => ImGuiKey.Minus,
            Key.Equal => ImGuiKey.Equal,
            Key.Backslash => ImGuiKey.Backslash,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Apostrophe => ImGuiKey.Apostrophe,
            Key.Comma => ImGuiKey.Comma,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.F1 => ImGuiKey.F1,
            Key.F2 => ImGuiKey.F2,
            Key.F3 => ImGuiKey.F3,
            Key.F4 => ImGuiKey.F4,
            Key.F5 => ImGuiKey.F5,
            Key.F6 => ImGuiKey.F6,
            Key.F7 => ImGuiKey.F7,
            Key.F8 => ImGuiKey.F8,
            Key.F9 => ImGuiKey.F9,
            Key.F10 => ImGuiKey.F10,
            Key.F11 => ImGuiKey.F11,
            Key.F12 => ImGuiKey.F12,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.Pause => ImGuiKey.Pause,
            Key.Insert => ImGuiKey.Insert,
            Key.Home => ImGuiKey.Home,
            Key.PageUp => ImGuiKey.PageUp,
            Key.Delete => ImGuiKey.Delete,
            Key.End => ImGuiKey.End,
            Key.PageDown => ImGuiKey.PageDown,
            Key.ArrowRight => ImGuiKey.RightArrow,
            Key.ArrowLeft => ImGuiKey.LeftArrow,
            Key.ArrowDown => ImGuiKey.DownArrow,
            Key.ArrowUp => ImGuiKey.UpArrow,
            Key.LeftControl => ImGuiKey.LeftCtrl,
            Key.LeftShift => ImGuiKey.LeftShift,
            Key.LeftAlt => ImGuiKey.LeftAlt,
            Key.LeftMeta => ImGuiKey.LeftSuper,
            Key.RightControl => ImGuiKey.RightCtrl,
            Key.RightShift => ImGuiKey.RightShift,
            Key.RightAlt => ImGuiKey.RightAlt,
            Key.RightMeta => ImGuiKey.RightSuper,
            _ => ImGuiKey.None
        };
    }
}