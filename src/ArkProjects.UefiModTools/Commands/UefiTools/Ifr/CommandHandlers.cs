using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly SetupDataParser _setupDataParser;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, SetupDataParser setupDataParser)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _setupDataParser = setupDataParser;
    }

    public int ExtractSetupData(string inputFile, string ifrFile, string outputFile)
    {
        var setupData = _fileManager.ReadBytes(inputFile);
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        _logger.LogInformation("Read {setupDataSize} bytes of SetupData and {operationCount} IFR operations",
            setupData.Length, ifr.Operations.Count);

        var result = _setupDataParser.ExtractAll(ifr.Operations, setupData);

        _logger.LogInformation("Writing {questionCount} extracted SetupData questions to {outputFile}",
            result.Questions.Count, outputFile);
        _fileManager.Write(_jsonSerializer.Serialize(result), outputFile, true);
        return 0;
    }

    public int PatchSetupData(string inputFile, string patchFile, string outputFile)
    {
        var setupData = _fileManager.ReadBytes(inputFile);
        var patch = _jsonSerializer.Deserialize<ExtractedAmiSetupDataQuestions>(_fileManager.ReadString(patchFile));
        _logger.LogInformation("Read {setupDataSize} bytes of SetupData and {questionCount} questions from patch",
            setupData.Length, patch.Questions.Count);

        _setupDataParser.PatchAll(patch.Questions, setupData);

        _logger.LogInformation("Writing patched SetupData to {outputFile}", outputFile);
        _fileManager.Write(setupData, outputFile, true);
        return 0;
    }

    public int PatchSct(string inputFile, string ifrFile, string patchFile, string outputFile)
    {
        _logger.LogError("IFR Platform_setup.sct patching is not implemented");
        return 1;
    }
}
