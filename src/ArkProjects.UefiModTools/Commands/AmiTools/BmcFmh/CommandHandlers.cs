using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly FmhParser _parser;
    private readonly JsonSerializationService _jsonSerializer;

    public CommandHandlers(ILogger<CommandHandlers> logger, FmhParser parser,
        JsonSerializationService jsonSerializer)
    {
        _logger = logger;
        _parser = parser;
        _jsonSerializer = jsonSerializer;
    }

    public int ScanFmh(string inputFile, int blockSize, string outputFile)
    {
        var dumpBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var sections = _parser.ScanFmh(dumpBytes, blockSize);
        var json = _jsonSerializer.Serialize(sections);
        CommandHelpers.WriteResult(json, outputFile, true, _logger);
        return 0;
    }
}
