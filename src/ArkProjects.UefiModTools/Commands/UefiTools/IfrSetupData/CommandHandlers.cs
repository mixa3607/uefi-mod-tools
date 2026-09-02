using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly SetupDataParser _setupDataParser;
    private readonly SetupDataPatchApplier _patchApplier;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, SetupDataParser setupDataParser, SetupDataPatchApplier patchApplier)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _setupDataParser = setupDataParser;
        _patchApplier = patchApplier;
    }

    public int MapIfr(string inputFile, string ifrFile, string outputFile)
    {
        var setupData = _fileManager.ReadBytes(inputFile);
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        _logger.LogInformation("Read {setupDataSize} bytes of SetupData and {operationCount} IFR operations",
            setupData.Length, ifr.Operations.Count);

        var questions = _setupDataParser.ExtractAll(ifr.Operations, setupData);
        var result = new SetupDataMapDocument
        {
            Questions = questions, 
            SetupDataSha256 = setupData.GetSha256String(),
            IfrSha256 = ifr.InputSha256,
            Version = SetupDataMapDocument.SupportedVersion,
            Type = SetupDataMapDocument.SupportedType
        };

        _logger.LogInformation("Writing {questionCount} extracted SetupData questions to {outputFile}",
            result.Questions.Count, outputFile);
        _fileManager.Write(_jsonSerializer.Serialize(result), outputFile, true);
        return 0;
    }

    public int PatchSetupData(string inputFile, string mapFile, string patchFile, string outputFile, bool ignoreVersions)
    {
        var setupData = _fileManager.ReadBytes(inputFile);
        var map = _jsonSerializer.Deserialize<SetupDataMapDocument>(_fileManager.ReadString(mapFile));
        var patch = _jsonSerializer.Deserialize<SetupDataPatchDocument>(_fileManager.ReadString(patchFile));
        ValidateMap(map, ignoreVersions);
        ValidatePatch(patch, ignoreVersions);

        var setupDataSha256 = setupData.GetSha256String();
        if (!string.Equals(setupDataSha256, map.SetupDataSha256, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("SetupData input does not match the map source hash", nameof(inputFile));

        _logger.LogInformation("Read {setupDataSize} bytes of SetupData, {questionCount} mapped questions, and {patchCount} patches",
            setupData.Length, map.Questions.Count, patch.Questions.Count);

        _patchApplier.Apply(setupData, map.Questions, patch.Questions);

        _logger.LogInformation("Writing patched SetupData to {outputFile}", outputFile);
        _fileManager.Write(setupData, outputFile, true);
        return 0;
    }

    private void ValidateMap(SetupDataMapDocument map, bool ignoreVersions)
    {
        if (map.Type != SetupDataMapDocument.SupportedType)
            throw new ArgumentException(
                $"Expected SetupData map type {SetupDataMapDocument.SupportedType}, but got {map.Type}. " +
                "--ignore-versions cannot ignore a different document type.", nameof(map));

        if (map.Version == SetupDataMapDocument.SupportedVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "SetupData map version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                map.Version, SetupDataMapDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected SetupData map version {SetupDataMapDocument.SupportedVersion}, but got {map.Version}. " +
            "Use --ignore-versions only when the map schema is known to be compatible.", nameof(map));
    }

    private void ValidatePatch(SetupDataPatchDocument patch, bool ignoreVersions)
    {
        if (patch.Type != SetupDataPatchDocument.SupportedType)
            throw new ArgumentException(
                $"Expected SetupData patch type {SetupDataPatchDocument.SupportedType}, but got {patch.Type}. " +
                "--ignore-versions cannot ignore a different document type.", nameof(patch));

        if (patch.Version == SetupDataPatchDocument.SupportedVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "SetupData patch version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                patch.Version, SetupDataPatchDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected SetupData patch version {SetupDataPatchDocument.SupportedVersion}, but got {patch.Version}. " +
            "Use --ignore-versions only when the patch schema is known to be compatible.", nameof(patch));
    }
}
