namespace Pollus.Engine;

using Pollus.ECS;
using Pollus.Engine.Platform;
using Pollus.Engine.Window;

public static class DefaultPlugins
{
    public static IPlugin[] Core => [new TimePlugin()];
    public static IPlugin[] Platform => [new PlatformEventsPlugin(), new WindowPlugin()];
    public static IPlugin[] Full => [..Core, ..Platform];
}
