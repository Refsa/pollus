namespace Pollus.Engine.Rendering;

using ECS;
using Pollus.Assets;

[SystemSet]
public partial class SpriteAnimationSystems
{
    [System(nameof(HandleClipTransitions))]
    public static readonly SystemBuilderDescriptor HandleClipTransitionsDescriptor = new()
    {
        Stage = CoreStage.Update,
        RunCriteria = EventRunCriteria<SpriteAnimatorEvents.ClipChangeRequest>.Create,
    };

    [System(nameof(TickSpriteAnimation))]
    public static readonly SystemBuilderDescriptor TickSpriteAnimationDescriptor = new()
    {
        Stage = CoreStage.Update,
        RunsAfter = ["SpriteAnimationSystems::HandleClipTransitions"],
    };

    static void HandleClipTransitions(
        World world,
        Assets<SpriteAnimation> animations,
        EventReader<SpriteAnimatorEvents.ClipChangeRequest> eClipChange,
        EventWriter<SpriteAnimatorEvents.ClipStarted> eClipStarted)
    {
        var store = world.Store;

        foreach (var request in eClipChange.Read())
        {
            if (!store.EntityExists(request.Animator)) continue;
            if (!store.HasComponent<SpriteAnimator>(request.Animator)) continue;

            ref var animator = ref store.GetComponent<SpriteAnimator>(request.Animator);

            if (!animator.Current.IsNull
                && store.EntityExists(animator.Current)
                && store.HasComponent<SpriteAnimatorClip>(animator.Current))
            {
                ref var oldClip = ref store.GetComponent<SpriteAnimatorClip>(animator.Current);
                oldClip.CurrentFrame = 0;
                oldClip.Timer = 0;
                oldClip.Playing = false;
            }

            animator.Current = request.NewClip;

            if (!request.NewClip.IsNull
                && store.EntityExists(request.NewClip)
                && store.HasComponent<SpriteAnimatorClip>(request.NewClip))
            {
                ref var newClip = ref store.GetComponent<SpriteAnimatorClip>(request.NewClip);
                newClip.CurrentFrame = 0;
                newClip.Direction = 1;
                newClip.Playing = true;

                var anim = animations.Get(newClip.Animation);
                if (anim is not null && anim.Frames.Length > 0)
                {
                    newClip.Timer = anim.Frames[0].Duration;

                    if (store.HasComponent<Sprite>(request.Animator))
                    {
                        ref var sprite = ref store.GetComponent<Sprite>(request.Animator);
                        sprite.Slice = anim.Frames[0].Rect;
                    }
                }

                eClipStarted.Write(new SpriteAnimatorEvents.ClipStarted { Animator = request.Animator });
            }
            else if (!request.NewClip.IsNull && !store.EntityExists(request.NewClip))
            {
                animator.Current = Entity.Null;
            }
        }
    }

    static void TickSpriteAnimation(
        Time time,
        World world,
        Query<SpriteAnimator, Sprite> qAnims,
        Assets<SpriteAnimation> animations,
        EventWriter<SpriteAnimatorEvents.ClipEnded> eClipEnded,
        EventWriter<SpriteAnimatorEvents.ClipFrame> eClipFrame)
    {
        if (time.DeltaTimeF <= 0f) return;

        qAnims.ForEach((world, animations, time.DeltaTimeF, eClipEnded, eClipFrame),
            static (in userData, in entity, ref animator, ref sprite) =>
            {
                if (!animator.Playing || animator.Current.IsNull) return;

                var store = userData.world.Store;

                if (!store.EntityExists(animator.Current))
                {
                    animator.Current = Entity.Null;
                    return;
                }

                if (!store.HasComponent<SpriteAnimatorClip>(animator.Current)) return;

                ref var clip = ref store.GetComponent<SpriteAnimatorClip>(animator.Current);
                if (!clip.Playing) return;

                var anim = userData.animations.Get(clip.Animation);
                if (anim is null || anim.Frames.Length == 0) return;

                var frameCount = anim.Frames.Length;
                clip.Timer -= userData.DeltaTimeF * animator.PlaybackSpeed;

                if (clip.Timer > 0f)
                {
                    sprite.Slice = anim.Frames[clip.CurrentFrame].Rect;
                    return;
                }

                var nextFrame = clip.CurrentFrame + clip.Direction;

                if (nextFrame < 0 || nextFrame >= frameCount)
                {
                    if (clip.Flags.HasFlag(SpriteAnimatorFlag.PingPong))
                    {
                        clip.Direction *= -1;
                        if (!clip.Flags.HasFlag(SpriteAnimatorFlag.Loop) && nextFrame < 0)
                        {
                            clip.Playing = false;
                            userData.eClipEnded.Write(new SpriteAnimatorEvents.ClipEnded { Animator = entity });
                            return;
                        }

                        nextFrame = clip.CurrentFrame + clip.Direction;
                    }
                    else if (clip.Flags.HasFlag(SpriteAnimatorFlag.Loop))
                    {
                        nextFrame = 0;
                    }
                    else
                    {
                        clip.Playing = false;
                        userData.eClipEnded.Write(new SpriteAnimatorEvents.ClipEnded { Animator = entity });
                        return;
                    }
                }

                clip.CurrentFrame = nextFrame;
                clip.Timer += anim.Frames[nextFrame].Duration;
                userData.eClipFrame.Write(new SpriteAnimatorEvents.ClipFrame { Animator = entity, Frame = nextFrame });
                sprite.Slice = anim.Frames[clip.CurrentFrame].Rect;
            });
    }
}
