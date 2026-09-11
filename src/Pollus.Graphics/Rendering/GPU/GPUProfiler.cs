namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.Platform;
using Pollus.Graphics.WGPU;
using System.Diagnostics;

public readonly record struct GPUTimestampWrites(NativeHandle<QuerySetTag> QuerySet, uint BeginIndex, uint EndIndex);

public sealed class GPUProfiler : IDisposable
{
    const int MaxPasses = 128;

    sealed class FrameSlot
    {
        public GPUQuerySet? Queries;
        public GPUBuffer? ResolveBuffer;
        public readonly string[] Labels = new string[MaxPasses];
        public int Count;
        public int Generation;
        public GPUReadbackHandle Readback;
        public bool Pending;
        public bool Truncated;
    }

    readonly IWGPUContext context;
    readonly FrameSlot[] slots = [new(), new(), new()];
    readonly GPUPassTiming[] results = new GPUPassTiming[MaxPasses];
    readonly Dictionary<string, string> labels = new(StringComparer.Ordinal);
    FrameSlot? recording;
    bool enabled;
    int generation;

    public bool Supported { get; }
    public double TimestampPeriodNanoseconds { get; }
    public GPUProfilerTimings Timings { get; } = new();
    public int DroppedFrames { get; private set; }
    public bool Enabled
    {
        get => enabled;
        set
        {
            if (enabled == value) return;
            enabled = value;
            generation++;
            recording = null;
            DroppedFrames = 0;
            Timings.Reset();
        }
    }

    public GPUProfiler(IWGPUContext context)
    {
        this.context = context;
        Supported = context.Backend.DeviceSupportsTimestamps(context.DeviceHandle);
        TimestampPeriodNanoseconds = Supported ? context.Backend.QueueTimestampPeriod(context.QueueHandle) : 0;
    }

    public void BeginFrame(GPUReadbackQueue readbacks)
    {
        recording = null;
        foreach (var slot in slots)
        {
            if (!slot.Pending) continue;
            if (readbacks.GetStatus(slot.Readback) == GPUReadbackStatus.Pending) continue;
            if (Enabled && slot.Generation == generation && readbacks.TryGet<ulong>(slot.Readback, out var values))
            {
                var count = 0;
                for (var i = 0; i < slot.Count && i * 2 + 1 < values.Length; i++)
                {
                    var start = values[i * 2];
                    var end = values[i * 2 + 1];
                    if (end < start) continue;
                    results[count++] = new(slot.Labels[i], end - start);
                }
                if (count > 0) Timings.RecordFrame(results.AsSpan(0, count), Stopwatch.GetTimestamp());
            }
            slot.Pending = false;
        }
        if (!Enabled || !Supported) return;
        foreach (var slot in slots)
        {
            if (slot.Pending) continue;
            slot.Queries ??= new GPUQuerySet(context, MaxPasses * 2);
            slot.ResolveBuffer ??= context.CreateBuffer(new()
            {
                Label = "gpu-timing-resolve",
                Size = MaxPasses * 2 * sizeof(ulong),
                Usage = BufferUsage.QueryResolve | BufferUsage.CopySrc,
            });
            slot.Count = 0;
            slot.Truncated = false;
            slot.Generation = generation;
            recording = slot;
            return;
        }
        DroppedFrames++;
    }

    public GPUTimestampWrites BeginPass(ReadOnlySpan<char> label)
    {
        if (recording is null) return default;
        if (recording.Count == MaxPasses)
        {
            recording.Truncated = true;
            return default;
        }
        var lookup = labels.GetAlternateLookup<ReadOnlySpan<char>>();
        if (!lookup.TryGetValue(label, out var name))
        {
            name = label.IsEmpty ? "unnamed-pass" : label.ToString();
            lookup[label] = name;
        }
        var index = recording.Count++;
        recording.Labels[index] = name;
        return new(recording.Queries!.Native, (uint)(index * 2), (uint)(index * 2 + 1));
    }

    public void EndFrame(RenderContext renderContext)
    {
        if (recording is not { Count: > 0 } slot) return;
        if (slot.Truncated)
        {
            DroppedFrames++;
            recording = null;
            return;
        }
        ref var encoder = ref renderContext.CreateCommandEncoder("gpu-timing-resolve");
        context.Backend.CommandEncoderResolveTimestamps(encoder.Native, slot.Queries!.Native, (uint)(slot.Count * 2), slot.ResolveBuffer!.Native);
        slot.Pending = renderContext.Readbacks.TryRequest(slot.ResolveBuffer, 0, slot.Count * 2 * sizeof(ulong), out slot.Readback);
        if (!slot.Pending) DroppedFrames++;
        recording = null;
    }

    public void Dispose()
    {
        foreach (var slot in slots)
        {
            slot.Queries?.Dispose();
            slot.ResolveBuffer?.Dispose();
        }
    }
}

sealed class GPUQuerySet : GPUResourceWrapper
{
    public NativeHandle<QuerySetTag> Native { get; }

    public GPUQuerySet(IWGPUContext context, uint count) : base(context)
    {
        Native = context.Backend.DeviceCreateTimestampQuerySet(context.DeviceHandle, count);
    }

    protected override void Free()
    {
        context.Backend.QuerySetRelease(Native);
    }
}
