using ArkProjects.UefiModTools.Commands.BinTools.Models;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.BinTools;

public class BinToolsCommandHandlers
{
    private readonly ILogger<BinToolsCommandHandlers> _logger;
    private readonly JsonSerializationService _jsonSerializer;

    public BinToolsCommandHandlers(ILogger<BinToolsCommandHandlers> logger,
        JsonSerializationService jsonSerializer)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    public int SplitBin(string inputFile, string partitionsTableFile, string outputDirectory)
    {
        var inputBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var pTableJson = CommandHelpers.ReadString(partitionsTableFile, null, _logger);
        var pTable = _jsonSerializer.Deserialize<PartitionsTable>(pTableJson);

        foreach (var partition in pTable.Partitions)
        {
            var partitionLen = partition.EndAddress - partition.BeginAddress;
            var saveToFile = Path.Combine(outputDirectory, partition.FileName);
            _logger.LogInformation("Saving {path}", saveToFile);
            var bytes = inputBytes.AsSpan(partition.BeginAddress, partitionLen).ToArray();
            CommandHelpers.WriteResult(bytes, saveToFile, true, _logger);
        }

        return 0;
    }

    public int CombineBin(string inputFile, string partitionsTableFile, string partitionsDirectory, string outputFile)
    {
        var inputBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var pTableJson = CommandHelpers.ReadString(partitionsTableFile, null, _logger);
        var pTable = _jsonSerializer.Deserialize<PartitionsTable>(pTableJson);

        foreach (var partition in pTable.Partitions)
        {
            var partitionLen = partition.EndAddress - partition.BeginAddress;
            var partitionFile = Path.Combine(partitionsDirectory, partition.FileName);

            _logger.LogInformation("Injecting {path}", partitionFile);
            var partitionBytes = CommandHelpers.ReadBytes(partitionFile, _logger);
            if (partitionBytes.Length > partitionLen)
            {
                throw new Exception($"Partition max len is {partitionLen} but read {partitionBytes.Length}");
            }

            if (partitionBytes.Length < partitionLen)
            {
                var padLen = partitionLen - partitionBytes.Length;
                _logger.LogWarning("Partition len is {len} but read {read}, adding {pad} to end",
                    partitionLen, partitionBytes.Length, padLen);
                partitionBytes = partitionBytes.Concat(Enumerable.Repeat(partition.PadByte, padLen)).ToArray();
            }

            Array.Copy(partitionBytes, 0, inputBytes, partition.BeginAddress, partitionLen);
        }

        _logger.LogInformation("Saving {path}", outputFile);
        CommandHelpers.WriteResult(inputBytes, outputFile, true, _logger);

        return 0;
    }
}
