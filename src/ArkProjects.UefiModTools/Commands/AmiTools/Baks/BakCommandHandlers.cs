using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.Baks;

public class BakCommandHandlers
{
    private readonly ILogger<BakCommandHandlers> _logger;
    private readonly AmiConfigBackupParser _parser;

    public BakCommandHandlers(ILogger<BakCommandHandlers> logger, AmiConfigBackupParser parser)
    {
        _logger = logger;
        _parser = parser;
    }

    public int PackBak(string inputDirectory, string outputFile, bool isBuggedSha1)
    {
        var indexFile = Path.Combine(inputDirectory, ".files-list");
        var indexList = CommandHelpers.ReadString(indexFile, null, _logger)
            .Split('\n', '\r')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        var files = new Dictionary<string, byte[]>();
        foreach (var fileName in indexList)
        {
            var filePath = Path.IsPathRooted(fileName)
                ? Path.GetRelativePath("/", fileName)
                : fileName;
            filePath = Path.Combine(inputDirectory, filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            files[fileName] = fileBytes;
        }

        var bakFileBytes = _parser.CreateBackup(files, isBuggedSha1);
        CommandHelpers.WriteResult(bakFileBytes, outputFile, true, _logger);
        return 0;
    }

    public int UnpackBak(string inputFile, string outputDirectory)
    {
        var backupBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var files = _parser.ParseBackup(backupBytes);

        foreach (var (fileName, fileBytes) in files)
        {
            var filePath = Path.IsPathRooted(fileName)
                ? Path.GetRelativePath("/", fileName)
                : fileName;
            filePath = Path.Combine(outputDirectory, filePath);

            CommandHelpers.WriteResult(fileBytes, filePath, true, _logger);
        }

        var indexFile = Path.Combine(outputDirectory, ".files-list");
        var indexContent = string.Join("\n", files.Select(x => x.Key));
        CommandHelpers.WriteResult(indexContent, indexFile, true, _logger);
        return 0;
    }
}