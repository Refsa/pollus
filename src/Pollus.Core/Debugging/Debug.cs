namespace Pollus.Debugging;

public static class Debug
{
    public static void WaitForDebugger()
    {
        while (!System.Diagnostics.Debugger.IsAttached)
        {
            Thread.Sleep(10);
        }
    }
}