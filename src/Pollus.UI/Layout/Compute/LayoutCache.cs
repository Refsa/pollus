namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;

public struct LayoutCache
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

    [InlineArray(SlotCount)]
    private struct EntryBuffer { Entry _element0; }

    private EntryBuffer _entries;
    private int _nextSlot;
    private bool _isDirty;

    /// True if this cache has been cleared/invalidated and has no valid entries.
    public readonly bool IsDirty => _isDirty;

    public LayoutCache()
    {
        _entries = default;
        _nextSlot = 0;
        _isDirty = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGet(in LayoutInput input, out LayoutOutput output)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ref readonly var entry = ref _entries[i];
            if (entry.Valid && InputsMatch(in entry, in input))
            {
                output = entry.Output;
                return true;
            }
        }
        output = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool InputsMatch(in Entry entry, in LayoutInput input)
    {
        return entry.RunMode == input.RunMode
            && entry.KnownDimensions.Width == input.KnownDimensions.Width
            && entry.KnownDimensions.Height == input.KnownDimensions.Height
            && entry.ParentSize.Width == input.ParentSize.Width
            && entry.ParentSize.Height == input.ParentSize.Height
            && entry.AvailableSpace.Width.Tag == input.AvailableSpace.Width.Tag
            && entry.AvailableSpace.Width.Value == input.AvailableSpace.Width.Value
            && entry.AvailableSpace.Height.Tag == input.AvailableSpace.Height.Tag
            && entry.AvailableSpace.Height.Value == input.AvailableSpace.Height.Value;
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
