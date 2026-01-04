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
            var partitionLen = partition.EndAddress - partition.BeginAddress;
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
            var partitionLen = partition.EndAddress - partition.BeginAddress;
            var partitionFile = Path.Combine(partitionsDirectory, partition.FileName);

            _logger.LogInformation("Injecting {path}", partitionFile);
            var partitionBytes = _fileManager.ReadBytes(partitionFile);
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
}
