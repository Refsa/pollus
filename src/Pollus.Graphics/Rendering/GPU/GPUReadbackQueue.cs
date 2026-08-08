namespace Pollus.Graphics.Rendering;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Pollus.Graphics.WGPU;

public enum GPUReadbackStatus : byte
{
    Invalid,
    Pending,
    Ready,
    Failed,
    Expired,
}

public readonly record struct GPUReadbackHandle(int Index, uint Generation);

public sealed unsafe class GPUReadbackQueue : IDisposable
{
    enum StagingState : byte
    {
        Available,
        Recording,
        Submitted,
        Mapping,
    }

    sealed class StagingSlot
    {
        public StagingState State;
        public int[] RequestIndices = new int[16];
        public int RequestCount;
        public int ByteLength;
        public uint Generation;
        public GPUBuffer? Buffer;
    }

    sealed class SnapshotSlot
    {
        public byte[] Data = [];
        public uint Generation;
        public long ExpiresAfterFrame;
        public bool Active;
    }

    internal readonly record struct BatchHandle(int Slot, uint Generation);

    struct RequestRecord
    {
        public uint Generation;
        public GPUReadbackStatus Status;
        public int ByteLength;
        public int SnapshotIndex;
        public uint SnapshotGeneration;
        public int DataOffset;
        public int DestinationOffset;
        public long ExpiresAfterFrame;
        public GPUBuffer? Source;
        public ulong SourceOffset;
    }

    sealed record MapRequest(GPUReadbackQueue Owner, IWGPUContext Context, BatchHandle Batch);

    static readonly Silk.NET.WebGPU.BufferMapCallback MapCallbackDelegate = OnMapCompleted;
    static readonly Silk.NET.WebGPU.PfnBufferMapCallback MapCallback = MapCallbackDelegate;

    readonly int resultRetentionFrames;
    readonly int maxBatchBytes;
    readonly string label;
    RequestRecord[] requests;
    readonly StagingSlot[] stagingSlots = [new(), new()];
    SnapshotSlot[] snapshots;
    int recordingSlot = -1;
    long frame;

    internal int PendingByteLength => recordingSlot >= 0 ? stagingSlots[recordingSlot].ByteLength : 0;
    internal bool HasPendingRequests => recordingSlot >= 0;
    internal int AllocatedSnapshotBufferCount { get; private set; }

    public GPUReadbackQueue(
        int resultRetentionFrames = 2,
        int requestCapacity = 64,
        int maxBatchBytes = 64 * 1024 * 1024,
        string label = "gpu-readback")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(resultRetentionFrames, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(requestCapacity, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxBatchBytes, 4);
        this.resultRetentionFrames = resultRetentionFrames;
        this.maxBatchBytes = maxBatchBytes;
        this.label = label;
        requests = new RequestRecord[requestCapacity];
        snapshots = new SnapshotSlot[resultRetentionFrames + 2];
        for (int i = 0; i < snapshots.Length; i++) snapshots[i] = new SnapshotSlot();
    }

    public void Dispose()
    {
        for (int i = 0; i < stagingSlots.Length; i++)
        {
            stagingSlots[i].Buffer?.Dispose();
            stagingSlots[i].Buffer = null;
        }
    }

    public GPUReadbackStatus GetStatus(GPUReadbackHandle handle)
    {
        return TryGetRecord(handle, out var record) ? record.Status : GPUReadbackStatus.Invalid;
    }

    public bool TryGet<T>(GPUReadbackHandle handle, out ReadOnlySpan<T> values)
        where T : unmanaged
    {
        values = default;
        if (!TryGetRecord(handle, out var record)
            || record.Status != GPUReadbackStatus.Ready
            || (uint)record.SnapshotIndex >= (uint)snapshots.Length
            || record.ByteLength % Unsafe.SizeOf<T>() != 0) return false;

        var snapshot = snapshots[record.SnapshotIndex];
        if (!snapshot.Active || snapshot.Generation != record.SnapshotGeneration) return false;
        values = MemoryMarshal.Cast<byte, T>(snapshot.Data.AsSpan(record.DataOffset, record.ByteLength));
        return true;
    }

    public void AdvanceFrame()
    {
        frame++;
        for (int i = 0; i < requests.Length; i++)
        {
            ref var request = ref requests[i];
            if (request.Status is not (GPUReadbackStatus.Ready or GPUReadbackStatus.Failed)
                || frame <= request.ExpiresAfterFrame) continue;
            request.Status = GPUReadbackStatus.Expired;
            request.SnapshotIndex = -1;
        }
        for (int i = 0; i < snapshots.Length; i++)
        {
            if (snapshots[i].Active && frame > snapshots[i].ExpiresAfterFrame)
            {
                snapshots[i].Active = false;
            }
        }
    }

    public void Update(IWGPUContext context)
    {
        AdvanceFrame();
        context.Backend.DevicePoll(context.DeviceHandle);
    }

    public bool TryRequest(
        GPUBuffer source,
        ulong sourceOffset,
        int byteLength,
        out GPUReadbackHandle handle)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(byteLength, 1);
        if (source.Disposed) throw new ObjectDisposedException(nameof(source));
        if (sourceOffset > source.Size || (ulong)byteLength > source.Size - sourceOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), "GPU readback range exceeds the source buffer.");
        }
        return ReserveRequest(source, sourceOffset, byteLength, out handle);
    }

    internal GPUReadbackHandle Request(int byteLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(byteLength, 1);
        var index = AllocateRequest(byteLength);
        return new(index, requests[index].Generation);
    }

    internal bool TryRequest(int byteLength, out GPUReadbackHandle handle)
    {
        return ReserveRequest(null, 0, byteLength, out handle);
    }

    bool ReserveRequest(GPUBuffer? source, ulong sourceOffset, int byteLength, out GPUReadbackHandle handle)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(byteLength, 1);
        if ((byteLength & 3) != 0)
        {
            throw new ArgumentException("GPU readback size must be a multiple of four bytes.", nameof(byteLength));
        }
        if (byteLength > maxBatchBytes
            || recordingSlot >= 0 && stagingSlots[recordingSlot].ByteLength > maxBatchBytes - byteLength)
        {
            handle = default;
            return false;
        }

        if (recordingSlot < 0)
        {
            for (int i = 0; i < stagingSlots.Length; i++)
            {
                if (stagingSlots[i].State != StagingState.Available) continue;
                recordingSlot = i;
                var staging = stagingSlots[i];
                staging.State = StagingState.Recording;
                staging.RequestCount = 0;
                staging.ByteLength = 0;
                break;
            }
        }
        if (recordingSlot < 0)
        {
            handle = default;
            return false;
        }

        var slot = stagingSlots[recordingSlot];
        var requestIndex = AllocateRequest(byteLength);
        if (slot.RequestCount == slot.RequestIndices.Length)
        {
            Array.Resize(ref slot.RequestIndices, slot.RequestIndices.Length * 2);
        }
        requests[requestIndex].DestinationOffset = slot.ByteLength;
        requests[requestIndex].Source = source;
        requests[requestIndex].SourceOffset = sourceOffset;
        slot.RequestIndices[slot.RequestCount++] = requestIndex;
        slot.ByteLength = checked(slot.ByteLength + byteLength);
        handle = new(requestIndex, requests[requestIndex].Generation);
        return true;
    }

    internal int GetDestinationOffset(GPUReadbackHandle handle)
    {
        if (!TryGetRecord(handle, out var request)) throw new ArgumentException("Invalid GPU readback handle.", nameof(handle));
        return request.DestinationOffset;
    }

    internal BatchHandle SubmitPending()
    {
        if (recordingSlot < 0) throw new InvalidOperationException("No GPU readback batch is pending.");
        var index = recordingSlot;
        var slot = stagingSlots[index];
        slot.State = StagingState.Submitted;
        slot.Generation++;
        if (slot.Generation == 0) slot.Generation = 1;
        recordingSlot = -1;
        return new(index, slot.Generation);
    }

    internal bool RecordPending(IWGPUContext context, in GPUCommandEncoder encoder)
    {
        if (recordingSlot < 0) return false;
        var slot = stagingSlots[recordingSlot];
        EnsureBuffer(context, recordingSlot, slot.ByteLength);
        for (int i = 0; i < slot.RequestCount; i++)
        {
            ref var request = ref requests[slot.RequestIndices[i]];
            if (request.Source is null)
            {
                throw new InvalidOperationException("GPU readback request has no source buffer.");
            }
            encoder.CopyBufferToBuffer(
                request.Source,
                slot.Buffer!,
                (ulong)request.ByteLength,
                request.SourceOffset,
                (ulong)request.DestinationOffset);
        }
        SubmitPending();
        return true;
    }

    internal void NotifySubmitted(IWGPUContext context)
    {
        for (int i = 0; i < stagingSlots.Length; i++)
        {
            var slot = stagingSlots[i];
            if (slot.State != StagingState.Submitted) continue;
            var batch = new BatchHandle(i, slot.Generation);
            slot.State = StagingState.Mapping;
            BeginMap(context, batch);
        }
    }

    internal void CompleteBatch(BatchHandle handle, ReadOnlySpan<byte> data, bool releaseSlot = true)
    {
        if ((uint)handle.Slot >= (uint)stagingSlots.Length)
        {
            throw new ArgumentException("Invalid GPU readback batch.", nameof(handle));
        }
        var slot = stagingSlots[handle.Slot];
        if (slot.State is not (StagingState.Submitted or StagingState.Mapping)
            || slot.Generation != handle.Generation)
        {
            throw new InvalidOperationException("GPU readback batch is not submitted.");
        }
        if (data.Length != slot.ByteLength)
        {
            throw new ArgumentException("Readback data size does not match the batch.", nameof(data));
        }

        var snapshotIndex = AcquireSnapshot(data.Length);
        var snapshot = snapshots[snapshotIndex];
        data.CopyTo(snapshot.Data);
        for (int i = 0; i < slot.RequestCount; i++)
        {
            ref var request = ref requests[slot.RequestIndices[i]];
            request.SnapshotIndex = snapshotIndex;
            request.SnapshotGeneration = snapshot.Generation;
            request.DataOffset = request.DestinationOffset;
            request.ExpiresAfterFrame = frame + resultRetentionFrames;
            request.Status = GPUReadbackStatus.Ready;
            request.Source = null;
        }
        if (releaseSlot) slot.State = StagingState.Available;
    }

    internal void FailBatch(BatchHandle handle, bool releaseSlot = true)
    {
        if ((uint)handle.Slot >= (uint)stagingSlots.Length)
        {
            throw new ArgumentException("Invalid GPU readback batch.", nameof(handle));
        }
        var slot = stagingSlots[handle.Slot];
        if (slot.State is not (StagingState.Submitted or StagingState.Mapping)
            || slot.Generation != handle.Generation)
        {
            throw new InvalidOperationException("GPU readback batch is not submitted.");
        }

        for (int i = 0; i < slot.RequestCount; i++)
        {
            ref var request = ref requests[slot.RequestIndices[i]];
            request.Status = GPUReadbackStatus.Failed;
            request.ExpiresAfterFrame = frame + resultRetentionFrames;
            request.Source = null;
        }
        if (releaseSlot) slot.State = StagingState.Available;
    }

    internal void ReleaseBatch(BatchHandle handle)
    {
        if ((uint)handle.Slot >= (uint)stagingSlots.Length)
        {
            throw new ArgumentException("Invalid GPU readback batch.", nameof(handle));
        }
        var slot = stagingSlots[handle.Slot];
        if (slot.State is not (StagingState.Submitted or StagingState.Mapping)
            || slot.Generation != handle.Generation)
        {
            throw new InvalidOperationException("GPU readback batch cannot be released.");
        }
        slot.State = StagingState.Available;
    }

    int AllocateRequest(int byteLength)
    {
        for (int i = 0; i < requests.Length; i++)
        {
            if (requests[i].Status is not GPUReadbackStatus.Invalid and not GPUReadbackStatus.Expired) continue;
            BeginRequest(i, byteLength);
            return i;
        }

        var index = requests.Length;
        Array.Resize(ref requests, checked(index * 2));
        BeginRequest(index, byteLength);
        return index;
    }

    internal void Publish(GPUReadbackHandle handle, ReadOnlySpan<byte> data)
    {
        if (!TryGetRecordIndex(handle, out var index) || requests[index].Status != GPUReadbackStatus.Pending)
        {
            throw new InvalidOperationException("GPU readback request is not pending.");
        }
        if (data.Length != requests[index].ByteLength)
        {
            throw new ArgumentException("Readback data size does not match the request.", nameof(data));
        }

        ref var request = ref requests[index];
        var snapshotIndex = AcquireSnapshot(data.Length);
        data.CopyTo(snapshots[snapshotIndex].Data);
        request.SnapshotIndex = snapshotIndex;
        request.SnapshotGeneration = snapshots[snapshotIndex].Generation;
        request.DataOffset = 0;
        request.ExpiresAfterFrame = frame + resultRetentionFrames;
        request.Status = GPUReadbackStatus.Ready;
    }

    void BeginRequest(int index, int byteLength)
    {
        ref var request = ref requests[index];
        request.Generation++;
        if (request.Generation == 0) request.Generation = 1;
        request.Status = GPUReadbackStatus.Pending;
        request.ByteLength = byteLength;
        request.SnapshotIndex = -1;
        request.SnapshotGeneration = 0;
        request.DataOffset = 0;
        request.DestinationOffset = 0;
        request.Source = null;
        request.SourceOffset = 0;
    }

    int AcquireSnapshot(int byteLength)
    {
        for (int i = 0; i < snapshots.Length; i++)
        {
            var snapshot = snapshots[i];
            if (snapshot.Active) continue;
            PrepareSnapshot(snapshot, byteLength);
            return i;
        }

        var index = snapshots.Length;
        Array.Resize(ref snapshots, checked(index * 2));
        for (int i = index; i < snapshots.Length; i++) snapshots[i] = new SnapshotSlot();
        PrepareSnapshot(snapshots[index], byteLength);
        return index;
    }

    void PrepareSnapshot(SnapshotSlot snapshot, int byteLength)
    {
        if (snapshot.Data.Length < byteLength)
        {
            snapshot.Data = new byte[byteLength];
            AllocatedSnapshotBufferCount++;
        }
        snapshot.Generation++;
        if (snapshot.Generation == 0) snapshot.Generation = 1;
        snapshot.ExpiresAfterFrame = frame + resultRetentionFrames;
        snapshot.Active = true;
    }

    void EnsureBuffer(IWGPUContext context, int slotIndex, int byteLength)
    {
        var slot = stagingSlots[slotIndex];
        if (slot.Buffer is null)
        {
            slot.Buffer = context.CreateBuffer(new BufferDescriptor
            {
                Label = $"{label}-{slotIndex}",
                Usage = BufferUsage.MapRead | BufferUsage.CopyDst,
                Size = (ulong)byteLength,
            });
            return;
        }
        if (slot.Buffer.Size >= (ulong)byteLength) return;
        var doubled = ulong.Min(slot.Buffer.Size * 2, (ulong)maxBatchBytes);
        slot.Buffer.Resize((uint)ulong.Max((ulong)byteLength, doubled));
    }

    void BeginMap(IWGPUContext context, BatchHandle batch)
    {
        var slot = stagingSlots[batch.Slot];
        var handle = GCHandle.Alloc(new MapRequest(this, context, batch));
        var userData = (void*)GCHandle.ToIntPtr(handle);
        try
        {
            context.Backend.BufferMapReadAsync(
                slot.Buffer!.Native,
                0,
                (nuint)slot.ByteLength,
                MapCallback,
                userData);
        }
        catch
        {
            handle.Free();
            FailBatch(batch);
            throw;
        }
    }

    /// <summary>
    /// WPGU Callback
    /// </summary>
    static void OnMapCompleted(Silk.NET.WebGPU.BufferMapAsyncStatus status, void* userData)
    {
        var handle = GCHandle.FromIntPtr((nint)userData);
        var request = (MapRequest)handle.Target!;
        try
        {
            if (status == Silk.NET.WebGPU.BufferMapAsyncStatus.Success)
            {
                request.Owner.ReadMapped(request.Context, request.Batch);
            }
            else
            {
                request.Owner.FailBatch(request.Batch);
            }
        }
        finally
        {
            handle.Free();
        }
    }

    void ReadMapped(IWGPUContext context, BatchHandle batch)
    {
        var slot = stagingSlots[batch.Slot];
        var mapped = context.Backend.BufferGetConstMappedRange(
            slot.Buffer!.Native,
            0,
            (nuint)slot.ByteLength);
        try
        {
            if (mapped is null)
            {
                FailBatch(batch, releaseSlot: false);
                return;
            }
            CompleteBatch(batch, new ReadOnlySpan<byte>(mapped, slot.ByteLength), releaseSlot: false);
        }
        finally
        {
            context.Backend.BufferUnmap(slot.Buffer.Native);
            ReleaseBatch(batch);
        }
    }

    bool TryGetRecord(GPUReadbackHandle handle, out RequestRecord record)
    {
        if (TryGetRecordIndex(handle, out var index))
        {
            record = requests[index];
            return true;
        }
        record = default;
        return false;
    }

    bool TryGetRecordIndex(GPUReadbackHandle handle, out int index)
    {
        index = handle.Index;
        return (uint)index < (uint)requests.Length
            && handle.Generation != 0
            && requests[index].Generation == handle.Generation;
    }
}
