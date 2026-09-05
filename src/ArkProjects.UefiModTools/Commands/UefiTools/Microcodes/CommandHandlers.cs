using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ISerializationService _serializer;
    private readonly ICommandFileManager _fileManager;
    private readonly MicrocodesCombiner _combiner;

    public CommandHandlers(ILogger<CommandHandlers> logger,
        ISerializationService serializer, ICommandFileManager fileManager, MicrocodesCombiner combiner)
    {
        _logger = logger;
        _serializer = serializer;
        _fileManager = fileManager;
        _combiner = combiner;
    }

    public int CombineMicrocodes(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var inputBytes = _fileManager.ReadBytes(inputFile);
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _serializer.Deserialize<MicrocodesTable>(mTableJson, SerializationFormat.Auto);

        var microcodes = new List<byte[]>();
        foreach (var mFile in mTable.MicrocodeFiles)
        {
            var mCodesFile = Path.Combine(mCodesDirectory, mFile);
            _logger.LogInformation("Injecting {path}", mCodesFile);
            var mCodesBytes = _fileManager.ReadBytes(mCodesFile);
            _logger.LogDebug("Read {count} bytes", mCodesBytes.Length);
            microcodes.Add(mCodesBytes);
        }

        _logger.LogInformation("Saving {path}", outputFile);
        _fileManager.Write(_combiner.Combine(inputBytes, mTable, microcodes), outputFile, true);
        return 0;
    }

}
