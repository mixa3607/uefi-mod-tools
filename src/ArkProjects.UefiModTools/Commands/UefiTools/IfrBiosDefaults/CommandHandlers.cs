using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly NvarMapExtractor _extractor;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, NvarMapExtractor extractor)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _extractor = extractor;
    }

    public int Extract(string inputFile, string outputFile)
    {
        var defaultsFileBytes = _fileManager.ReadBytes(inputFile);
        _logger.LogInformation("Read {size} bytes of BIOS defaults from {inputFile}", defaultsFileBytes.Length, inputFile);

        var result = new BiosDefaultsMapDocument
        {
            Version = BiosDefaultsMapDocument.SupportedVersion,
            Type = BiosDefaultsMapDocument.SupportedType,

            Variables = _extractor.Extract(defaultsFileBytes),
            SourceName = Path.GetFileName(inputFile),
            SourceSha256 = Convert.ToHexString(SHA256.HashData(defaultsFileBytes)).ToLowerInvariant(),
        };

        _logger.LogInformation("Writing {count} BIOS defaults variables to {outputFile}", result.Variables.Count, outputFile);
        _fileManager.Write(_jsonSerializer.Serialize(result), outputFile, true);
        return 0;
    }
}
