using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class FmhParser
{
    private static readonly byte[] FmhSignature = Encoding.ASCII.GetBytes("$MODULE$");
    private static readonly int FmhTailSizeOf = Marshal.SizeOf<AmiFlashModuleHeaderTailed>();
    private static readonly int FmhSizeOf = Marshal.SizeOf<AmiFlashModuleHeader>();

    private readonly ILogger<FmhParser> _logger;

    public FmhParser(ILogger<FmhParser> logger)
    {
        _logger = logger;
    }

    public List<IFmhSectionModel> ScanFmh(byte[] flashBytes, int blockSize)
    {
        if (flashBytes.Length % blockSize != 0)
            throw new Exception("Dump len not divided by block size!");

        var sections = new List<IFmhSectionModel>();
        for (int i = 0; i < flashBytes.Length / blockSize; i++)
        {
            sections.AddRange(ScanPage(flashBytes, i, blockSize));
        }

        return sections;
    }

    public IReadOnlyList<IFmhSectionModel> ScanPage(byte[] flashBytes, int page, int blockSize)
    {
        var sections = new List<IFmhSectionModel>();
        var pageRange = new Range(page * blockSize, page * blockSize + blockSize);

        var fromTail = ReadFmhTailed(flashBytes, pageRange);
        if (fromTail != null)
        {
            sections.Add(fromTail);
            var fromMidRange = new Range(fromTail.PointingToAddress, FmhSizeOf);
            var fromMid = ReadFmh(flashBytes, fromMidRange);
            if (fromMid == null)
            {
                _logger.LogWarning("FMH not found in 0x{being}-0x{end}",
                    fromMidRange.Start.Value, fromMidRange.End.Value);
            }
            else
            {
                sections.Add(fromMid);
            }
        }

        var fromBegin = ReadFmh(flashBytes, pageRange);
        if (fromBegin != null)
        {
            sections.Add(fromBegin);
        }

        return sections;
    }

    private FmhTailSectionModel? ReadFmhTailed(byte[] flashBytes, Range range)
    {
        range = new Range(range.End.Value - FmhTailSizeOf, range.End);

        var bytes = flashBytes.AsSpan(range);
        if (!bytes.EndsWith(FmhSignature))
            return null;
        var fmhTail = FromBytes<AmiFlashModuleHeaderTailed>(bytes);

        var sct = new FmhTailSectionModel()
        {
            BeginAddress = range.Start.Value,
            EndAddress = range.End.Value,
            PointingToAddress = (int)fmhTail.LinkAddress,
        };
        _logger.LogInformation("Found FMH tail in 0x{being}-0x{end} that pointing to 0x{addr:X8}",
            sct.BeginAddress, sct.EndAddress, sct.PointingToAddress);

        return sct;
    }

    private FmhSectionModel? ReadFmh(byte[] flashBytes, Range range)
    {
        range = new Range(range.Start.Value, range.Start.Value + FmhSizeOf);

        var bytes = flashBytes.AsSpan(range);
        if (!bytes.StartsWith(FmhSignature))
            return null;
        var fmh = FromBytes<AmiFlashModuleHeader>(bytes);

        var sct = new FmhSectionModel()
        {
            BeginAddress = range.Start.Value,
            EndAddress = range.End.Value,
            ModuleBeginAddress = (int)fmh.ModuleInfo.Location,
            ModuleEndAddress = (int)(fmh.ModuleInfo.Location + fmh.ModuleInfo.Size),
            ModuleName = Encoding.ASCII.GetString(fmh.ModuleInfo.Name)
        };

        _logger.LogInformation(
            "Found FMH in 0x{being}-0x{end} that pointing to module {name} 0x{mBegin:X8}-0x{mEnd:X8}",
            sct.BeginAddress, sct.EndAddress, sct.ModuleName, sct.ModuleBeginAddress, sct.ModuleEndAddress);

        return sct;
    }

    private static T FromBytes<T>(Span<byte> span) where T : struct
    {
        var data = span.ToArray();
        var offset = 0;

        var size = Marshal.SizeOf<T>();
        if (offset + size > data.Length)
            throw new ArgumentException("Недостаточно данных");

        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(data, offset, ptr, size);
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
