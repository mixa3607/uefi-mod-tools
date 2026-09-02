using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;
using ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct;

public class SctCommandHandlers
{
    private readonly ILogger<SctCommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly SctPatchApplier _patchApplier;

    public SctCommandHandlers(ILogger<SctCommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, SctPatchApplier patchApplier)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _patchApplier = patchApplier;
    }

    public int Patch(string inputFile, string ifrFile, string patchFile, string outputFile, bool ignoreVersions)
    {
        var sct = _fileManager.ReadBytes(inputFile);
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        var patch = _jsonSerializer.Deserialize<SctPatchDocument>(_fileManager.ReadString(patchFile));
        ValidateIfrDocument(ifr, ignoreVersions);
        ValidatePatch(patch, ignoreVersions);
        _logger.LogInformation(
            "Read {sctSize} bytes of {inputFile}, {operationCount} IFR operations, and patch version {patchVersion}",
            sct.Length, inputFile, ifr.Operations.Count, patch.Version);

        _patchApplier.Apply(sct, ifr.Operations, patch);

        _logger.LogInformation("Writing Platform_setup.sct to {outputFile}", outputFile);
        _fileManager.Write(sct.ToArray(), outputFile, true);
        return 0;
    }

    private void ValidatePatch(SctPatchDocument patch, bool ignoreVersions)
    {
        if (patch.Type != SctPatchDocument.SupportedType)
            throw new ArgumentException(
                $"Expected SCT patch type {SctPatchDocument.SupportedType}, but got {patch.Type}. " +
                "--ignore-versions cannot ignore a different document type.", nameof(patch));

        if (patch.Version == SctPatchDocument.SupportedVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "SCT patch version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                patch.Version, SctPatchDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected SCT patch version {SctPatchDocument.SupportedVersion}, but got {patch.Version}. " +
            "Use --ignore-versions only when the patch schema is known to be compatible.", nameof(patch));
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
