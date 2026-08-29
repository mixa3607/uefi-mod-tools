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
            var filePath = GetFilePathInDirectory(inputDirectory, fileName);
            _logger.LogInformation("Reading {file}", fileName);
            var fileBytes = _fileManager.ReadBytes(filePath);
            files[fileName] = fileBytes;
        }

        var bakFileBytes = _parser.CreateBackup(index, files);
        _fileManager.Write(bakFileBytes, outputFile, true);
        return 0;
    }

    public int UnpackBak(string inputFile, string outputDirectory, bool force)
    {
        var backupBytes = _fileManager.ReadBytes(inputFile);
        var (info, files) = _parser.ParseBackup(backupBytes, force);

        foreach (var (fileName, fileBytes) in files)
        {
            var filePath = GetFilePathInDirectory(outputDirectory, fileName);

            _logger.LogInformation("Saving {file}", fileName);
            _fileManager.Write(fileBytes, filePath, true);
        }

        var indexFile = Path.Combine(outputDirectory, "backup-info.json");
        var indexContent = _jsonSerializer.Serialize(info);
        _fileManager.Write(indexContent, indexFile, true);
        return 0;
    }

    private static string GetFilePathInDirectory(string directory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.IsPathRooted(fileName))
        {
            throw new ArgumentException("Backup file name must be a non-empty relative path", nameof(fileName));
        }

        var root = Path.GetFullPath(directory);
        var path = Path.GetFullPath(Path.Combine(root, fileName));
        var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Backup file path escapes its working directory: {fileName}", nameof(fileName));
        }

        return path;
    }
}
