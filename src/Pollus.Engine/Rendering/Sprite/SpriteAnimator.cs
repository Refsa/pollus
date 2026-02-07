namespace Pollus.Engine.Rendering;

using ECS;
using Pollus.Utils;

[Flags]
public enum SpriteAnimatorFlag
{
    None = 0,
    Loop = 1 << 0,
    PingPong = 1 << 1,
    OneShot = 1 << 2,
}

public partial struct SpriteAnimator() : IComponent
{
    public Entity Current = Entity.Null;
    public float PlaybackSpeed;
    public bool Playing;
}

public partial struct SpriteAnimatorClip : IComponent
{
    public Handle<SpriteAnimation> Animation;
    public SpriteAnimatorFlag Flags;
    public int CurrentFrame;
    public float Timer;
    public int Direction;
    public bool Playing;
}

public static class SpriteAnimatorEvents
{
    public struct ClipStarted
    {
        public Entity Animator;
    }

    public struct ClipEnded
    {
        public Entity Animator;
    }

    public struct ClipFrame
    {
        public Entity Animator;
        public int Frame;
    }

    public struct ClipChangeRequest
    {
        public Entity Animator;
        public Entity NewClip;
    }
}
