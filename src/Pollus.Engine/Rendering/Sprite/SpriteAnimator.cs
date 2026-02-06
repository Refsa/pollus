namespace Pollus.Engine.Rendering;

using ECS;
using Pollus.Utils;

[Flags]
public enum SpriteAnimatorFlag
{
    None = 0,
    Loop = 1 << 0,
}

public partial struct SpriteAnimator : IComponent
{
    public Handle<SpriteAnimation> Animation;
    public int CurrentFrame;
    public float Timer;
    public bool Playing;
    public SpriteAnimatorFlag Flags;
}