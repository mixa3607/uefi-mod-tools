using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;
using ArkProjects.UefiModTools.Services.ManifestVer;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public class FitCommandHandlers
{
    private readonly ILogger<FitCommandHandlers> _logger;
    private readonly ISerializationService _serializer;
    private readonly ICommandFileManager _fileManager;
    private readonly IManifestVersionVerifier _manifestVersionVerifier;
    private readonly FitParser _fitParser;
    private readonly FitMapper _fitMapper;
    private readonly FitPatchApplier _fitPatchApplier;

    public FitCommandHandlers(ILogger<FitCommandHandlers> logger, ISerializationService serializer,
        ICommandFileManager fileManager, FitParser fitParser, FitMapper fitMapper, FitPatchApplier fitPatchApplier,
        IManifestVersionVerifier manifestVersionVerifier)
    {
        _logger = logger;
        _serializer = serializer;
        _fileManager = fileManager;
        _fitParser = fitParser;
        _fitMapper = fitMapper;
        _fitPatchApplier = fitPatchApplier;
        _manifestVersionVerifier = manifestVersionVerifier;
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
        _fileManager.Write(_serializer.Serialize(map, outputFormat), outputFile, true);
        return 0;
    }

    public int ApplyPatch(string inputFile, string mapFile, string patchFile, string outputFile, bool ignoreVersions,
        bool ignoreChecksums)
    {
        var fitBytes = _fileManager.ReadBytes(inputFile);
        var map = _serializer.Deserialize<FitMapDocument>(_fileManager.ReadString(mapFile), SerializationFormat.Auto);
        _manifestVersionVerifier.Verify(map, "FIT map", FitMapDocument.SupportedType, ignoreVersions,
            FitMapDocument.SupportedVersion);
        var patch = _serializer.Deserialize<FitPatchDocument>(_fileManager.ReadString(patchFile), SerializationFormat.Auto);
        _manifestVersionVerifier.Verify(patch, "FIT patch", FitPatchDocument.SupportedType, ignoreVersions,
            FitPatchDocument.SupportedVersion);

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
}
