namespace ArkProjects.UefiModTools.Utils;

public static class RangeExtensions
{
    public static bool IsEmpty(this Range range, int len)
    {
        return range.GetOffsetAndLength(len).Length == 0;
    }
}
