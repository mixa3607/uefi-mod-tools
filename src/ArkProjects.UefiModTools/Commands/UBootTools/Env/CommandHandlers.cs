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

    public int Scan(string inputFile, int blockSize, string outputFile)
    {
        var dumpBytes = _fileManager.ReadBytes(inputFile);
        if (dumpBytes.Length % blockSize != 0)
            throw new Exception("Dump len not divided by block size!");

        var result = new UBootScanResult();
        for (int i = 0; i < dumpBytes.Length / blockSize; i++)
        {
            var pageRange = new Range(i * blockSize, i * blockSize + blockSize);
            var page = dumpBytes
                .AsSpan(pageRange)
                .ToArray();
            var env = _parser.Parse(page, true);
            if (env == null)
                continue;

            _logger.LogInformation("Found potential env section in page 0x{start:X8}-0x{end:X8}",
                pageRange.Start.Value, pageRange.End.Value);
            result.FoundEnvPages.Add(new UBootEnvInDump()
            {
                BeginAddress = pageRange.Start.Value,
                EndAddress = pageRange.End.Value,
                Variables = env.Variables,
            });
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
