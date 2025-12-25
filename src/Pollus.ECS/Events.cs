namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Utils;

public class Events
{
    readonly Dictionary<TypeID, IEventQueue> events = [];

    public void InitEvent<TEvent>()
        where TEvent : struct
    {
        var typeId = TypeLookup.ID<TEvent>();
        if (events.ContainsKey(typeId)) return;

        EventWriterFetch<TEvent>.Register();
        EventReaderFetch<TEvent>.Register();
        events.Add(typeId, new EventQueue<TEvent>());
    }

    internal void ClearEvents()
    {
        foreach (var queue in events.Values)
        {
            queue.Clear();
        }
    }

    EventQueue<TEvent> GetQueue<TEvent>()
        where TEvent : struct
    {
        var typeId = TypeLookup.ID<TEvent>();
        if (!events.TryGetValue(typeId, out var queue))
        {
            queue = new EventQueue<TEvent>();
            events.Add(typeId, queue);
        }

        return (EventQueue<TEvent>)queue;
    }

    public EventWriter<TEvent> GetWriter<TEvent>()
        where TEvent : struct
    {
        return GetQueue<TEvent>().GetWriter();
    }

    public EventReader<TEvent>? GetReader<TEvent>()
        where TEvent : struct
    {
        return GetQueue<TEvent>().GetReader();
    }

    public ReadOnlySpan<TEvent> ReadEvents<TEvent>()
        where TEvent : struct
    {
        return GetQueue<TEvent>().Events;
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
    int prevEnd = 0;
    TEvent[] events = new TEvent[16];

    readonly List<EventReader<TEvent>> readers = [];

    public ReadOnlySpan<TEvent> Events => events.AsSpan()[..cursor];
    public int Count => cursor;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void AddEvent(in TEvent e)
    {
        if (cursor >= events.Length) Array.Resize(ref events, events.Length * 2);
        events[cursor++] = e;
    }

    public void Clear()
    {
        if (cursor == 0) return;
        Array.Clear(events, 0, prevEnd);
        Array.Copy(events, prevEnd, events, 0, cursor - prevEnd);

        foreach (var reader in readers)
        {
            reader.Cursor = int.Max(0, reader.Cursor - prevEnd);
        }

        cursor -= prevEnd;
        prevEnd = cursor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public EventReader<TEvent> GetReader()
    {
        var reader = new EventReader<TEvent>(this);
        readers.Add(reader);
        return reader;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void RemoveReader(EventReader<TEvent> reader)
    {
        readers.Remove(reader);
    }

    public EventWriter<TEvent> GetWriter() => new(this);
}

public struct EventWriter<TEvent>
    where TEvent : struct
{
    static EventWriter()
    {
        EventWriterFetch<TEvent>.Register();
    }

    readonly EventQueue<TEvent> queue;

    public EventWriter(EventQueue<TEvent> queue)
    {
        this.queue = queue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write(in TEvent e) => queue.AddEvent(e);
}

public class EventReader<TEvent> : IDisposable
    where TEvent : struct
{
    static EventReader()
    {
        EventReaderFetch<TEvent>.Register();
    }

    readonly EventQueue<TEvent> queue;
    internal int Cursor { get; set; }

    public bool HasAny => Count > 0;
    public int Count => queue.Count - Cursor;

    public EventReader(EventQueue<TEvent> queue)
    {
        this.queue = queue;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        queue.RemoveReader(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ReadOnlySpan<TEvent> Read()
    {
        var data = Peek();
        Cursor += data.Length;
        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ReadOnlySpan<TEvent> Peek()
    {
        if (HasAny is false) return ReadOnlySpan<TEvent>.Empty;
        return queue.Events[Cursor..queue.Count];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Consume()
    {
        Cursor = queue.Count;
    }
}

public class EventWriterFetch<TEvent> : IFetch<EventWriter<TEvent>>
    where TEvent : struct
{
    public static void Register()
    {
        Fetch.Register(new EventWriterFetch<TEvent>(), [typeof(EventWriter<TEvent>)]);
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
        if (system.Resources.TryGet<EventReader<TEvent>>(out var reader))
        {
            return reader;
        }

        reader = world.Events.GetReader<TEvent>();
        if (reader is null)
        {
            throw new InvalidOperationException($"Event {typeof(TEvent).AssemblyQualifiedName} is not initialized");
        }

        system.Resources.Add(reader);
        return reader;
    }
}

public class EventRunCriteria<TEvent> : IRunCriteria
    where TEvent : struct
{
    public static EventRunCriteria<TEvent> Create => new();

    public bool ShouldRun(World world)
    {
        var events = world.Events.ReadEvents<TEvent>();
        return events.Length > 0;
    }
}
