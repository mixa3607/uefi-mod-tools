namespace ArkProjects.UefiModTools.Utils;

public static class SpanExtensions
{
    public static ReadOnlySpan<T> Slice<T>(this ReadOnlySpan<T> span, Range range)
    {
        var l = range.GetOffsetAndLength(span.Length);
        return span.Slice(l.Offset, l.Length);
    }
}
