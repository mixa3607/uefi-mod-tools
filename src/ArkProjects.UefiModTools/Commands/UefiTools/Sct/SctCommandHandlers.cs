using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;
using ArkProjects.UefiModTools.Services.ManifestVer;
using Microsoft.Extensions.Logging;
using ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct;

public class SctCommandHandlers
{
    private readonly ILogger<SctCommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly ISerializationService _serializer;
    private readonly IManifestVersionVerifier _manifestVersionVerifier;
    private readonly SctPatchApplier _patchApplier;

    public SctCommandHandlers(ILogger<SctCommandHandlers> logger, ICommandFileManager fileManager,
        ISerializationService serializer, SctPatchApplier patchApplier, IManifestVersionVerifier manifestVersionVerifier)
    {
        _logger = logger;
        _fileManager = fileManager;
        _serializer = serializer;
        _manifestVersionVerifier = manifestVersionVerifier;
        _patchApplier = patchApplier;
    }

    public int Patch(string inputFile, string ifrFile, string patchFile, string outputFile, bool ignoreVersions)
    {
        var sct = _fileManager.ReadBytes(inputFile);
        var ifr = _serializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile), SerializationFormat.Auto);
        var patch = _serializer.Deserialize<SctPatchDocument>(_fileManager.ReadString(patchFile), SerializationFormat.Auto);
        ValidateIfrDocument(ifr, ignoreVersions);
        _manifestVersionVerifier.Verify(patch, "SCT patch", SctPatchDocument.SupportedType, ignoreVersions,
            SctPatchDocument.SupportedVersion);
        _logger.LogInformation(
            "Read {sctSize} bytes of {inputFile}, {operationCount} IFR operations, and patch version {patchVersion}",
            sct.Length, inputFile, ifr.Operations.Count, patch.Version);

        _patchApplier.Apply(sct, ifr.Operations, patch);

        _logger.LogInformation("Writing Platform_setup.sct to {outputFile}", outputFile);
        _fileManager.Write(sct.ToArray(), outputFile, true);
        return 0;
    }

    private void ValidateIfrDocument(IfrJsonDocument ifr, bool ignoreVersions)
    {
        if (ifr.ExtractionMode != "UEFI")
            throw new ArgumentException(
                $"Expected IFR extraction mode UEFI, but got {ifr.ExtractionMode}. " +
                "--ignore-versions cannot ignore a different extraction mode.", nameof(ifr));

        const string supportedProgramVersion = "1.6.1";
        if (ifr.ProgramVersion == supportedProgramVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "IFR extractor version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                ifr.ProgramVersion, supportedProgramVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected IFR extractor version {supportedProgramVersion}, but got {ifr.ProgramVersion}. " +
            "Use --ignore-versions only when the IFR JSON schema is known to be compatible.", nameof(ifr));
    }
}
