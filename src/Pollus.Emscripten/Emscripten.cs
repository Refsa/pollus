namespace Pollus.Emscripten;

using System.Diagnostics;
using System.Runtime.InteropServices;

public static partial class Emscripten
{
    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_main_loop(nint action, int fps, [MarshalAs(UnmanagedType.I1)] bool simulateInfiniteLoop);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_request_animation_frame(nint callback, nint userData);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_timeout(nint callback, double ms, nint userData);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_sleep(double ms);

    [LibraryImport("__Internal_emscripten")]
    private static partial int emscripten_sample_gamepad_data();

    [Conditional("BROWSER")]
    unsafe public static void SetMainLoop(delegate* unmanaged[Cdecl]<void> action, int fps, bool simulateInfiniteLoop)
    {
        emscripten_set_main_loop((nint)action, fps, simulateInfiniteLoop);
    }

    [Conditional("BROWSER")]
    unsafe public static void RequestAnimationFrame(delegate* unmanaged[Cdecl]<double, void*, void> callback, void* userData)
    {
        emscripten_request_animation_frame((nint)callback, (nint)userData);
    }

    [Conditional("BROWSER")]
    unsafe public static void SetTimeout(delegate* unmanaged[Cdecl]<void*, void> callback, void* userData, double ms)
    {
        emscripten_set_timeout((nint)callback, ms, (nint)userData);
    }

    [Conditional("BROWSER")]
    public static void SampleGamepadData()
    {
        emscripten_sample_gamepad_data();
    }

    [Conditional("BROWSER")]
    public static void Sleep(double ms)
    {
        emscripten_sleep(ms);
    }
}