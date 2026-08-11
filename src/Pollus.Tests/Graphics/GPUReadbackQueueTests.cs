namespace Pollus.Tests.Graphics;

using System.Reflection;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;

public class GPUReadbackQueueTests
{
    [Fact]
    public void NativeMapCallbackUsesAnUnmanagedEntryPoint()
    {
        var callback = typeof(GPUReadbackQueue).GetMethod("OnMapCompleted", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(callback);
        Assert.NotNull(callback.GetCustomAttribute<UnmanagedCallersOnlyAttribute>());
    }

    [Fact]
    public void PublishedBytesCanBeReadAsTypedValues()
    {
        var readbacks = new GPUReadbackQueue(resultRetentionFrames: 2);
        var handle = readbacks.Request(3 * sizeof(uint));
        ReadOnlySpan<uint> expected = [4u, 8u, 12u];

        readbacks.Publish(handle, MemoryMarshal.AsBytes(expected));

        Assert.True(readbacks.TryGet<uint>(handle, out var values));
        Assert.Equal(expected.ToArray(), values.ToArray());
        Assert.Equal(GPUReadbackStatus.Ready, readbacks.GetStatus(handle));
    }

    [Fact]
    public void TypedReadRequiresAnExactElementSize()
    {
        var readbacks = new GPUReadbackQueue(resultRetentionFrames: 2);
        var handle = readbacks.Request(12);

        readbacks.Publish(handle, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);

        Assert.False(readbacks.TryGet<ulong>(handle, out _));
    }

    [Fact]
    public void ResultsExpireAfterConfiguredGameplayUpdates()
    {
        var readbacks = new GPUReadbackQueue(resultRetentionFrames: 2);
        var handle = readbacks.Request(sizeof(uint));
        ReadOnlySpan<uint> expected = [10u];
        readbacks.Publish(handle, MemoryMarshal.AsBytes(expected));

        readbacks.AdvanceFrame();
        Assert.True(readbacks.TryGet<uint>(handle, out _));

        readbacks.AdvanceFrame();
        Assert.True(readbacks.TryGet<uint>(handle, out _));

        readbacks.AdvanceFrame();
        Assert.False(readbacks.TryGet<uint>(handle, out _));
        Assert.Equal(GPUReadbackStatus.Expired, readbacks.GetStatus(handle));
    }

    [Fact]
    public void StaleHandlesCannotReadReusedRequestSlots()
    {
        var readbacks = new GPUReadbackQueue(resultRetentionFrames: 1, requestCapacity: 1);
        var stale = readbacks.Request(sizeof(uint));
        ReadOnlySpan<uint> first = [10u];
        readbacks.Publish(stale, MemoryMarshal.AsBytes(first));
        readbacks.AdvanceFrame();
        readbacks.AdvanceFrame();

        var current = readbacks.Request(sizeof(uint));
        ReadOnlySpan<uint> second = [20u];
        readbacks.Publish(current, MemoryMarshal.AsBytes(second));

        Assert.False(readbacks.TryGet<uint>(stale, out _));
        Assert.True(readbacks.TryGet<uint>(current, out var values));
        Assert.Equal(20u, values[0]);
    }

    [Fact]
    public void RequestsShareOneAlignedPendingBatch()
    {
        var readbacks = new GPUReadbackQueue();

        Assert.True(readbacks.TryRequest(4, out var first));
        Assert.True(readbacks.TryRequest(12, out var second));

        Assert.Equal(0, readbacks.GetDestinationOffset(first));
        Assert.Equal(4, readbacks.GetDestinationOffset(second));
        Assert.Equal(16, readbacks.PendingByteLength);
    }

    [Fact]
    public void TwoSubmittedBatchesApplyBackpressureUntilOneCompletes()
    {
        var readbacks = new GPUReadbackQueue();
        Assert.True(readbacks.TryRequest(4, out var first));
        var firstBatch = readbacks.SubmitPending();
        Assert.True(readbacks.TryRequest(4, out _));
        readbacks.SubmitPending();

        Assert.False(readbacks.TryRequest(4, out _));

        ReadOnlySpan<uint> result = [42u];
        readbacks.CompleteBatch(firstBatch, MemoryMarshal.AsBytes(result));

        Assert.True(readbacks.TryRequest(4, out _));
        Assert.True(readbacks.TryGet<uint>(first, out var values));
        Assert.Equal(42u, values[0]);
    }

    [Fact]
    public void FailedBatchReleasesItsStagingBuffer()
    {
        var readbacks = new GPUReadbackQueue();
        Assert.True(readbacks.TryRequest(4, out var handle));
        var batch = readbacks.SubmitPending();

        readbacks.FailBatch(batch);

        Assert.Equal(GPUReadbackStatus.Failed, readbacks.GetStatus(handle));
        Assert.True(readbacks.TryRequest(4, out _));
    }

    [Fact]
    public void RequestsBeyondTheConfiguredBatchLimitAreRejected()
    {
        var readbacks = new GPUReadbackQueue(maxBatchBytes: 16);

        Assert.True(readbacks.TryRequest(12, out _));
        Assert.False(readbacks.TryRequest(8, out _));
        Assert.Equal(12, readbacks.PendingByteLength);
    }

    [Fact]
    public void MappedBatchIsNotReusedUntilItIsUnmapped()
    {
        var readbacks = new GPUReadbackQueue();
        Assert.True(readbacks.TryRequest(4, out _));
        var mapped = readbacks.SubmitPending();
        Assert.True(readbacks.TryRequest(4, out _));
        readbacks.SubmitPending();

        readbacks.CompleteBatch(mapped, [1, 2, 3, 4], releaseSlot: false);

        Assert.False(readbacks.TryRequest(4, out _));
        readbacks.ReleaseBatch(mapped);
        Assert.True(readbacks.TryRequest(4, out _));
    }

    [Fact]
    public void ExpiredManagedSnapshotsAreReused()
    {
        var readbacks = new GPUReadbackQueue(resultRetentionFrames: 1);
        Assert.True(readbacks.TryRequest(4, out _));
        readbacks.CompleteBatch(readbacks.SubmitPending(), [1, 2, 3, 4]);
        Assert.Equal(1, readbacks.AllocatedSnapshotBufferCount);

        readbacks.AdvanceFrame();
        readbacks.AdvanceFrame();
        Assert.True(readbacks.TryRequest(4, out _));
        readbacks.CompleteBatch(readbacks.SubmitPending(), [5, 6, 7, 8]);

        Assert.Equal(1, readbacks.AllocatedSnapshotBufferCount);
    }
}
