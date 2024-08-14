namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.ECS;

public struct AudioSource : IComponent
{
    public bool Playing;
    public bool Looping;
    public float Gain;
    public float Pitch;
}

public struct AudioPlayback : IComponent
{
    
}

public class AudioPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(new AudioManager());
    }
}