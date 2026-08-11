namespace Pollus.Jobs;

using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Utils;

public record struct JobHandle
{
    internal JobScheduler Pool { get; init; }
    internal int WorkIndex { get; init; }
    internal int Generation { get; init; }

    public bool IsCompleted => Pool.IsCompleted(WorkIndex, Generation);
    public void Complete() => Pool.Complete(WorkIndex, Generation);
}

public class JobScheduler
{
    class Slot
    {
        WorkItem? workItem;
        int generation;

        public WorkItem? WorkItem
        {
            get => Volatile.Read(ref workItem);
            set => Volatile.Write(ref workItem, value);
        }

        public int Generation => Volatile.Read(ref generation);
        public int NextGeneration() => Interlocked.Increment(ref generation);
    }

    Stack<int> slotPool = new();
    ArrayList<Slot> slots = new();

    ConcurrentQueue<WorkItem> completed = new();
    Queue<WorkItem> flushNext = new();

    Dictionary<int, Pool<WorkItem>> workItemPool = new();

    public JobScheduler()
    {

    }

    public WorkItem GetWorkItem(int idx) => slots[idx].WorkItem
        ?? throw new InvalidOperationException("Job slot is not occupied.");

    internal bool IsCompleted(int workIndex, int generation)
    {
        var slot = slots[workIndex];
        if (slot.Generation != generation) return true;

        var workItem = slot.WorkItem;
        var completed = workItem is null || workItem.IsCompleted;

        return slot.Generation != generation || completed;
    }

    internal void Complete(int workIndex, int generation)
    {
        var slot = slots[workIndex];
        if (slot.Generation != generation) return;

        var workItem = slot.WorkItem;
        if (workItem is null || slot.Generation != generation) return;

        workItem.Wait();
    }

    public JobHandle Schedule<T>(T job, params ReadOnlySpan<JobHandle> deps)
        where T : struct, IJob
    {
        var workItem = GetWorkItem<T>();
        if (!slotPool.TryPop(out var workIndex))
        {
            workIndex = slots.Count;
            slots.Add(new Slot());
        }

        var slot = slots[workIndex];
        var generation = slot.NextGeneration();
        workItem.Prepare(job, workIndex, deps);
        slot.WorkItem = workItem;

        workItem.TryQueue();

        return new JobHandle()
        {
            Pool = this,
            WorkIndex = workIndex,
            Generation = generation,
        };
    }

    public JobHandle ScheduleFor<T>(T job, int length, int batchSize, params ReadOnlySpan<JobHandle> deps)
        where T : struct, IJobFor
    {
        if (length == 0)
        {
            return Schedule(new JobBarrier(), deps);
        }

        if (length <= batchSize)
        {
            return ScheduleForChunk(job, 0, length, deps);
        }

        var batchCount = (length + batchSize - 1) / batchSize;
        var batchDeps = ArrayPool<JobHandle>.Shared.Rent(batchCount);

        try
        {
            for (int i = 0; i < batchCount; i++)
            {
                var start = i * batchSize;
                var end = Math.Min(length, start + batchSize);
                batchDeps[i] = ScheduleForChunk(job, start, end, deps);
            }

            return Schedule(new JobBarrier(), batchDeps.AsSpan(0, batchCount));
        }
        finally
        {
            ArrayPool<JobHandle>.Shared.Return(batchDeps);
        }
    }

    internal JobHandle ScheduleForChunk<T>(T job, int start, int end, params ReadOnlySpan<JobHandle> deps)
        where T : struct, IJobForBase
    {
        var workItem = GetWorkItemFor<T>();
        if (!slotPool.TryPop(out var workIndex))
        {
            workIndex = slots.Count;
            slots.Add(new Slot());
        }

        var slot = slots[workIndex];
        var generation = slot.NextGeneration();
        workItem.Prepare(job, workIndex, start, end, deps);
        slot.WorkItem = workItem;

        workItem.TryQueue();

        return new JobHandle()
        {
            Pool = this,
            WorkIndex = workIndex,
            Generation = generation,
        };
    }

    public JobHandle Combine(params ReadOnlySpan<JobHandle> deps)
    {
        return Schedule(new JobBarrier(), deps);
    }

    public WorkItemJoin BeginJoin(params ReadOnlySpan<JobHandle> deps)
    {
        var workItem = GetWorkItemJoin();
        if (!slotPool.TryPop(out var workIndex))
        {
            workIndex = slots.Count;
            slots.Add(new Slot());
        }

        var slot = slots[workIndex];
        var generation = slot.NextGeneration();
        var handle = new JobHandle()
        {
            Pool = this,
            WorkIndex = workIndex,
            Generation = generation,
        };

        workItem.Prepare(handle, deps);
        slot.WorkItem = workItem;

        return workItem;
    }

    public void Flush()
    {
        while (flushNext.TryDequeue(out var wi))
        {
            Return(wi);
        }

        while (completed.TryDequeue(out var wi))
        {
            flushNext.Enqueue(wi);
        }
    }

    internal void MarkCompleted(WorkItem workItem)
    {
        completed.Enqueue(workItem);
    }

    void Return(WorkItem workItem)
    {
        var slot = slots[workItem.WorkIndex];
        if (!ReferenceEquals(slot.WorkItem, workItem)) return;

        slot.NextGeneration();
        slot.WorkItem = null;
        slotPool.Push(workItem.WorkIndex);
        if (!workItemPool.TryGetValue(workItem.TypeId, out var pool)) return;
        pool.Return(workItem);
    }

    internal bool TryAddDependent(JobHandle dependency, WorkItem dependent)
    {
        if (!ReferenceEquals(dependency.Pool, this))
        {
            throw new InvalidOperationException("Dependency belongs to another job pool.");
        }

        var slot = slots[dependency.WorkIndex];
        if (slot.Generation != dependency.Generation) return false;

        var workItem = slot.WorkItem;
        if (workItem is null || slot.Generation != dependency.Generation) return false;

        return workItem.TryAddDependent(dependent);
    }

    WorkItem<T> GetWorkItem<T>()
        where T : struct, IJob
    {
        var tid = TypeLookup.ID<WorkItem<T>>();
        if (!workItemPool.TryGetValue(tid, out var pool))
        {
            pool = new Pool<WorkItem>(() => new WorkItem<T>(this, tid), 4);
            workItemPool[tid] = pool;
        }

        return (WorkItem<T>)pool.Rent();
    }

    WorkItemFor<T> GetWorkItemFor<T>()
        where T : struct, IJobForBase
    {
        var tid = TypeLookup.ID<WorkItemFor<T>>();
        if (!workItemPool.TryGetValue(tid, out var pool))
        {
            pool = new Pool<WorkItem>(() => new WorkItemFor<T>(this, tid), 4);
            workItemPool[tid] = pool;
        }

        return (WorkItemFor<T>)pool.Rent();
    }

    WorkItemJoin GetWorkItemJoin()
    {
        var tid = TypeLookup.ID<WorkItemJoin>();
        if (!workItemPool.TryGetValue(tid, out var pool))
        {
            pool = new Pool<WorkItem>(() => new WorkItemJoin(this, tid), 4);
            workItemPool[tid] = pool;
        }

        return (WorkItemJoin)pool.Rent();
    }
}

public class WorkItem : IThreadPoolWorkItem
{
    protected const int Created = 0;
    protected const int Queued = 1;
    protected const int Running = 2;
    protected const int Completed = 3;

    public JobScheduler Pool { get; set; } = null!;
    public int WorkIndex { get; set; }
    public int TypeId { get; set; }
    readonly Lock sync;
    readonly ManualResetEventSlim completion;
    protected int state;
    protected int remainingDependencies;
    protected ArrayList<WorkItem> dependents = new(4);
    protected bool completed;
    protected Exception? error;

    public bool IsCompleted => Volatile.Read(ref completed);
    public bool IsReady => Volatile.Read(ref remainingDependencies) == 0;

    public WorkItem()
    {
        sync = new();
        completion = new(false);
    }

    public WorkItem(JobScheduler pool, int typeId) : this()
    {
        Pool = pool;
        TypeId = typeId;
    }

    protected void Prepare(int workIndex, ReadOnlySpan<JobHandle> deps, int pendingCount = 0)
    {
        WorkIndex = workIndex;
        completed = false;
        completion.Reset();
        state = Created;
        error = null;

        Interlocked.Exchange(ref remainingDependencies, deps.Length + pendingCount);

        dependents.Clear();
        for (int i = 0; i < deps.Length; i++)
        {
            if (!Pool.TryAddDependent(deps[i], this))
            {
                Interlocked.Decrement(ref remainingDependencies);
            }
        }
    }

    protected void Complete()
    {
        Span<WorkItem> deps;
        lock (sync)
        {
            Volatile.Write(ref completed, true);
            completion.Set();
            deps = dependents.AsSpan();
            dependents.Clear();
        }

        foreach (var dep in deps) dep.Notify();
        deps.Clear();
        Pool.MarkCompleted(this);
    }

    public void Wait() => completion.Wait();

    public void Notify()
    {
        if (Interlocked.Decrement(ref remainingDependencies) == 0)
        {
            TryQueue();
        }
    }

    public bool TryAddDependent(WorkItem workItem)
    {
        lock (sync)
        {
            if (Volatile.Read(ref completed)) return false;
            dependents.Add(workItem);
            return true;
        }
    }

    public void TryQueue()
    {
        if (!IsReady) return;
        if (Interlocked.CompareExchange(ref state, Queued, Created) != Created)
        {
            return;
        }

        OnReady();
    }

    protected virtual void OnReady()
    {
        ThreadPool.UnsafeQueueUserWorkItem(this, false);
    }

    public virtual void Execute() { }
}

public interface IJob
{
    void Execute();
}

public class WorkItem<T> : WorkItem
    where T : struct, IJob
{
    T job;

    public WorkItem(JobScheduler pool, int typeId) : base(pool, typeId)
    {
    }

    public void Prepare(T job, int workIndex, ReadOnlySpan<JobHandle> deps)
    {
        this.job = job;
        base.Prepare(workIndex, deps);
    }

    public override void Execute()
    {
        try
        {
            job.Execute();
        }
        catch (Exception ex)
        {
            error = ex;
        }
        finally
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) job = default;
            Interlocked.Exchange(ref state, Completed);
            Complete();
        }
    }
}

public interface IJobForBase
{
    void Execute(int index) { }
    void Execute(int start, int end) { }
}

public interface IJobFor : IJobForBase
{
    new void Execute(int index);
}

public interface IJobForChunk : IJobForBase
{
    new void Execute(int start, int end);
}

public struct JobBarrier : IJob
{
    public void Execute()
    {

    }
}

public class WorkItemFor<T> : WorkItem
    where T : struct, IJobForBase
{
    T job;
    int start;
    int end;

    public WorkItemFor() { }
    public WorkItemFor(JobScheduler pool, int typeId) : base(pool, typeId) { }

    public void Prepare(T job, int workIndex, int start, int end, ReadOnlySpan<JobHandle> deps)
    {
        this.job = job;
        this.start = start;
        this.end = end;
        base.Prepare(workIndex, deps);
    }

    public override void Execute()
    {
        try
        {
            if (job is IJobForChunk)
            {
                job.Execute(start, end);
            }
            else
            {
                for (int i = start; i < end; i++)
                {
                    job.Execute(i);
                }
            }
        }
        catch (Exception ex)
        {
            error = ex;
        }
        finally
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) job = default;
            Interlocked.Exchange(ref state, Completed);
            Complete();
        }
    }
}

public class WorkItemJoin : WorkItem
{
    JobHandle ownHandle;

    public WorkItemJoin() { }
    public WorkItemJoin(JobScheduler pool, int typeId) : base(pool, typeId) { }

    public void Prepare(JobHandle ownHandle, ReadOnlySpan<JobHandle> deps)
    {
        base.Prepare(ownHandle.WorkIndex, deps, 1);
        this.ownHandle = ownHandle;
    }

    public void Add(JobHandle handle)
    {
        Interlocked.Increment(ref remainingDependencies);
        if (!Pool.TryAddDependent(handle, this)) Notify();
    }

    public JobHandle EndJoin()
    {
        Notify();
        return ownHandle;
    }

    protected override void OnReady()
    {
        Interlocked.Exchange(ref state, Completed);
        Complete();
    }
}
