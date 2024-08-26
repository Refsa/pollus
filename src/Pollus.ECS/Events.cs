using Pollus.ECS.Core;

namespace Pollus.ECS;

public class Events
{
    Dictionary<Type, IEventQueue> events = [];

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
            queue?.Clear();
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
    void Clear(bool zero = false);
}

public class EventQueue<TEvent> : IEventQueue
    where TEvent : struct
{
    int cursor = 0;
    TEvent[] events = new TEvent[16];

    public ReadOnlySpan<TEvent> Events => events.AsSpan()[..cursor];

    public void AddEvent(TEvent e)
    {
        if (cursor >= events.Length) Array.Resize(ref events, events.Length * 2);
        events[cursor++] = e;
    }

    public void Clear(bool zero = false)
    {
        if (zero) Array.Clear(events, 0, cursor);
        cursor = 0;
    }

    public EventReader<TEvent> GetReader() => new(this);
    public EventWriter<TEvent> GetWriter() => new(this);
}

public struct EventWriter<TEvent>
    where TEvent : struct
{
    EventQueue<TEvent> queue;

    public EventWriter(EventQueue<TEvent> queue)
    {
        this.queue = queue;
    }

    public void Write(TEvent e) => queue.AddEvent(e);
}

public struct EventReader<TEvent>
    where TEvent : struct
{
    EventQueue<TEvent> queue;

    public EventReader(EventQueue<TEvent> queue)
    {
        this.queue = queue;
    }

    public ReadOnlySpan<TEvent> Read() => queue.Events;
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