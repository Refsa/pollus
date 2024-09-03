namespace Pollus.Generators;

public static class DebugUtils
{
    public static void WaitForDebugger()
    {
        while (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Threading.Thread.Sleep(100);
        }
    }
}