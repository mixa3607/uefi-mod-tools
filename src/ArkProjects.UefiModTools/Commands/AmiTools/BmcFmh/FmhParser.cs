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
            var fromMid = ReadFmh(flashBytes, pageRange, fromTail.PointingToAddress);
            if (fromMid == null)
            {
                _logger.LogWarning("FMH not found at 0x{being:X8}", fromTail.PointingToAddress);
            }
            else
            {
                sections.Add(fromMid);
            }
        }

        var fromBegin = ReadFmh(flashBytes, pageRange, pageRange.Start.Value);
        if (fromBegin != null)
        {
            sections.Add(fromBegin);
        }

        return sections;
    }

    private FmhTailSectionModel? ReadFmhTailed(byte[] flashBytes, Range page)
    {
        var fmhBytes = new Range(page.End.Value - FmhTailSizeOf, page.End);

        var bytes = flashBytes.AsSpan(fmhBytes);
        if (!bytes.EndsWith(FmhSignature))
            return null;
        var fmhTail = FromBytes<AmiFlashModuleHeaderTailed>(bytes);

        var sct = new FmhTailSectionModel()
        {
            BeginAddress = fmhBytes.Start.Value,
            EndAddress = fmhBytes.End.Value,
            PointingToAddress = (int)fmhTail.LinkAddress,
        };
        _logger.LogInformation("Found FMH tail in 0x{being:X8}-0x{end:X8} that pointing to 0x{addr:X8}",
            sct.BeginAddress, sct.EndAddress, sct.PointingToAddress);

        return sct;
    }

    private FmhSectionModel? ReadFmh(byte[] flashBytes, Range page, int fmhStart)
    {
        var fmhBytes = new Range(fmhStart, fmhStart + FmhSizeOf);

        var bytes = flashBytes.AsSpan(fmhBytes);
        if (!bytes.StartsWith(FmhSignature))
            return null;
        var fmh = FromBytes<AmiFlashModuleHeader>(bytes);

        var sectionPointerRange = new Range(
            page.Start.Value + (int)fmh.ModuleInfo.Location,
            page.Start.Value + (int)fmh.ModuleInfo.Location + (int)fmh.ModuleInfo.Size
        );
        var sct = new FmhSectionModel()
        {
            BeginAddress = fmhBytes.Start.Value,
            EndAddress = fmhBytes.End.Value,
            ModuleBeginAddress = sectionPointerRange.Start.Value,
            ModuleEndAddress = sectionPointerRange.End.Value,
            ModuleName = Encoding.ASCII.GetString(fmh.ModuleInfo.Name).TrimEnd('\x00')
        };

        _logger.LogInformation(
            "Found FMH in 0x{being:X8}-0x{end:X8} that pointing to module {name} 0x{mBegin:X8}-0x{mEnd:X8}",
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
#pragma warning disable IL2091
            return Marshal.PtrToStructure<T>(ptr);
#pragma warning restore IL2091
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
