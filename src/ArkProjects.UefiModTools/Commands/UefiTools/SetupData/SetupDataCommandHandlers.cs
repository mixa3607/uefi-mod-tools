using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.Logging;
using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData;

public class SetupDataCommandHandlers
{
    private readonly ILogger<SetupDataCommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly ISerializationService _serializer;
    private readonly SetupDataIfrMapper _setupDataIfrMapper;
    private readonly SetupDataPatchApplier _patchApplier;

    public SetupDataCommandHandlers(ILogger<SetupDataCommandHandlers> logger, ICommandFileManager fileManager,
        ISerializationService serializer, SetupDataIfrMapper setupDataIfrMapper, SetupDataPatchApplier patchApplier)
    {
        _logger = logger;
        _fileManager = fileManager;
        _serializer = serializer;
        _setupDataIfrMapper = setupDataIfrMapper;
        _patchApplier = patchApplier;
    }

    public int MapIfr(string inputFile, string ifrFile, string outputFile, SerializationFormat outputFormat)
    {
        var setupData = _fileManager.ReadBytes(inputFile);
        var ifr = _serializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile), SerializationFormat.Auto);
        _logger.LogInformation("Read {setupDataSize} bytes of SetupData and {operationCount} IFR operations",
            setupData.Length, ifr.Operations.Count);

        var questions = _setupDataIfrMapper.ExtractAll(ifr.Operations, setupData);
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
        _fileManager.Write(_serializer.Serialize(result, outputFormat), outputFile, true);
        return 0;
    }

    public int PatchSetupData(string inputFile, string mapFile, string patchFile, string outputFile, bool ignoreVersions,
        bool ignoreChecksums)
    {
        var setupData = _fileManager.ReadBytes(inputFile);
        var map = _serializer.Deserialize<SetupDataMapDocument>(_fileManager.ReadString(mapFile), SerializationFormat.Auto);
        var patch = _serializer.Deserialize<SetupDataPatchDocument>(_fileManager.ReadString(patchFile), SerializationFormat.Auto);
        ValidateMap(map, ignoreVersions);
        ValidatePatch(patch, ignoreVersions);

        var setupDataSha256 = setupData.GetSha256String();
        if (!string.Equals(setupDataSha256, map.SetupDataSha256, StringComparison.OrdinalIgnoreCase) && !ignoreChecksums)
            throw new ArgumentException("SetupData input does not match the map source hash", nameof(inputFile));
        if (!string.Equals(setupDataSha256, map.SetupDataSha256, StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning("SetupData input does not match the map source hash; continuing because --ignore-checksums was specified");

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
