using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UBootTools;

public class UBootEnvCommandHandlers
{
    private readonly ILogger<UBootEnvCommandHandlers> _logger;
    private readonly UBootEnvParser _parser;
    private readonly JsonSerializationService _jsonSerializer;

    public UBootEnvCommandHandlers(ILogger<UBootEnvCommandHandlers> logger, UBootEnvParser parser,
        JsonSerializationService jsonSerializer)
    {
        _logger = logger;
        _parser = parser;
        _jsonSerializer = jsonSerializer;
    }

    public int Read(string inputFile, string outputFile)
    {
        var ubootEnvBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var ubootEnv = _parser.Parse(ubootEnvBytes);
        var ubootEnvJson = _jsonSerializer.Serialize(ubootEnv);
        CommandHelpers.WriteResult(ubootEnvJson, outputFile, true, _logger);
        return 0;
    }

    public int Write(string inputFile, string outputFile)
    {
        var ubootEnvJson = CommandHelpers.ReadString(inputFile, null, _logger);
        var ubootEnv = _jsonSerializer.Deserialize<UBootEnv>(ubootEnvJson);
        var ubootEnvBytes = _parser.Create(ubootEnv);
        CommandHelpers.WriteResult(ubootEnvBytes, outputFile, true, _logger);
        return 0;
    }
}
