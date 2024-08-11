namespace Pollus.Emscripten;

using System.Runtime.InteropServices;

public static class Emscripten
{
    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_set_main_loop(nint action, int fps, bool simulateInfiniteLoop);

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_request_animation_frame(nint callback, nint userData);

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_set_timeout(nint callback, double ms, nint userData);

    [DllImport("__Internal_emscripten")]
    private static extern void emscripten_sleep(double ms);

    unsafe public static void SetMainLoop(delegate* unmanaged[Cdecl]<void> action, int fps, bool simulateInfiniteLoop)
    {
        emscripten_set_main_loop((nint)action, fps, simulateInfiniteLoop);
    }

    unsafe public static void RequestAnimationFrame(delegate* unmanaged[Cdecl]<double, void*, void> callback, void* userData)
    {
        emscripten_request_animation_frame((nint)callback, (nint)userData);
    }

    unsafe public static void SetTimeout(delegate* unmanaged[Cdecl]<void*, void> callback, void* userData, double ms)
    {
        emscripten_set_timeout((nint)callback, ms, (nint)userData);
    }

    public static void Sleep(double ms)
    {
        emscripten_sleep(ms);
    }
}