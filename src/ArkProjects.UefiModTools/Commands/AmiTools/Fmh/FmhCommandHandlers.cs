using System.Runtime.InteropServices;
using System.Text;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.Fmh;

public class FmhCommandHandlers
{
    private readonly ILogger<FmhCommandHandlers> _logger;

    public FmhCommandHandlers(ILogger<FmhCommandHandlers> logger)
    {
        _logger = logger;
    }

    public int ExtractFmh(string inputFile, int blockSize)
    {
        var dumpBytes = CommandHelpers.ReadBytes(inputFile, _logger).AsSpan();
        if (dumpBytes.Length % blockSize != 0)
        {
            throw new Exception("Dump len not divided by block size!");
        }

        var magicBytes = Encoding.ASCII.GetBytes("$MODULE$");
        for (int i = 0; i < dumpBytes.Length / blockSize; i++)
        {
            var blk = dumpBytes.Slice(i * blockSize, blockSize);

            var beginMagicBytes = blk.Slice(0, magicBytes.Length);
            if (beginMagicBytes.SequenceEqual(magicBytes))
            {
                var fmhSize = Marshal.SizeOf<AmiFlashModuleHeader>();
                var fmhBytes = blk.Slice(0, fmhSize);
                var fmh = FromBytes<AmiFlashModuleHeader>(fmhBytes);

                _logger.LogInformation("Found module {name} 0x{from:X8}-0x{to:X8}",
                    Encoding.ASCII.GetString(fmh.ModuleInfo.Name), fmh.ModuleInfo.Location,
                    fmh.ModuleInfo.Size + fmh.ModuleInfo.Location);
            }

            var endMagicBytes = blk.Slice(blockSize - magicBytes.Length, magicBytes.Length);
            if (endMagicBytes.SequenceEqual(magicBytes))
            {
                var fmhTailSize = Marshal.SizeOf<AmiFlashModuleHeaderTailed>();
                var fmhTailBytes = blk.Slice(blockSize - fmhTailSize, fmhTailSize);
                var fmhTail = FromBytes<AmiFlashModuleHeaderTailed>(fmhTailBytes);

                var fmhSize = Marshal.SizeOf<AmiFlashModuleHeader>();
                var fmhBytes = dumpBytes.Slice((int)fmhTail.LinkAddress, fmhSize);
                var fmh = FromBytes<AmiFlashModuleHeader>(fmhBytes);

                _logger.LogInformation("Found module {name} 0x{from:X8}-0x{to:X8}",
                    Encoding.ASCII.GetString(fmh.ModuleInfo.Name), fmh.ModuleInfo.Location,
                    fmh.ModuleInfo.Size + fmh.ModuleInfo.Location);
            }
        }

        return 0;
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