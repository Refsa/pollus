namespace Pollus.Graphics.Rendering;

using System.Diagnostics;

public readonly record struct GPUPassTiming(string Label, ulong ElapsedTicks);
public readonly record struct GPUProfilerSample(string Label, double AverageTicks, double PeakTicks);

public sealed class GPUProfilerTimings
{
    sealed class Accumulator
    {
        public double FrameTicks;
        public double TotalTicks;
        public double PeakTicks;
        public bool Seen;
    }

    readonly Dictionary<string, Accumulator> passes = new(StringComparer.Ordinal);
    readonly List<GPUProfilerSample> snapshot = [];
    long windowStart;
    int frames;
    bool started;

    public IReadOnlyList<GPUProfilerSample> Snapshot => snapshot;
    public int SnapshotVersion { get; private set; }
    public int SampledFrames { get; private set; }
    public double AverageTotalTicks { get; private set; }

    public void Reset()
    {
        passes.Clear();
        snapshot.Clear();
        frames = 0;
        started = false;
        SampledFrames = 0;
        AverageTotalTicks = 0;
        SnapshotVersion++;
    }

    public void RecordFrame(ReadOnlySpan<GPUPassTiming> timings, long timestamp)
    {
        if (!started)
        {
            windowStart = timestamp;
            started = true;
        }
        foreach (var timing in timings)
        {
            if (!passes.TryGetValue(timing.Label, out var pass)) passes.Add(timing.Label, pass = new());
            pass.FrameTicks += timing.ElapsedTicks;
            pass.Seen = true;
        }
        foreach (var pass in passes.Values)
        {
            pass.TotalTicks += pass.FrameTicks;
            pass.PeakTicks = Math.Max(pass.PeakTicks, pass.FrameTicks);
            pass.FrameTicks = 0;
        }
        frames++;
        if (timestamp - windowStart < Stopwatch.Frequency / 2) return;

        snapshot.Clear();
        AverageTotalTicks = 0;
        foreach (var (label, pass) in passes)
        {
            if (pass.Seen)
            {
                var average = pass.TotalTicks / frames;
                snapshot.Add(new(label, average, pass.PeakTicks));
                AverageTotalTicks += average;
            }
            pass.TotalTicks = 0;
            pass.PeakTicks = 0;
            pass.Seen = false;
        }
        snapshot.Sort(static (left, right) => right.AverageTicks.CompareTo(left.AverageTicks));
        SampledFrames = frames;
        SnapshotVersion++;
        frames = 0;
        windowStart = timestamp;
    }
}
