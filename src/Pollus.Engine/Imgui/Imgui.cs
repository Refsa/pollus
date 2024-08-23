namespace Pollus.Engine.Imgui;

using ImGuiNET;
using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Engine.Platform;
using Pollus.Engine.Rendering;
using Pollus.Graphics.Imgui;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Graphics.Windowing;

class ImguiDraw : IRenderPassStageDraw
{
    public RenderPassStage2D Stage => RenderPassStage2D.UI;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var imguiRenderer = resources.Get<ImguiRenderer>();
        imguiRenderer.Render(encoder);
    }
}

public class ImguiPlugin : IPlugin
{
    static ImguiPlugin()
    {
        ResourceFetch<ImguiRenderer>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Init<ImguiRenderer>();

        world.Schedule.AddSystems(CoreStage.Init, SystemBuilder.FnSystem(
            "SetupImgui",
            static (Resources resources, IWGPUContext gpuContext, IWindow window, RenderGraph renderGraph) =>
            {
                var imguiRenderer = new ImguiRenderer(gpuContext, gpuContext.GetSurfaceFormat(), window.Size);
                resources.Add(imguiRenderer);
                renderGraph.Add(new ImguiDraw());
            }
        ));

        world.Schedule.AddSystems(CoreStage.First, SystemBuilder.FnSystem(
            "BeginImguiFrame",
            static (ImguiRenderer imguiRenderer, Time time, IWindow window, PlatformEvents platformEvents) =>
            {
                imguiRenderer.Resized(window.Size);
                var io = ImGui.GetIO();
                foreach (var @event in platformEvents.Events)
                {
                    var evType = (Silk.NET.SDL.EventType)@event.Type;
                    if (evType is Silk.NET.SDL.EventType.Keydown)
                    {
                        io.AddKeyEvent(MapKey(@event.Key.Keysym.Scancode), true);
                    }
                    else if (evType is Silk.NET.SDL.EventType.Keyup)
                    {
                        io.AddKeyEvent(MapKey(@event.Key.Keysym.Scancode), false);
                    }

                    if (evType is Silk.NET.SDL.EventType.Mousebuttondown)
                    {
                        io.AddMouseButtonEvent(@event.Button.Button - 1, true);
                    }
                    else if (evType is Silk.NET.SDL.EventType.Mousebuttonup)
                    {
                        io.AddMouseButtonEvent(@event.Button.Button - 1, false);
                    }

                    if (evType is Silk.NET.SDL.EventType.Mousewheel)
                    {
                        io.AddMouseWheelEvent(@event.Wheel.X, @event.Wheel.Y);
                    }

                    if (evType is Silk.NET.SDL.EventType.Mousemotion)
                    {
                        io.AddMousePosEvent(@event.Motion.X, @event.Motion.Y);
                    }
                }

                imguiRenderer.Update((float)time.DeltaTime);
            }
        ));
    }

    static ImGuiKey MapKey(Silk.NET.SDL.Scancode sdlKey)
    {
        return sdlKey switch
        {
            Silk.NET.SDL.Scancode.ScancodeA => ImGuiKey.A,
            Silk.NET.SDL.Scancode.ScancodeB => ImGuiKey.B,
            Silk.NET.SDL.Scancode.ScancodeC => ImGuiKey.C,
            Silk.NET.SDL.Scancode.ScancodeD => ImGuiKey.D,
            Silk.NET.SDL.Scancode.ScancodeE => ImGuiKey.E,
            Silk.NET.SDL.Scancode.ScancodeF => ImGuiKey.F,
            Silk.NET.SDL.Scancode.ScancodeG => ImGuiKey.G,
            Silk.NET.SDL.Scancode.ScancodeH => ImGuiKey.H,
            Silk.NET.SDL.Scancode.ScancodeI => ImGuiKey.I,
            Silk.NET.SDL.Scancode.ScancodeJ => ImGuiKey.J,
            Silk.NET.SDL.Scancode.ScancodeK => ImGuiKey.K,
            Silk.NET.SDL.Scancode.ScancodeL => ImGuiKey.L,
            Silk.NET.SDL.Scancode.ScancodeM => ImGuiKey.M,
            Silk.NET.SDL.Scancode.ScancodeN => ImGuiKey.N,
            Silk.NET.SDL.Scancode.ScancodeO => ImGuiKey.O,
            Silk.NET.SDL.Scancode.ScancodeP => ImGuiKey.P,
            Silk.NET.SDL.Scancode.ScancodeQ => ImGuiKey.Q,
            Silk.NET.SDL.Scancode.ScancodeR => ImGuiKey.R,
            Silk.NET.SDL.Scancode.ScancodeS => ImGuiKey.S,
            Silk.NET.SDL.Scancode.ScancodeT => ImGuiKey.T,
            Silk.NET.SDL.Scancode.ScancodeU => ImGuiKey.U,
            Silk.NET.SDL.Scancode.ScancodeV => ImGuiKey.V,
            Silk.NET.SDL.Scancode.ScancodeW => ImGuiKey.W,
            Silk.NET.SDL.Scancode.ScancodeX => ImGuiKey.X,
            Silk.NET.SDL.Scancode.ScancodeY => ImGuiKey.Y,
            Silk.NET.SDL.Scancode.ScancodeZ => ImGuiKey.Z,
            Silk.NET.SDL.Scancode.Scancode1 => ImGuiKey._1,
            Silk.NET.SDL.Scancode.Scancode2 => ImGuiKey._2,
            Silk.NET.SDL.Scancode.Scancode3 => ImGuiKey._3,
            Silk.NET.SDL.Scancode.Scancode4 => ImGuiKey._4,
            Silk.NET.SDL.Scancode.Scancode5 => ImGuiKey._5,
            Silk.NET.SDL.Scancode.Scancode6 => ImGuiKey._6,
            Silk.NET.SDL.Scancode.Scancode7 => ImGuiKey._7,
            Silk.NET.SDL.Scancode.Scancode8 => ImGuiKey._8,
            Silk.NET.SDL.Scancode.Scancode9 => ImGuiKey._9,
            Silk.NET.SDL.Scancode.Scancode0 => ImGuiKey._0,
            Silk.NET.SDL.Scancode.ScancodeReturn => ImGuiKey.Enter,
            Silk.NET.SDL.Scancode.ScancodeEscape => ImGuiKey.Escape,
            Silk.NET.SDL.Scancode.ScancodeBackspace => ImGuiKey.Backspace,
            Silk.NET.SDL.Scancode.ScancodeTab => ImGuiKey.Tab,
            Silk.NET.SDL.Scancode.ScancodeSpace => ImGuiKey.Space,
            Silk.NET.SDL.Scancode.ScancodeMinus => ImGuiKey.Minus,
            Silk.NET.SDL.Scancode.ScancodeEquals => ImGuiKey.Equal,
            Silk.NET.SDL.Scancode.ScancodeBackslash => ImGuiKey.Backslash,
            Silk.NET.SDL.Scancode.ScancodeSemicolon => ImGuiKey.Semicolon,
            Silk.NET.SDL.Scancode.ScancodeApostrophe => ImGuiKey.Apostrophe,
            Silk.NET.SDL.Scancode.ScancodeGrave => ImGuiKey.GraveAccent,
            Silk.NET.SDL.Scancode.ScancodeComma => ImGuiKey.Comma,
            Silk.NET.SDL.Scancode.ScancodePeriod => ImGuiKey.Period,
            Silk.NET.SDL.Scancode.ScancodeSlash => ImGuiKey.Slash,
            Silk.NET.SDL.Scancode.ScancodeCapslock => ImGuiKey.CapsLock,
            Silk.NET.SDL.Scancode.ScancodeF1 => ImGuiKey.F1,
            Silk.NET.SDL.Scancode.ScancodeF2 => ImGuiKey.F2,
            Silk.NET.SDL.Scancode.ScancodeF3 => ImGuiKey.F3,
            Silk.NET.SDL.Scancode.ScancodeF4 => ImGuiKey.F4,
            Silk.NET.SDL.Scancode.ScancodeF5 => ImGuiKey.F5,
            Silk.NET.SDL.Scancode.ScancodeF6 => ImGuiKey.F6,
            Silk.NET.SDL.Scancode.ScancodeF7 => ImGuiKey.F7,
            Silk.NET.SDL.Scancode.ScancodeF8 => ImGuiKey.F8,
            Silk.NET.SDL.Scancode.ScancodeF9 => ImGuiKey.F9,
            Silk.NET.SDL.Scancode.ScancodeF10 => ImGuiKey.F10,
            Silk.NET.SDL.Scancode.ScancodeF11 => ImGuiKey.F11,
            Silk.NET.SDL.Scancode.ScancodeF12 => ImGuiKey.F12,
            Silk.NET.SDL.Scancode.ScancodePrintscreen => ImGuiKey.PrintScreen,
            Silk.NET.SDL.Scancode.ScancodeScrolllock => ImGuiKey.ScrollLock,
            Silk.NET.SDL.Scancode.ScancodePause => ImGuiKey.Pause,
            Silk.NET.SDL.Scancode.ScancodeInsert => ImGuiKey.Insert,
            Silk.NET.SDL.Scancode.ScancodeHome => ImGuiKey.Home,
            Silk.NET.SDL.Scancode.ScancodePageup => ImGuiKey.PageUp,
            Silk.NET.SDL.Scancode.ScancodeDelete => ImGuiKey.Delete,
            Silk.NET.SDL.Scancode.ScancodeEnd => ImGuiKey.End,
            Silk.NET.SDL.Scancode.ScancodePagedown => ImGuiKey.PageDown,
            Silk.NET.SDL.Scancode.ScancodeRight => ImGuiKey.RightArrow,
            Silk.NET.SDL.Scancode.ScancodeLeft => ImGuiKey.LeftArrow,
            Silk.NET.SDL.Scancode.ScancodeDown => ImGuiKey.DownArrow,
            Silk.NET.SDL.Scancode.ScancodeUp => ImGuiKey.UpArrow,
            Silk.NET.SDL.Scancode.ScancodeNumlockclear => ImGuiKey.NumLock,
            Silk.NET.SDL.Scancode.ScancodeApplication => ImGuiKey.Menu,
            Silk.NET.SDL.Scancode.ScancodePower => ImGuiKey.None,
            _ => throw new ArgumentOutOfRangeException(nameof(sdlKey), sdlKey, null),
        };
    }
}