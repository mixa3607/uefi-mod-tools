using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;
using ArkProjects.UefiModTools.Services.ManifestVer;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.IfrMapping;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Nvar;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults;

public class BiosDefaultsCommandHandlers
{
    private readonly ILogger<BiosDefaultsCommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly ISerializationService _serializer;
    private readonly IManifestVersionVerifier _manifestVersionVerifier;
    private readonly NvarMapExtractor _extractor;
    private readonly BiosDefaultsIfrMapper _ifrMapper;
    private readonly BiosDefaultsPatchApplier _patchApplier;

    public BiosDefaultsCommandHandlers(ILogger<BiosDefaultsCommandHandlers> logger, ICommandFileManager fileManager,
        ISerializationService serializer, NvarMapExtractor extractor, BiosDefaultsIfrMapper ifrMapper,
        BiosDefaultsPatchApplier patchApplier, IManifestVersionVerifier manifestVersionVerifier)
    {
        _logger = logger;
        _fileManager = fileManager;
        _serializer = serializer;
        _manifestVersionVerifier = manifestVersionVerifier;
        _extractor = extractor;
        _ifrMapper = ifrMapper;
        _patchApplier = patchApplier;
    }

    public int Extract(string inputFile, string outputFile, SerializationFormat outputFormat)
    {
        var defaultsFileBytes = _fileManager.ReadBytes(inputFile);
        _logger.LogInformation("Read {size} bytes of BIOS defaults from {inputFile}", defaultsFileBytes.Length, inputFile);

        var result = new BiosDefaultsMapDocument
        {
            Version = BiosDefaultsMapDocument.SupportedVersion,
            Type = BiosDefaultsMapDocument.SupportedType,

            Variables = _extractor.Extract(defaultsFileBytes),
            SourceName = Path.GetFileName(inputFile),
            SourceSha256 = Convert.ToHexString(SHA256.HashData(defaultsFileBytes)).ToLowerInvariant(),
        };

        _logger.LogInformation("Writing {count} BIOS defaults variables to {outputFile}", result.Variables.Count, outputFile);
        _fileManager.Write(_serializer.Serialize(result, outputFormat), outputFile, true);
        return 0;
    }

    public int MapStore(string inputFile, string ifrFile, string outputFile, SerializationFormat outputFormat, bool ignoreVersions)
    {
        var biosDefaultsMap = _serializer.Deserialize<BiosDefaultsMapDocument>(_fileManager.ReadString(inputFile), SerializationFormat.Auto);
        _manifestVersionVerifier.Verify(biosDefaultsMap, "BIOS defaults map", BiosDefaultsMapDocument.SupportedType,
            ignoreVersions, BiosDefaultsMapDocument.SupportedVersion);

        var ifr = _serializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile), SerializationFormat.Auto);
        ValidateIfrDocument(ifr, ignoreVersions);

        _logger.LogInformation(
            "Read {variableCount} NVAR variables from {inputFile} and {operationCount} IFR operations from {ifrFile}",
            biosDefaultsMap.Variables.Count, inputFile, ifr.Operations.Count, ifrFile);

        var vars = _ifrMapper.Map(biosDefaultsMap.Variables, ifr.Operations);
        var result = new BiosDefaultsIfrMapDocument
        {
            Version = BiosDefaultsIfrMapDocument.SupportedVersion,
            Type = BiosDefaultsIfrMapDocument.SupportedType,
            BiosDefaultsSha256 = biosDefaultsMap.SourceSha256,
            IfrSha256 = ifr.InputSha256,
            QuestionMappings = vars,
        };

        _logger.LogInformation("Writing {mappingCount} BIOS defaults store mappings to {outputFile}",
            result.QuestionMappings.Count, outputFile);
        _fileManager.Write(_serializer.Serialize(result, outputFormat), outputFile, true);
        return 0;
    }

    public int ApplyPatch(string inputFile, string mapFile, string patchFile, string outputFile, bool ignoreVersions,
        bool ignoreChecksums)
    {
        var biosDefaults = _fileManager.ReadBytes(inputFile);
        var storeMap = _serializer.Deserialize<BiosDefaultsIfrMapDocument>(_fileManager.ReadString(mapFile), SerializationFormat.Auto);
        var patch = _serializer.Deserialize<BiosDefaultsPatchDocument>(_fileManager.ReadString(patchFile), SerializationFormat.Auto);
        _manifestVersionVerifier.Verify(storeMap, "BIOS defaults store map", BiosDefaultsIfrMapDocument.SupportedType,
            ignoreVersions, BiosDefaultsIfrMapDocument.SupportedVersion);
        _manifestVersionVerifier.Verify(patch, "BIOS defaults store patch", BiosDefaultsPatchDocument.SupportedType,
            ignoreVersions, BiosDefaultsPatchDocument.SupportedVersion);

        var inputSha256 = Convert.ToHexString(SHA256.HashData(biosDefaults)).ToLowerInvariant();
        if (!string.Equals(inputSha256, storeMap.BiosDefaultsSha256, StringComparison.OrdinalIgnoreCase) && !ignoreChecksums)
            throw new ArgumentException("BIOS defaults input does not match the store map source hash", nameof(inputFile));
        if (!string.Equals(inputSha256, storeMap.BiosDefaultsSha256, StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning("BIOS defaults input does not match the store map source hash; continuing because --ignore-checksums was specified");

        _logger.LogInformation("Applying {patchCount} NVAR question patches from {patchFile}", patch.VarPatches.Count, patchFile);
        _patchApplier.Apply(biosDefaults, storeMap, patch.VarPatches);
        _logger.LogInformation("Writing patched BIOS defaults to {outputFile}", outputFile);
        _fileManager.Write(biosDefaults, outputFile, true);
        return 0;
    }

    private void ValidateIfrDocument(IfrJsonDocument ifr, bool ignoreVersions)
    {
        if (ifr.ExtractionMode != "UEFI")
        {
            throw new ArgumentException(
                $"Expected IFR extraction mode UEFI, but got {ifr.ExtractionMode}. " +
                "--ignore-versions cannot ignore a different extraction mode.",
                nameof(ifr));
        }

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
            "Use --ignore-versions only when the IFR JSON schema is known to be compatible.",
            nameof(ifr));
    }

}
