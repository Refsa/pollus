namespace Pollus.UI.Layout;

public enum RunMode : byte
{
    PerformLayout,
    ComputeSize,
}

public enum SizingMode : byte
{
    InherentSize,
    ContentSize,
}

public enum RequestedAxis : byte
{
    Both,
    Horizontal,
    Vertical,
}

public record struct LayoutInput
{
    public RunMode RunMode;
    public SizingMode SizingMode;
    public RequestedAxis Axis;
    public Size<float?> KnownDimensions;
    public Size<float?> ParentSize;
    public Size<AvailableSpace> AvailableSpace;
}
