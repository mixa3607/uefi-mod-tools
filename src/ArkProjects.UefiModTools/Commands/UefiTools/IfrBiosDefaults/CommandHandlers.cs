using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly NvarMapExtractor _extractor;
    private readonly BiosDefaultsStoreMapper _storeMapper;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, NvarMapExtractor extractor, BiosDefaultsStoreMapper storeMapper)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _extractor = extractor;
        _storeMapper = storeMapper;
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

    public int MapStore(string inputFile, string ifrFile, string outputFile)
    {
        var biosDefaultsMap = _jsonSerializer.Deserialize<BiosDefaultsMapDocument>(_fileManager.ReadString(inputFile));
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        _logger.LogInformation(
            "Read {variableCount} NVAR variables from {inputFile} and {operationCount} IFR operations from {ifrFile}",
            biosDefaultsMap.Variables.Count, inputFile, ifr.Operations.Count, ifrFile);

        var result = _storeMapper.Map(biosDefaultsMap, ifr);

        _logger.LogInformation("Writing {mappingCount} BIOS defaults store mappings to {outputFile}",
            result.QuestionMappings.Count, outputFile);
        _fileManager.Write(_jsonSerializer.Serialize(result), outputFile, true);
        return 0;
    }
}
