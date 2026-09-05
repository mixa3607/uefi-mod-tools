using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public class FitCommandHandlers
{
    private readonly ILogger<FitCommandHandlers> _logger;
    private readonly ISerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;
    private readonly FitParser _fitParser;
    private readonly FitMapper _fitMapper;
    private readonly FitPatchApplier _fitPatchApplier;

    public FitCommandHandlers(ILogger<FitCommandHandlers> logger, ISerializationService jsonSerializer,
        ICommandFileManager fileManager, FitParser fitParser, FitMapper fitMapper, FitPatchApplier fitPatchApplier)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
        _fitParser = fitParser;
        _fitMapper = fitMapper;
        _fitPatchApplier = fitPatchApplier;
    }

    public int Map(string inputFile, string outputFile, SerializationFormat outputFormat)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var fitTable = _fitParser.Read(fitBytes);
        var map = new FitMapDocument
        {
            Version = FitMapDocument.SupportedVersion,
            Type = FitMapDocument.SupportedType,
            FitSha256 = fitBytes.GetSha256String(),
            TableOffset = fitTable.HeadGarbage.Length,
            Entries = _fitMapper.Extract(fitTable),
        };

        _logger.LogInformation("Writing {entryCount} FIT entries to {outputFile}", map.Entries.Count, outputFile);
        _fileManager.Write(_jsonSerializer.Serialize(map, outputFormat), outputFile, true);
        return 0;
    }

    public int ApplyPatch(string inputFile, string mapFile, string patchFile, string outputFile, bool ignoreVersions,
        bool ignoreChecksums)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var map = _jsonSerializer.Deserialize<FitMapDocument>(_fileManager.ReadString(mapFile), SerializationFormat.Auto);
        var patch = _jsonSerializer.Deserialize<FitPatchDocument>(_fileManager.ReadString(patchFile), SerializationFormat.Auto);
        ValidateMap(map, ignoreVersions);
        ValidatePatch(patch, ignoreVersions);

        var fitSha256 = fitBytes.GetSha256String();
        if (!string.Equals(fitSha256, map.FitSha256, StringComparison.OrdinalIgnoreCase) && !ignoreChecksums)
            throw new ArgumentException("FIT input does not match the map source hash", nameof(inputFile));
        if (!string.Equals(fitSha256, map.FitSha256, StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning("FIT input does not match the map source hash; continuing because --ignore-checksums was specified");

        var fitTable = _fitParser.Read(fitBytes);
        _fitPatchApplier.Apply(fitTable, map, patch.Operations);

        _logger.LogInformation("Writing patched FIT to {outputFile}", outputFile);
        _fileManager.Write(_fitParser.Write(fitTable), outputFile, true);
        return 0;
    }

    private void ValidateMap(FitMapDocument map, bool ignoreVersions)
    {
        if (map.Type != FitMapDocument.SupportedType)
            throw new ArgumentException(
                $"Expected FIT map type {FitMapDocument.SupportedType}, but got {map.Type}. " +
                "--ignore-versions cannot ignore a different document type.", nameof(map));

        if (map.Version == FitMapDocument.SupportedVersion)
            return;
        if (ignoreVersions)
        {
            _logger.LogWarning(
                "FIT map version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                map.Version, FitMapDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected FIT map version {FitMapDocument.SupportedVersion}, but got {map.Version}. " +
            "Use --ignore-versions only when the map schema is known to be compatible.", nameof(map));
    }

    private void ValidatePatch(FitPatchDocument patch, bool ignoreVersions)
    {
        if (patch.Type != FitPatchDocument.SupportedType)
            throw new ArgumentException(
                $"Expected FIT patch type {FitPatchDocument.SupportedType}, but got {patch.Type}. " +
                "--ignore-versions cannot ignore a different document type.", nameof(patch));

        if (patch.Version == FitPatchDocument.SupportedVersion)
            return;
        if (ignoreVersions)
        {
            _logger.LogWarning(
                "FIT patch version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                patch.Version, FitPatchDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected FIT patch version {FitPatchDocument.SupportedVersion}, but got {patch.Version}. " +
            "Use --ignore-versions only when the patch schema is known to be compatible.", nameof(patch));
    }
}
