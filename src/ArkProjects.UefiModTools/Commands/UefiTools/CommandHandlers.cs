using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;
    private readonly FitParser _fitParser;

    public CommandHandlers(ILogger<CommandHandlers> logger,
        IJsonSerializationService jsonSerializer, ICommandFileManager fileManager, FitParser fitParser)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
        _fitParser = fitParser;
    }

    public int InjectMicrocodes2Fit(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fitTable = _fitParser.Read(fitBytes);
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _jsonSerializer.Deserialize<MicrocodesTable>(mTableJson);

        var position = mTable.UsableStart;
        foreach (var mFile in mTable.MicrocodeFiles)
        {
            var mCodesFile = Path.Combine(mCodesDirectory, mFile);
            _logger.LogInformation("Injecting {path}", mCodesFile);
            var mCodesBytes = _fileManager.ReadBytes(mCodesFile);
            _logger.LogDebug("Read {count} bytes", mCodesBytes.Length);


            // copy
            var fwStart = position + mTable.SectionBaseAddress;
            _logger.LogInformation("Add {file} as 0x{from:X8}", mFile, fwStart);

            var placeAt = fitTable.Entries.FindIndex(0, x => x.Type == FitEntryType.MicrocodeUpdateEntry);
            if (placeAt < 0)
            {
                _logger.LogError("Can not find any empty slot in FIT");
                throw new Exception("Can not find any empty slot in FIT");
            }

            _logger.LogInformation("Place FIT entry at index {idx}", placeAt);
            var fitEntry = new FitEntry()
            {
                Type = FitEntryType.MicrocodeUpdateEntry,
                Size = 0x00,
                Version = 0x01,
                ChecksumValidate = false,
                Checksum = 0x00,
            };
            fitTable.Entries[placeAt] = fitEntry;

            // ff
            position += mCodesBytes.Length;
        }

        fitBytes = _fitParser.Write(fitTable);

        _logger.LogInformation("Saving {path}", outputFile);
        _fileManager.Write(fitBytes, outputFile, true);

        return 0;
    }

    public int CombineMicrocodes(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var inputBytes = _fileManager.ReadBytes(inputFile);
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _jsonSerializer.Deserialize<MicrocodesTable>(mTableJson);

        var usableStart = mTable.UsableStart;
        var usableEnd = mTable.UsableEnd;
        if (usableEnd < 0)
        {
            usableEnd = inputBytes.Length;
            _logger.LogWarning("UsableEnd not set. Use {end}", usableEnd);
        }

        var position = usableStart;
        foreach (var mFile in mTable.MicrocodeFiles)
        {
            var mCodesFile = Path.Combine(mCodesDirectory, mFile);
            _logger.LogInformation("Injecting {path}", mCodesFile);
            var mCodesBytes = _fileManager.ReadBytes(mCodesFile);
            _logger.LogDebug("Read {count} bytes", mCodesBytes.Length);

            var freeSpace = usableEnd - position;
            if (mCodesBytes.Length > freeSpace)
            {
                _logger.LogError("Try write {try} bytes but free space is {free}", mCodesBytes.Length, freeSpace);
                throw new Exception("No space on payload section");
            }

            // copy
            Array.Copy(mCodesBytes, 0, inputBytes, position, mCodesBytes.Length);
            var fwStart = position + mTable.SectionBaseAddress;
            var fwEnd = fwStart + mCodesBytes.Length;
            _logger.LogInformation("Place {file} in range 0x{from:X8}-0x{to:X8}", mFile, fwStart, fwEnd);

            // ff
            position += mCodesBytes.Length;
            freeSpace = usableEnd - usableStart - position;
            _logger.LogInformation("Free space: {count}", freeSpace);
        }

        _logger.LogInformation("Saving {path}", outputFile);
        _fileManager.Write(inputBytes, outputFile, true);
        return 0;
    }

    public int ReadFit(string inputFile, string outputFile)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fit = _fitParser.Read(fitBytes);
        var fitJson = _jsonSerializer.Serialize(fit);
        _fileManager.Write(fitJson, outputFile, true);
        return 0;
    }

    public int WriteFit(string inputFile, string outputFile)
    {
        var fitJson = _fileManager.ReadString(inputFile);
        var fit = _jsonSerializer.Deserialize<FitTable>(fitJson);
        var fitBytes = _fitParser.Write(fit);
        _fileManager.Write(fitBytes, outputFile, true);
        return 0;
    }
}
