using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly FmhParser _parser;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;

    public CommandHandlers(ILogger<CommandHandlers> logger, FmhParser parser,
        IJsonSerializationService jsonSerializer, ICommandFileManager fileManager)
    {
        _logger = logger;
        _parser = parser;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
    }

    public int ScanFmh(string inputFile, int blockSize, string outputFile)
    {
        var dumpBytes = _fileManager.ReadBytes(inputFile);
        var sections = _parser.ScanFmh(dumpBytes, blockSize);
        var json = _jsonSerializer.Serialize(sections);
        _fileManager.Write(json, outputFile, true);
        return 0;
    }
}
