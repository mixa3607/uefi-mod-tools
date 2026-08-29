using ArkProjects.UefiModTools.Commands.BinTools.Models;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.BinTools;

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

    public int SplitBin(string inputFile, string partitionsTableFile, string outputDirectory)
    {
        var inputBytes = _fileManager.ReadBytes(inputFile);
        var pTableJson = _fileManager.ReadString(partitionsTableFile);
        var pTable = _jsonSerializer.Deserialize<PartitionsTable>(pTableJson);

        foreach (var partition in pTable.Partitions)
        {
            var partitionLen = GetPartitionLength(partition, inputBytes.Length);
            var saveToFile = Path.Combine(outputDirectory, partition.FileName);
            _logger.LogInformation("Saving {path}", saveToFile);
            var bytes = inputBytes.AsSpan(partition.BeginAddress, partitionLen).ToArray();
            _fileManager.Write(bytes, saveToFile, true);
        }

        return 0;
    }

    public int CombineBin(string inputFile, string partitionsTableFile, string partitionsDirectory, string outputFile)
    {
        var inputBytes = _fileManager.ReadBytes(inputFile);
        var pTableJson = _fileManager.ReadString(partitionsTableFile);
        var pTable = _jsonSerializer.Deserialize<PartitionsTable>(pTableJson);

        foreach (var partition in pTable.Partitions)
        {
            var partitionFile = Path.Combine(partitionsDirectory, partition.FileName);
            _logger.LogInformation("Injecting {path}", partitionFile);

            var partitionLen = GetPartitionLength(partition, inputBytes.Length);
            _logger.LogDebug("Partition len {count} bytes", partitionLen);

            var partitionBytes = _fileManager.ReadBytes(partitionFile);
            _logger.LogDebug("Read {count} bytes", partitionBytes.Length);

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
        _fileManager.Write(inputBytes, outputFile, true);

        return 0;
    }

    private static int GetPartitionLength(Partition partition, int inputLength)
    {
        if (partition.BeginAddress < 0 || partition.EndAddress < partition.BeginAddress ||
            partition.EndAddress > inputLength)
        {
            throw new ArgumentException(
                $"Partition {partition.FileName} range 0x{partition.BeginAddress:X8}-0x{partition.EndAddress:X8} " +
                $"is outside the input file");
        }

        return partition.EndAddress - partition.BeginAddress;
    }
}
