namespace Pollus.ECS.Generators;

public static class DebugUtils
{
    public static void WaitForDebugger()
    {
        while (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Threading.Thread.Sleep(100);
        }
    }

    public static void Break()
    {
        System.Diagnostics.Debugger.Break();
    }
}