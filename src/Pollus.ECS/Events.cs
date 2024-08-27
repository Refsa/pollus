using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.ECS.Core;

namespace Pollus.ECS;

public class Events
{
    readonly Dictionary<Type, IEventQueue> events = [];

    public void InitEvent<TEvent>()
        where TEvent : struct
    {
        EventWriterFetch<TEvent>.Register();
        EventReaderFetch<TEvent>.Register();
        events.Add(typeof(TEvent), new EventQueue<TEvent>());
    }

    public void ClearEvents()
    {
        foreach (var queue in events.Values)
        {
            queue.Clear();
        }
    }

    public EventWriter<TEvent> GetWriter<TEvent>()
        where TEvent : struct
    {
        if (events.TryGetValue(typeof(TEvent), out var queue))
        {
            return ((EventQueue<TEvent>)queue).GetWriter();
        }
        return default;
    }

    public EventReader<TEvent> GetReader<TEvent>()
        where TEvent : struct
    {
        if (events.TryGetValue(typeof(TEvent), out var queue))
        {
            return ((EventQueue<TEvent>)queue).GetReader();
        }
        return default;
    }
}

public interface IEventQueue
{
    void Clear();
}

public class EventQueue<TEvent> : IEventQueue
    where TEvent : struct
{
    int cursor = 0;
    TEvent[] events = new TEvent[16];

    public ReadOnlySpan<TEvent> Events => events.AsSpan()[..cursor];
    public int Count => cursor;

    public void AddEvent(in TEvent e)
    {
        if (cursor >= events.Length) Array.Resize(ref events, events.Length * 2);
        events[cursor++] = e;
    }

    public void Clear()
    {
        if (cursor == 0) return;

        // var bytes = MemoryMarshal.AsBytes(events.AsSpan());
        // Unsafe.InitBlock(ref bytes[0], 0, (uint)(bytes.Length * Unsafe.SizeOf<TEvent>()));
        Array.Fill(events, default);

        cursor = 0;
    }

    public EventReader<TEvent> GetReader() => new(this);
    public EventWriter<TEvent> GetWriter() => new(this);
}

public struct EventWriter<TEvent>
    where TEvent : struct
{
    readonly EventQueue<TEvent> queue;

    public EventWriter(EventQueue<TEvent> queue)
    {
        this.queue = queue;
    }

    public void Write(in TEvent e) => queue.AddEvent(e);
}

public readonly struct EventReader<TEvent>
    where TEvent : struct
{
    readonly EventQueue<TEvent> queue;

    public readonly bool HasAny => Count > 0;
    public readonly int Count => queue.Count;

    public EventReader(EventQueue<TEvent> queue)
    {
        this.queue = queue;
    }

    public readonly ReadOnlySpan<TEvent> Read() => queue.Events;
}

public class EventWriterFetch<TEvent> : IFetch<EventWriter<TEvent>>
    where TEvent : struct
{
    public static void Register()
    {
        Fetch.Register(new EventWriterFetch<TEvent>(), []);
    }

    public EventWriter<TEvent> DoFetch(World world, ISystem system)
    {
        return world.Events.GetWriter<TEvent>();
    }
}

public class EventReaderFetch<TEvent> : IFetch<EventReader<TEvent>>
    where TEvent : struct
{
    public static void Register()
    {
        Fetch.Register(new EventReaderFetch<TEvent>(), []);
    }

    public EventReader<TEvent> DoFetch(World world, ISystem system)
    {
        return world.Events.GetReader<TEvent>();
    }
}