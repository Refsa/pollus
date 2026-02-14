namespace Pollus.Examples;

using ECS;
using Engine;
using Pollus.Assets;
using Engine.Camera;
using Pollus.Input;
using Pollus.Input;
using Engine.Rendering;
using Engine.Transform;
using Graphics.Rendering;
using Mathematics;
using Utils;

public partial class SpriteAnimationExample : IExample
{
    public string Name => "sprite-animation";

    partial struct AnimClipRefs() : IComponent
    {
        public Entity Idle = Entity.Null;
        public Entity Walk = Entity.Null;
        public Entity Attack = Entity.Null;
    }

    IApplication? app;

    public void Run() => (app = Application.Builder
            .AddPlugins([
                SpritePlugin.Default,
                AssetPlugin.Default,
                InputPlugin.Default,
            ])
            .AddSystems(CoreStage.PostInit, FnSystem.Create("Init",
                static (Commands commands, AssetServer assetServer,
                    Assets<SpriteMaterial> materials,
                    Assets<SpriteAnimation> spriteAnimations,
                    Assets<TextureAtlas> atlases,
                    EventWriter<SpriteAnimatorEvents.ClipChangeRequest> eClipChange
                ) =>
                {
                    commands.Spawn(Camera2D.Bundle);

                    var sheet = assetServer.LoadAsync<Texture2D>("sprites/test_sheet.png");
                    var atlas = TextureAtlas.FromGrid("Atlas1", sheet, 5, 4, new Vec2<int>(16, 16));
                    var atlasHandle = atlases.Add(atlas);

                    var material = materials.Add(new SpriteMaterial()
                    {
                        ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/sprite.wgsl"),
                        Texture = sheet,
                        Sampler = assetServer.Load<SamplerAsset>("internal://samplers/nearest"),
                    });

                    var idleAnim = spriteAnimations.Add(SpriteAnimation.From(
                        "Idle", atlasHandle, atlas, 4, [0, 1, 2, 3]
                    ));

                    var walkAnim = spriteAnimations.Add(SpriteAnimation.From(
                        "Walk", atlasHandle, atlas, 8, [5, 6, 7, 8, 9]
                    ));

                    var attackAnim = spriteAnimations.Add(SpriteAnimation.From(
                        "Attack", atlasHandle, atlas, 10, [10, 11, 12, 13, 14]
                    ));

                    var idleClip = commands.Spawn(Entity.With(new SpriteAnimatorClip
                    {
                        Animation = idleAnim,
                        Flags = SpriteAnimatorFlag.Loop,
                        Direction = 1,
                    })).Entity;

                    var walkClip = commands.Spawn(Entity.With(new SpriteAnimatorClip
                    {
                        Animation = walkAnim,
                        Flags = SpriteAnimatorFlag.PingPong | SpriteAnimatorFlag.Loop,
                        Direction = 1,
                    })).Entity;

                    var attackClip = commands.Spawn(Entity.With(new SpriteAnimatorClip
                    {
                        Animation = attackAnim,
                        Flags = SpriteAnimatorFlag.OneShot,
                        Direction = 1,
                    })).Entity;

                    var animatorEntity = commands.Spawn(Entity
                            .With(new Sprite()
                            {
                                Material = material,
                                Slice = atlas.GetRect(0),
                                Color = Color.WHITE,
                            })
                            .With(Transform2D.Default with
                            {
                                Position = Vec2f.One * 256f,
                                Scale = Vec2f.One * 4f,
                            })
                            .With(new SpriteAnimator()
                            {
                                PlaybackSpeed = 1f,
                                Playing = true,
                            })
                            .With(new AnimClipRefs()
                            {
                                Idle = idleClip,
                                Walk = walkClip,
                                Attack = attackClip,
                            }))
                        .AddChild(idleClip)
                        .AddChild(walkClip)
                        .AddChild(attackClip)
                        .Entity;

                    eClipChange.Write(new SpriteAnimatorEvents.ClipChangeRequest
                    {
                        Animator = animatorEntity,
                        NewClip = idleClip,
                    });
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create("HandleInput",
                static (EventReader<ButtonEvent<Key>> eKeys,
                    EventWriter<SpriteAnimatorEvents.ClipChangeRequest> eClipChange,
                    Query<SpriteAnimator, AnimClipRefs> qAnimators
                ) =>
                {
                    Key? clipKey = null;
                    float speedDelta = 0f;

                    foreach (var key in eKeys.Read())
                    {
                        if (key.State != ButtonState.JustPressed) continue;

                        switch (key.Button)
                        {
                            case Key.Digit1:
                            case Key.Digit2:
                            case Key.Digit3:
                                clipKey = key.Button;
                                break;
                            case Key.ArrowUp: speedDelta += 0.25f; break;
                            case Key.ArrowDown: speedDelta -= 0.25f; break;
                        }
                    }

                    qAnimators.ForEach((clipKey, speedDelta, eClipChange),
                        static (in userData, in entity, ref animator, ref refs) =>
                        {
                            if (userData.clipKey is not null)
                            {
                                var target = userData.clipKey switch
                                {
                                    Key.Digit1 => refs.Idle,
                                    Key.Digit2 => refs.Walk,
                                    Key.Digit3 => refs.Attack,
                                    _ => Entity.Null,
                                };
                                if (!target.IsNull && target != animator.Current)
                                {
                                    userData.eClipChange.Write(new SpriteAnimatorEvents.ClipChangeRequest
                                    {
                                        Animator = entity,
                                        NewClip = target,
                                    });
                                }
                            }

                            if (userData.speedDelta != 0f)
                            {
                                animator.PlaybackSpeed = Math.Max(0f, animator.PlaybackSpeed + userData.speedDelta);
                            }
                        });
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create(new SystemBuilderDescriptor("OnClipEnded")
                {
                    RunsAfter = ["SpriteAnimationSystems::TickSpriteAnimation"],
                },
                static (EventReader<SpriteAnimatorEvents.ClipEnded> eClipEnded,
                    EventWriter<SpriteAnimatorEvents.ClipChangeRequest> eClipChange,
                    Query q
                ) =>
                {
                    foreach (var e in eClipEnded.Read())
                    {
                        if (!q.Has<SpriteAnimator>(e.Animator) || !q.Has<AnimClipRefs>(e.Animator)) continue;

                        ref var refs = ref q.Get<AnimClipRefs>(e.Animator);

                        eClipChange.Write(new SpriteAnimatorEvents.ClipChangeRequest
                        {
                            Animator = e.Animator,
                            NewClip = refs.Idle,
                        });
                    }
                }))
            .Build()
        ).Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}
