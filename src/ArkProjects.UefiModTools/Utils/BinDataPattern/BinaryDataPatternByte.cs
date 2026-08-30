namespace ArkProjects.UefiModTools.Utils.BinDataPattern;

public class BinaryDataPatternByte
{
    private readonly bool _any;
    private readonly byte _exact;

    public BinaryDataPatternByte(byte? data)
    {
        _any = data == null;
        _exact = data ?? 0xFF;
    }

    public bool IsMatch(byte data)
    {
        return _any || data == _exact;
    }

    public static readonly BinaryDataPatternByte Any = new BinaryDataPatternByte(null);
}
