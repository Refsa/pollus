#pragma warning disable CA1416
namespace Pollus.Tests.ECS;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Engine.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public class SpriteAnimationTests
{
    static SpriteAnimation CreateAnimation(int frameCount, float frameDuration)
    {
        var frames = new SpriteAnimation.Frame[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            frames[i] = new SpriteAnimation.Frame
            {
                Rect = new Rect { Min = new Vec2f(i * 16, 0), Max = new Vec2f((i + 1) * 16, 16) },
                Duration = frameDuration,
            };
        }
        return new SpriteAnimation
        {
            Name = "TestAnim",
            AtlasHandle = Handle<TextureAtlas>.Null,
            Frames = frames,
        };
    }

    struct TestSetup
    {
        public World World;
        public Assets<SpriteAnimation> Assets;
        public Entity AnimatorEntity;
        public Entity ClipEntity;
        public Handle<SpriteAnimation> AnimHandle;
        public Time Time;
    }

    static TestSetup SetupBasic(SpriteAnimatorFlag flags, int frameCount = 4, float frameDuration = 0.5f)
    {
        var world = new World();
        world.Events.InitEvent<AssetEvent<SpriteAnimation>>();

        var time = new Time();
        world.Resources.Add(time);
        var assetServer = new AssetServer(new FileAssetIO("."));
        world.Resources.Add(assetServer);
        var assets = assetServer.GetAssets<SpriteAnimation>();

        world.AddPlugins(true, [SpriteAnimatorPlugin.Default]);

        var anim = CreateAnimation(frameCount, frameDuration);
        var animHandle = assets.Add(anim);

        var commands = world.GetCommands();
        var clipEntity = commands.Spawn(Entity.With(new SpriteAnimatorClip
        {
            Animation = animHandle,
            Flags = flags,
            Direction = 1,
        })).Entity;

        var animatorEntity = commands.Spawn(Entity.With(
            new Sprite { Material = Handle<SpriteMaterial>.Null, Slice = Rect.Zero, Color = Color.WHITE },
            new SpriteAnimator
            {
                PlaybackSpeed = 1f,
                Playing = true,
            }
        )).AddChild(clipEntity).Entity;

        world.Prepare();

        WriteClipChange(world, animatorEntity, clipEntity);
        time.DeltaTime = 0;
        world.Update();

        return new TestSetup
        {
            World = world,
            Assets = assets,
            AnimatorEntity = animatorEntity,
            ClipEntity = clipEntity,
            AnimHandle = animHandle,
            Time = time,
        };
    }

    static void WriteClipChange(World world, Entity animator, Entity newClip)
    {
        var writer = world.Events.GetWriter<SpriteAnimatorEvents.ClipChangeRequest>();
        writer.Write(new SpriteAnimatorEvents.ClipChangeRequest
        {
            Animator = animator,
            NewClip = newClip,
        });
    }

    static void Tick(TestSetup setup, float deltaTime)
    {
        setup.Time.DeltaTime = deltaTime;
        setup.World.Update();
    }

    [Fact]
    public void ClipTransition_InitializesNewClip()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop);

        ref var animator = ref setup.World.Store.GetComponent<SpriteAnimator>(setup.AnimatorEntity);
        Assert.Equal(setup.ClipEntity, animator.Current);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.True(clip.Playing);
        Assert.Equal(0, clip.CurrentFrame);
        Assert.Equal(1, clip.Direction);
        Assert.Equal(0.5f, clip.Timer);

        setup.World.Dispose();
    }

    [Fact]
    public void ClipTransition_SkipsWhenCurrentUnchanged()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop);

        Tick(setup, 0.6f);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);

        Tick(setup, 0.6f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(2, clip.CurrentFrame);

        setup.World.Dispose();
    }

    [Fact]
    public void ClipTransition_ResetsOldClip_WhenSwitching()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop);

        Tick(setup, 0.6f);
        Tick(setup, 0.6f);

        var anim2 = CreateAnimation(3, 0.25f);
        var animHandle2 = setup.Assets.Add(anim2);
        var commands = setup.World.GetCommands();
        var clip2Entity = commands.Spawn(Entity.With(new SpriteAnimatorClip
        {
            Animation = animHandle2,
            Flags = SpriteAnimatorFlag.OneShot,
            Direction = 1,
        })).Entity;
        commands.AddChild(setup.AnimatorEntity, clip2Entity);

        WriteClipChange(setup.World, setup.AnimatorEntity, clip2Entity);

        Tick(setup, 0);

        ref var clip1 = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(0, clip1.CurrentFrame);
        Assert.Equal(0f, clip1.Timer);
        Assert.False(clip1.Playing);

        ref var clip2 = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(clip2Entity);
        Assert.True(clip2.Playing);
        Assert.Equal(0, clip2.CurrentFrame);
        Assert.Equal(1, clip2.Direction);
        Assert.Equal(0.25f, clip2.Timer);

        setup.World.Dispose();
    }

    [Fact]
    public void ClipTransition_ToNull_StopsAnimation()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop);

        WriteClipChange(setup.World, setup.AnimatorEntity, Entity.Null);

        Tick(setup, 0);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.False(clip.Playing);
        Assert.Equal(0, clip.CurrentFrame);

        ref var animator = ref setup.World.Store.GetComponent<SpriteAnimator>(setup.AnimatorEntity);
        Assert.True(animator.Current.IsNull);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_AdvancesFrame_WhenTimerExpires()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        Tick(setup, 0.3f);
        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(0, clip.CurrentFrame);

        Tick(setup, 0.3f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_UpdatesSpriteSlice()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        var anim = setup.Assets.Get(setup.AnimHandle)!;
        Tick(setup, 0.6f);

        ref var sprite = ref setup.World.Store.GetComponent<Sprite>(setup.AnimatorEntity);
        Assert.Equal(anim.Frames[1].Rect, sprite.Slice);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_Loop_WrapsToBeginning()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 3, frameDuration: 0.5f);

        Tick(setup, 0.6f);
        Tick(setup, 0.6f);
        Tick(setup, 0.6f);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(0, clip.CurrentFrame);
        Assert.True(clip.Playing);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_OneShot_StopsAtEnd()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.OneShot, frameCount: 3, frameDuration: 0.5f);

        Tick(setup, 0.6f);
        Tick(setup, 0.6f);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(2, clip.CurrentFrame);
        Assert.True(clip.Playing);

        Tick(setup, 0.6f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.False(clip.Playing);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_PingPong_ReversesDirection()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.PingPong | SpriteAnimatorFlag.Loop, frameCount: 3, frameDuration: 0.5f);

        // Forward: 0 -> 1 -> 2
        Tick(setup, 0.55f);
        Tick(setup, 0.55f);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(2, clip.CurrentFrame);

        // Reverse: 2 -> 1 -> 0
        Tick(setup, 0.55f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);
        Assert.Equal(-1, clip.Direction);

        Tick(setup, 0.55f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(0, clip.CurrentFrame);

        // Forward again: 0 -> 1
        Tick(setup, 0.55f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);
        Assert.Equal(1, clip.Direction);
        Assert.True(clip.Playing);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_PingPongNoLoop_StopsAfterCycle()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.PingPong, frameCount: 3, frameDuration: 0.5f);

        // Forward: 0 -> 1 -> 2
        Tick(setup, 0.6f);
        Tick(setup, 0.6f);

        // Reverse: 2 -> 1 -> 0
        Tick(setup, 0.6f);
        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);
        Assert.True(clip.Playing);

        Tick(setup, 0.6f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(0, clip.CurrentFrame);
        Assert.True(clip.Playing);

        Tick(setup, 0.6f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.False(clip.Playing);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_CurrentNull_DoesNotCrash()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop);

        WriteClipChange(setup.World, setup.AnimatorEntity, Entity.Null);
        Tick(setup, 0);

        Tick(setup, 0.5f);

        setup.World.Dispose();
    }

    [Fact]
    public void MultipleClips_SwitchBetween()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        Tick(setup, 0.6f);
        Tick(setup, 0.6f);

        var anim2 = CreateAnimation(2, 0.3f);
        var animHandle2 = setup.Assets.Add(anim2);
        var commands = setup.World.GetCommands();
        var clip2Entity = commands.Spawn(Entity.With(new SpriteAnimatorClip
        {
            Animation = animHandle2,
            Flags = SpriteAnimatorFlag.Loop,
            Direction = 1,
        })).Entity;
        commands.AddChild(setup.AnimatorEntity, clip2Entity);

        WriteClipChange(setup.World, setup.AnimatorEntity, clip2Entity);

        Tick(setup, 0);

        ref var clip1 = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(0, clip1.CurrentFrame);
        Assert.False(clip1.Playing);

        ref var clip2 = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(clip2Entity);
        Assert.Equal(0, clip2.CurrentFrame);
        Assert.True(clip2.Playing);
        Assert.Equal(0.3f, clip2.Timer);

        Tick(setup, 0.4f);
        clip2 = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(clip2Entity);
        Assert.Equal(1, clip2.CurrentFrame);

        WriteClipChange(setup.World, setup.AnimatorEntity, setup.ClipEntity);

        Tick(setup, 0);

        clip1 = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.True(clip1.Playing);
        Assert.Equal(0, clip1.CurrentFrame);
        Assert.Equal(0.5f, clip1.Timer);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_DespawnedClipEntity_ClearsCurrentAndDoesNotCrash()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        Tick(setup, 0.6f);
        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);

        setup.World.Despawn(setup.ClipEntity);
        Tick(setup, 0.5f);

        ref var animator = ref setup.World.Store.GetComponent<SpriteAnimator>(setup.AnimatorEntity);
        Assert.True(animator.Current.IsNull);

        setup.World.Dispose();
    }

    [Fact]
    public void ClipTransition_DespawnedClipEntity_ClearsCurrent()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        var anim2 = CreateAnimation(3, 0.25f);
        var animHandle2 = setup.Assets.Add(anim2);
        var commands = setup.World.GetCommands();
        var clip2Entity = commands.Spawn(Entity.With(new SpriteAnimatorClip
        {
            Animation = animHandle2,
            Flags = SpriteAnimatorFlag.Loop,
            Direction = 1,
        })).Entity;
        commands.AddChild(setup.AnimatorEntity, clip2Entity);

        Tick(setup, 0);

        setup.World.Despawn(clip2Entity);

        WriteClipChange(setup.World, setup.AnimatorEntity, clip2Entity);

        Tick(setup, 0);

        ref var animator = ref setup.World.Store.GetComponent<SpriteAnimator>(setup.AnimatorEntity);
        Assert.True(animator.Current.IsNull);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_TimerCarryOver_PreservesOvershootTime()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 1.0f);

        Tick(setup, 1.3f);
        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);
        Assert.Equal(0.7f, clip.Timer, 0.001f);

        Tick(setup, 0.6f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);
        Assert.Equal(0.1f, clip.Timer, 0.001f);

        Tick(setup, 0.2f);
        clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(2, clip.CurrentFrame);
        Assert.Equal(0.9f, clip.Timer, 0.001f);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_LargeDelta_AdvancesOneFrameAndCarriesTime()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        Tick(setup, 1.6f);

        ref var clip = ref setup.World.Store.GetComponent<SpriteAnimatorClip>(setup.ClipEntity);
        Assert.Equal(1, clip.CurrentFrame);

        setup.World.Dispose();
    }

    [Fact]
    public void ClipTransition_EmitsClipStarted()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop);

        var anim2 = CreateAnimation(3, 0.5f);
        var animHandle2 = setup.Assets.Add(anim2);
        var commands = setup.World.GetCommands();
        var clip2Entity = commands.Spawn(Entity.With(new SpriteAnimatorClip
        {
            Animation = animHandle2,
            Flags = SpriteAnimatorFlag.Loop,
            Direction = 1,
        })).Entity;
        commands.AddChild(setup.AnimatorEntity, clip2Entity);

        WriteClipChange(setup.World, setup.AnimatorEntity, clip2Entity);

        var reader = setup.World.Events.GetReader<SpriteAnimatorEvents.ClipStarted>()!;

        Tick(setup, 0);

        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(setup.AnimatorEntity, events[0].Animator);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_OneShot_EmitsClipEnded()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.OneShot, frameCount: 3, frameDuration: 0.5f);

        Tick(setup, 0.6f); // frame 1
        Tick(setup, 0.6f); // frame 2

        var reader = setup.World.Events.GetReader<SpriteAnimatorEvents.ClipEnded>()!;

        Tick(setup, 0.6f); // stops

        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(setup.AnimatorEntity, events[0].Animator);

        setup.World.Dispose();
    }

    [Fact]
    public void Tick_EmitsClipFrameOnAdvance()
    {
        var setup = SetupBasic(SpriteAnimatorFlag.Loop, frameCount: 4, frameDuration: 0.5f);

        var reader = setup.World.Events.GetReader<SpriteAnimatorEvents.ClipFrame>()!;

        Tick(setup, 0.6f);

        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(1, events[0].Frame);

        setup.World.Dispose();
    }

    [Fact]
    public void Integration_SchedulerDriven_AdvancesFramesAndEmitsEvents()
    {
        var world = new World();
        world.Events.InitEvent<AssetEvent<SpriteAnimation>>();

        var time = new Time();
        world.Resources.Add(time);
        var assetServer = new AssetServer(new FileAssetIO("."));
        world.Resources.Add(assetServer);
        var assets = assetServer.GetAssets<SpriteAnimation>();

        world.AddPlugins(true, [SpriteAnimatorPlugin.Default]);

        var anim = CreateAnimation(3, 0.5f);
        var animHandle = assets.Add(anim);

        var commands = world.GetCommands();
        var clipEntity = commands.Spawn(Entity.With(new SpriteAnimatorClip
        {
            Animation = animHandle,
            Flags = SpriteAnimatorFlag.OneShot,
            Direction = 1,
        })).Entity;

        var animatorEntity = commands.Spawn(Entity.With(
            new Sprite { Material = Handle<SpriteMaterial>.Null, Slice = Rect.Zero, Color = Color.WHITE },
            new SpriteAnimator
            {
                PlaybackSpeed = 1f,
                Playing = true,
            }
        )).AddChild(clipEntity).Entity;

        world.Prepare();

        WriteClipChange(world, animatorEntity, clipEntity);
        time.DeltaTime = 0;
        world.Update();

        ref var clip = ref world.Store.GetComponent<SpriteAnimatorClip>(clipEntity);
        Assert.True(clip.Playing);
        Assert.Equal(0, clip.CurrentFrame);

        time.DeltaTime = 0.6;
        world.Update();
        clip = ref world.Store.GetComponent<SpriteAnimatorClip>(clipEntity);
        Assert.Equal(1, clip.CurrentFrame);

        time.DeltaTime = 0.6;
        world.Update();
        clip = ref world.Store.GetComponent<SpriteAnimatorClip>(clipEntity);
        Assert.Equal(2, clip.CurrentFrame);

        var reader = world.Events.GetReader<SpriteAnimatorEvents.ClipEnded>()!;

        time.DeltaTime = 0.6;
        world.Update();

        clip = ref world.Store.GetComponent<SpriteAnimatorClip>(clipEntity);
        Assert.False(clip.Playing);

        var events = reader.Read();
        Assert.Equal(1, events.Length);
        Assert.Equal(animatorEntity, events[0].Animator);

        world.Dispose();
    }
}
#pragma warning restore CA1416
