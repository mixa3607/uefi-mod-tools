namespace ArkProjects.UefiModTools.Utils.BinDataPattern;

public interface IBinaryDataPattern<T> where T : struct
{
    bool IsMatch(ReadOnlySpan<byte> fullData);
    bool TryFindSingle(ReadOnlySpan<byte> fullData, out Range match);
    T Read(ReadOnlySpan<byte> bytes, Range range);
}
