using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;
    public CommandHandlers(ILogger<CommandHandlers> logger,
        IJsonSerializationService jsonSerializer, ICommandFileManager fileManager)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
    }

    public int CombineMicrocodes(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var inputBytes = _fileManager.ReadBytes(inputFile);
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _jsonSerializer.Deserialize<MicrocodesTable>(mTableJson);

        if (mTable.UsableEnd < 0)
            _logger.LogWarning("UsableEnd not set. Use {end}", inputBytes.Length);
        var (position, usableEnd) = MicrocodesTableUtilities.GetUsableRange(mTable, inputBytes.Length);
        var usableStart = position;
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
            var fwStart = checked((ulong)position + mTable.SectionBaseAddress);
            var fwEnd = fwStart + (ulong)mCodesBytes.Length;
            _logger.LogInformation("Place {file} in range 0x{from:X8}-0x{to:X8}", mFile, fwStart, fwEnd);

            // ff
            position = checked(position + mCodesBytes.Length);
            freeSpace = usableEnd - position;
            _logger.LogInformation("Free space: {count}", freeSpace);
        }

        _logger.LogInformation("Saving {path}", outputFile);
        _fileManager.Write(inputBytes, outputFile, true);
        return 0;
    }

}
