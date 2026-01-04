using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly AmiConfigBackupParser _parser;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;

    public CommandHandlers(ILogger<CommandHandlers> logger, AmiConfigBackupParser parser,
        IJsonSerializationService jsonSerializer, ICommandFileManager fileManager)
    {
        _logger = logger;
        _parser = parser;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
    }

    public int PackBak(string inputDirectory, string outputFile)
    {
        var indexFile = Path.Combine(inputDirectory, "backup-info.json");
        var indexText = _fileManager.ReadString(indexFile);
        var index = _jsonSerializer.Deserialize<BackupInfoModel>(indexText);

        var files = new Dictionary<string, byte[]>();
        foreach (var fileName in index.Files)
        {
            var filePath = Path.IsPathRooted(fileName)
                ? Path.GetRelativePath("/", fileName)
                : fileName;
            filePath = Path.Combine(inputDirectory, filePath);
            _logger.LogInformation("Reading {file}", fileName);
            var fileBytes = _fileManager.ReadBytes(filePath);
            files[fileName] = fileBytes;
        }

        var bakFileBytes = _parser.CreateBackup(index, files);
        _fileManager.Write(bakFileBytes, outputFile, true);
        return 0;
    }

    public int UnpackBak(string inputFile, string outputDirectory)
    {
        var backupBytes = _fileManager.ReadBytes(inputFile);
        var (info, files) = _parser.ParseBackup(backupBytes);

        foreach (var (fileName, fileBytes) in files)
        {
            var filePath = Path.IsPathRooted(fileName)
                ? Path.GetRelativePath("/", fileName)
                : fileName;
            filePath = Path.Combine(outputDirectory, filePath);

            _logger.LogInformation("Saving {file}", fileName);
            _fileManager.Write(fileBytes, filePath, true);
        }

        var indexFile = Path.Combine(outputDirectory, "backup-info.json");
        var indexContent = _jsonSerializer.Serialize(info);
        _fileManager.Write(indexContent, indexFile, true);
        return 0;
    }
}
