namespace Pollus.Emscripten;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Utils;

public static partial class Emscripten
{
    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_main_loop(nint action, int fps, [MarshalAs(UnmanagedType.I1)] bool simulateInfiniteLoop);
    
    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_main_loop_arg(nint action, nint args, int fps, [MarshalAs(UnmanagedType.I1)] bool simulateInfiniteLoop);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_pause_main_loop();

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_resume_main_loop();

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_request_animation_frame(nint callback, nint userData);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_set_timeout(nint callback, double ms, nint userData);

    [LibraryImport("__Internal_emscripten")]
    private static partial void emscripten_sleep(double ms);

    [LibraryImport("__Internal_emscripten")]
    private static partial int emscripten_sample_gamepad_data();

    [LibraryImport("__Internal_emscripten")]
    unsafe private static partial void emscripten_console_log(byte* message);

    [LibraryImport("__Internal_emscripten")]
    unsafe private static partial void emscripten_request_animation_frame_loop(nint callback, nint userData);

    [Conditional("BROWSER")]
    unsafe public static void SetMainLoop(delegate* unmanaged[Cdecl]<void> action, int fps, bool simulateInfiniteLoop)
    {
        emscripten_set_main_loop((nint)action, fps, simulateInfiniteLoop);
    }

    [Conditional("BROWSER")]
    unsafe public static void SetMainLoop(delegate* unmanaged[Cdecl]<void*, void> action, void* args, int fps, bool simulateInfiniteLoop)
    {
        emscripten_set_main_loop_arg((nint)action, (nint)args, fps, simulateInfiniteLoop);
    }

    [Conditional("BROWSER")]
    unsafe public static void ResumeMainLoop()
    {
        emscripten_resume_main_loop();
    }

    [Conditional("BROWSER")]
    unsafe public static void PauseMainLoop()
    {
        emscripten_pause_main_loop();
    }

    [Conditional("BROWSER")]
    unsafe public static void ClearMainLoop()
    {
        emscripten_set_main_loop((nint)null, 0, false);
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
    unsafe public static void RequestAnimationFrameLoop(delegate* unmanaged[Cdecl]<double, void*, int> callback, void* userData)
    {
        emscripten_request_animation_frame_loop((nint)callback, (nint)userData);
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

    [Conditional("BROWSER")]
    unsafe public static void ConsoleLog(string message)
    {
        using var messagePtr = TemporaryPin.PinString(message);
        emscripten_console_log((byte*)messagePtr.Ptr);
    }
}