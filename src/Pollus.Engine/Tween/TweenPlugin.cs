namespace Pollus.Engine.Tween;

using System.Runtime.InteropServices;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine.Reflect;
using Pollus.Mathematics;

public class TweenPlugin : IPlugin
{
    static TweenPlugin()
    {
        TweenResources.RegisterHandler<float, FloatHandler>();
        TweenResources.RegisterHandler<Vec2f, Vec2fHandler>();
    }

    List<IPlugin> plugins = [];

    public void Apply(World world)
    {
        world.Resources.Add(new TweenResources());
        foreach (var plugin in plugins)
        {
            plugin.Apply(world);
        }

        world.Schedule.AddSystemSet<TweenSequenceSystemSet>();

        world.Schedule.AddSystems(CoreStage.PostUpdate, FnSystem.Create("TweenTick",
        static (Commands commands, Time time, Query<TweenData>.Filter<None<TweenDisabled>> qTweens) =>
        {
            qTweens.ForEach((time.DeltaTimeF, commands),
            static (in (float deltaTime, Commands commands) userData, in Entity entity, ref TweenData tween) =>
            {
                if (tween.Elapsed < tween.Duration)
                {
                    tween.Elapsed = (tween.Elapsed + userData.deltaTime).Clamp(0f, tween.Duration);

                    var t = tween.Elapsed / tween.Duration;
                    tween.Progress = tween.Easing switch
                    {
                        Easing.Linear => t,
                        Easing.Sine => float.Sin(t),
                        Easing.Quadratic => t * t,
                        Easing.Cubic => t * t * t,
                        Easing.Quartic => t * t * t * t,
                        Easing.Quintic => t * t * t * t * t,
                        _ => t,
                    };
                    return;
                }

                tween.Elapsed = 0f;
                tween.Progress = 0f;

                if (tween.Flags.HasFlag(TweenFlag.OneShot))
                {
                    userData.commands.Despawn(entity);
                }
                else if (!tween.Flags.HasFlag(TweenFlag.Loop) && !tween.Flags.HasFlag(TweenFlag.PingPong))
                {
                    userData.commands.AddComponent(entity, new TweenDisabled());
                }
            });

        }));
    }

    public TweenPlugin Register<TData>()
        where TData : unmanaged, IComponent, ITweenable, IReflect<TData>
    {
        plugins.Add(new TweenablePlugin<TData>());
        return this;
    }
}

public class TweenablePlugin<TData> : IPlugin
    where TData : unmanaged, IComponent, ITweenable, IReflect<TData>
{
    public void Apply(World world)
    {
        TData.PrepareTweenSystems(world.Schedule);
    }
}

public class TweenSystem<TData, TField> : SystemBase<Commands, Time, Query, Query<Tween<TField>, TweenData, TweenTarget>.Filter<None<TweenDisabled>>, Query<TData>>
    where TData : unmanaged, IComponent, ITweenable, IReflect<TData>
    where TField : unmanaged
{
    public delegate void ApplyDelegate(TData data, TField value);

    public TweenSystem() : base(new($"TweenSystem<{typeof(TData).Name}, {typeof(TField).Name}>"))
    {
    }

    protected override void OnTick(Commands commands, Time time, Query query, Query<Tween<TField>, TweenData, TweenTarget>.Filter<None<TweenDisabled>> qTweens, Query<TData> qData)
    {
        Guard.IsNotNull(TweenResources.Handler<TField>.Instance, $"Tween handler not registered");

        qTweens.ForEach(query, static (in Query query, ref Tween<TField> tween, ref TweenData data, ref TweenTarget target) =>
        {
            var t = data.Progress;
            if (data.Flags.HasFlag(TweenFlag.Reverse)) t = 1f - t;

            var value = TweenResources.Handler<TField>.Instance.Lerp(tween.From, tween.To, t);
            query.Get<TData>(target.Entity).SetValue(tween.FieldID, value);

            if (data.Progress == 1f)
            {
                if (data.Flags.HasFlag(TweenFlag.PingPong))
                {
                    (tween.From, tween.To) = (tween.To, tween.From);
                }
            }
        });
    }
}

class TweenResources
{
    public static class Handler<TType>
        where TType : unmanaged
    {
        static ITweenHandler<TType>? handler;
        public static ITweenHandler<TType>? Instance => handler;

        public static void Set(ITweenHandler<TType> handler) => Handler<TType>.handler = handler;
    }

    public static void RegisterHandler<TType, THandler>()
        where TType : unmanaged
        where THandler : struct, ITweenHandler<TType>
    {
        Handler<TType>.Set(new THandler());
    }
}

/* -------------------------------------------------------------------------------------------- */

public enum Easing : byte
{
    Linear = 0,
    Sine,
    Quadratic,
    Cubic,
    Quartic,
    Quintic,
}

public enum EasingDirection : byte
{
    InOut = 0,
    In,
    Out,
}

[Flags]
public enum TweenFlag
{
    None = 0,
    Loop = 1 << 0,
    PingPong = 1 << 1,
    Reverse = 1 << 2,
    Delayed = 1 << 3,
    OneShot = 1 << 4,
}

/* -------------------------------------------------------------------------------------------- */

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Tween<TType> : IComponent
    where TType : unmanaged
{
    public byte FieldID;
    public TType From;
    public TType To;
}

public struct TweenData : IComponent
{
    public Easing Easing;
    public EasingDirection Direction;
    public TweenFlag Flags;
    public float Duration;

    public float Elapsed;
    public float Progress;
}

public struct TweenTarget : IComponent
{
    public Entity Entity;
}

public struct TweenDisabled : IComponent
{

}

/* -------------------------------------------------------------------------------------------- */

public interface ITweenable
{
    static abstract void PrepareTweenSystems(Schedule schedule);
}