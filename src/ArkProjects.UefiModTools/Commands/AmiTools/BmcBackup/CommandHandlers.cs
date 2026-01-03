using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly AmiConfigBackupParser _parser;
    private readonly JsonSerializationService _jsonSerializer;

    public CommandHandlers(ILogger<CommandHandlers> logger, AmiConfigBackupParser parser,
        JsonSerializationService jsonSerializer)
    {
        _logger = logger;
        _parser = parser;
        _jsonSerializer = jsonSerializer;
    }

    public int PackBak(string inputDirectory, string outputFile)
    {
        var indexFile = Path.Combine(inputDirectory, "backup-info.json");
        var indexText = CommandHelpers.ReadString(indexFile, null, _logger);
        var index = _jsonSerializer.Deserialize<BackupInfoModel>(indexText);

        var files = new Dictionary<string, byte[]>();
        foreach (var fileName in index.Files)
        {
            var filePath = Path.IsPathRooted(fileName)
                ? Path.GetRelativePath("/", fileName)
                : fileName;
            filePath = Path.Combine(inputDirectory, filePath);
            _logger.LogInformation("Reading {file}", fileName);
            var fileBytes = CommandHelpers.ReadBytes(filePath, _logger);
            files[fileName] = fileBytes;
        }

        var bakFileBytes = _parser.CreateBackup(index, files);
        CommandHelpers.WriteResult(bakFileBytes, outputFile, true, _logger);
        return 0;
    }

    public int UnpackBak(string inputFile, string outputDirectory)
    {
        var backupBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var (info, files) = _parser.ParseBackup(backupBytes);

        foreach (var (fileName, fileBytes) in files)
        {
            var filePath = Path.IsPathRooted(fileName)
                ? Path.GetRelativePath("/", fileName)
                : fileName;
            filePath = Path.Combine(outputDirectory, filePath);

            _logger.LogInformation("Saving {file}", fileName);
            CommandHelpers.WriteResult(fileBytes, filePath, true, _logger);
        }

        var indexFile = Path.Combine(outputDirectory, "backup-info.json");
        var indexContent = _jsonSerializer.Serialize(info);
        CommandHelpers.WriteResult(indexContent, indexFile, true, _logger);
        return 0;
    }
}
