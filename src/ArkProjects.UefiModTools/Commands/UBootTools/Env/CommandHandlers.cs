using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UBootTools.Env;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly UBootEnvParser _parser;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;

    public CommandHandlers(ILogger<CommandHandlers> logger, UBootEnvParser parser,
        IJsonSerializationService jsonSerializer, ICommandFileManager fileManager)
    {
        _logger = logger;
        _parser = parser;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
    }

    public int Scan(string inputFile, int[] blockSizes, int[] blocksWindows, string outputFile)
    {
        var dumpBytes = _fileManager.ReadBytes(inputFile);

        var variants = blockSizes.SelectMany(s => blocksWindows.Select(w => (s, w))).ToList();
        var result = new UBootScanResult();
        foreach (var (blockSize, blockWindow) in variants)
        {
            var badTail = dumpBytes.Length % blockSize;
            if (badTail != 0)
                _logger.LogWarning("Dump len not divided by block size! Last {count} bytes will be ignored", badTail);

            var pagesCount = (dumpBytes.Length - badTail) / blockSize;
            var scanResult = _parser.Scan(dumpBytes, pagesCount, blockSize, blockWindow);
            result.FoundEnvPages.AddRange(scanResult);
        }

        var scanResultJson = _jsonSerializer.Serialize(result);
        _fileManager.Write(scanResultJson, outputFile, true);
        return 0;
    }

    public int Read(string inputFile, string outputFile)
    {
        var ubootEnvBytes = _fileManager.ReadBytes(inputFile);
        var ubootEnv = _parser.Parse(ubootEnvBytes, false)!;
        var ubootEnvJson = _jsonSerializer.Serialize(ubootEnv);
        _fileManager.Write(ubootEnvJson, outputFile, true);
        return 0;
    }

    public int Write(string inputFile, string outputFile)
    {
        var ubootEnvJson = _fileManager.ReadString(inputFile);
        var ubootEnv = _jsonSerializer.Deserialize<UBootEnv>(ubootEnvJson);
        var ubootEnvBytes = _parser.Create(ubootEnv);
        _fileManager.Write(ubootEnvBytes, outputFile, true);
        return 0;
    }
}
