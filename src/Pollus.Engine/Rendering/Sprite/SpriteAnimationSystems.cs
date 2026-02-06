namespace Pollus.Engine.Rendering;

using Debugging;
using ECS;
using Pollus.Engine.Assets;

[SystemSet]
public partial class SpriteAnimationSystems
{
    [System(nameof(PrepareSpriteAnimation))]
    public static readonly SystemBuilderDescriptor PrepareSpriteAnimationDescriptor = new()
    {
        Stage = CoreStage.First,
    };

    [System(nameof(UpdateSpriteAnimation))]
    public static readonly SystemBuilderDescriptor UpdateSpriteAnimationDescriptor = new()
    {
        Stage = CoreStage.Update,
    };

    static void PrepareSpriteAnimation(EventReader<AssetEvent<SpriteAnimation>> eSpriteAnimations, Assets<SpriteAnimation> spriteAnimations, Assets<TextureAtlas> atlases)
    {
        foreach (var e in eSpriteAnimations.Read())
        {
            if (e.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;

            var spriteAnimation = spriteAnimations.Get(e.Handle);
            Guard.IsNotNull(spriteAnimation, "SpriteAnimation is null");
        }
    }

    static void UpdateSpriteAnimation(Time time, Assets<SpriteAnimation> animations, Query<SpriteAnimator, Sprite> qAnims)
    {
        qAnims.ForEach((animations, time.DeltaTimeF), static (in userData, ref animator, ref sprite) =>
        {
            if (!animator.Playing) return;

            animator.Timer -= userData.DeltaTimeF;
            if (animator.Timer > 0f) return;

            var anim = userData.animations.Get(animator.Animation);
            Guard.IsNotNull(anim, "spriteAnimator.Animation was null");

            var nextFrame = animator.CurrentFrame + 1;
            if (nextFrame >= anim.Frames.Length && !animator.Flags.HasFlag(SpriteAnimatorFlag.Loop))
            {
                animator.Playing = false;
                return;
            }

            var frame = anim.Frames[nextFrame % anim.Frames.Length];
            animator.CurrentFrame = nextFrame;
            animator.Timer = frame.Duration;
            sprite.Slice = frame.Rect;
        });
    }
}