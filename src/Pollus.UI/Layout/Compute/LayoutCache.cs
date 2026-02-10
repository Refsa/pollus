using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public class LayoutCache
{
    private const int SlotCount = 9;

    private struct Entry
    {
        public bool Valid;
        public Size<float?> KnownDimensions;
        public Size<float?> ParentSize;
        public Size<AvailableSpace> AvailableSpace;
        public RunMode RunMode;
        public LayoutOutput Output;
    }

    private readonly Entry[] _entries = new Entry[SlotCount];
    private int _nextSlot;
    private bool _isDirty = true;

    /// True if this cache has been cleared/invalidated and has no valid entries.
    public bool IsDirty => _isDirty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(in LayoutInput input, out LayoutOutput output)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ref var entry = ref _entries[i];
            if (entry.Valid
                && entry.RunMode == input.RunMode
                && entry.KnownDimensions.Equals(input.KnownDimensions)
                && entry.ParentSize.Equals(input.ParentSize)
                && entry.AvailableSpace.Equals(input.AvailableSpace))
            {
                output = entry.Output;
                return true;
            }
        }
        output = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Store(in LayoutInput input, in LayoutOutput output)
    {
        ref var entry = ref _entries[_nextSlot];
        entry.Valid = true;
        entry.KnownDimensions = input.KnownDimensions;
        entry.ParentSize = input.ParentSize;
        entry.AvailableSpace = input.AvailableSpace;
        entry.RunMode = input.RunMode;
        entry.Output = output;
        _nextSlot = (_nextSlot + 1) % SlotCount;
        _isDirty = false;
    }

    public void Clear()
    {
        for (int i = 0; i < SlotCount; i++)
            _entries[i].Valid = false;
        _nextSlot = 0;
        _isDirty = true;
    }
}
