using ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;
    private readonly FitParser _fitParser;

    public CommandHandlers(ILogger<CommandHandlers> logger, IJsonSerializationService jsonSerializer,
        ICommandFileManager fileManager, FitParser fitParser)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
        _fitParser = fitParser;
    }

    public int InjectMicrocodes(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fitTable = _fitParser.Read(fitBytes);
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _jsonSerializer.Deserialize<MicrocodesTable>(mTableJson);
        var (position, usableEnd) = MicrocodesTableUtilities.GetUsableRange(mTable, int.MaxValue);

        foreach (var mFile in mTable.MicrocodeFiles)
        {
            var mCodesFile = Path.Combine(mCodesDirectory, mFile);
            _logger.LogInformation("Injecting {path}", mCodesFile);
            var mCodesBytes = _fileManager.ReadBytes(mCodesFile);
            _logger.LogDebug("Read {count} bytes", mCodesBytes.Length);
            if (mCodesBytes.Length > usableEnd - position)
                throw new Exception("No space on payload section");

            var fwStart = checked((ulong)position + mTable.SectionBaseAddress);
            _logger.LogInformation("Add {file} as 0x{from:X8}", mFile, fwStart);

            var placeAt = fitTable.Entries.FindIndex(0, x => x.Type == FitEntryType.UnusedEntry);
            if (placeAt < 0)
            {
                _logger.LogError("Can not find any empty slot in FIT");
                throw new Exception("Can not find any empty slot in FIT");
            }

            _logger.LogInformation("Place FIT entry at index {idx}", placeAt);
            fitTable.Entries[placeAt] = new FitEntry
            {
                Type = FitEntryType.MicrocodeUpdateEntry,
                Address = fwStart,
                Size = 0,
                Version = 1,
                ChecksumValidate = false,
                Checksum = 0,
            };

            position = checked(position + mCodesBytes.Length);
        }

        _logger.LogInformation("Saving {path}", outputFile);
        _fileManager.Write(_fitParser.Write(fitTable), outputFile, true);
        return 0;
    }

    public int Read(string inputFile, string outputFile, bool verify)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fit = _fitParser.Read(fitBytes);

        if (!verify)
        {
            _logger.LogWarning("Skip repack verification!");
        }
        else
        {
            _logger.LogInformation("Verifying FIT by repacking...");
            var newFitBytes = _fitParser.Write(fit);
            if (!newFitBytes.SequenceEqual(fitBytes))
            {
                _logger.LogCritical("Repacked dump and source dump not equal!");
                throw new Exception("Repacked dump and source dump not equal!");
            }

            _logger.LogInformation("Repacking success! Old and new dumps will be equal");
        }

        _fileManager.Write(_jsonSerializer.Serialize(fit), outputFile, true);
        return 0;
    }

    public int Write(string inputFile, string outputFile)
    {
        var fit = _jsonSerializer.Deserialize<FitTable>(_fileManager.ReadString(inputFile));
        _fileManager.Write(_fitParser.Write(fit), outputFile, true);
        return 0;
    }
}
