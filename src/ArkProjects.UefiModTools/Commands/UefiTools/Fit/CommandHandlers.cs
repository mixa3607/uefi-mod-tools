using ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;
    private readonly FitParser _fitParser;
    private readonly FitMicrocodesInjector _microcodesInjector;

    public CommandHandlers(ILogger<CommandHandlers> logger, IJsonSerializationService jsonSerializer,
        ICommandFileManager fileManager, FitParser fitParser, FitMicrocodesInjector microcodesInjector)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
        _fitParser = fitParser;
        _microcodesInjector = microcodesInjector;
    }

    public int InjectMicrocodes(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fitTable = _fitParser.Read(fitBytes);
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _jsonSerializer.Deserialize<MicrocodesTable>(mTableJson);
        var microcodes = new List<byte[]>();
        foreach (var mFile in mTable.MicrocodeFiles)
        {
            var mCodesFile = Path.Combine(mCodesDirectory, mFile);
            _logger.LogInformation("Injecting {path}", mCodesFile);
            var mCodesBytes = _fileManager.ReadBytes(mCodesFile);
            _logger.LogDebug("Read {count} bytes", mCodesBytes.Length);
            microcodes.Add(mCodesBytes);
        }

        _logger.LogInformation("Saving {path}", outputFile);
        _fileManager.Write(_fitParser.Write(_microcodesInjector.Inject(fitTable, mTable, microcodes)), outputFile, true);
        return 0;
    }

    public int Read(string inputFile, string outputFile, bool verify)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fit = _fitParser.Read(fitBytes);

        if (!verify)
        {
            _logger.LogWarning("Skip repack verification!");
        }
        else
        {
            _logger.LogInformation("Verifying FIT by repacking...");
            var newFitBytes = _fitParser.Write(fit);
            if (!newFitBytes.SequenceEqual(fitBytes))
            {
                _logger.LogCritical("Repacked dump and source dump not equal!");
                throw new Exception("Repacked dump and source dump not equal!");
            }

            _logger.LogInformation("Repacking success! Old and new dumps will be equal");
        }

        _fileManager.Write(_jsonSerializer.Serialize(fit), outputFile, true);
        return 0;
    }

    public int Write(string inputFile, string outputFile)
    {
        var fit = _jsonSerializer.Deserialize<FitTable>(_fileManager.ReadString(inputFile));
        _fileManager.Write(_fitParser.Write(fit), outputFile, true);
        return 0;
    }
}
