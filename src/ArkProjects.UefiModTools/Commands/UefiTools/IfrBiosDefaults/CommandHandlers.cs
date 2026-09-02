using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaults;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaultsStore;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly NvarMapExtractor _extractor;
    private readonly BiosDefaultsStoreMapper _storeMapper;
    private readonly BiosDefaultsStorePatchApplier _patchApplier;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, NvarMapExtractor extractor, BiosDefaultsStoreMapper storeMapper,
        BiosDefaultsStorePatchApplier patchApplier)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _extractor = extractor;
        _storeMapper = storeMapper;
        _patchApplier = patchApplier;
    }

    public int Extract(string inputFile, string outputFile)
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
        _fileManager.Write(_jsonSerializer.Serialize(result), outputFile, true);
        return 0;
    }

    public int MapStore(string inputFile, string ifrFile, string outputFile, bool ignoreVersions)
    {
        var biosDefaultsMap = _jsonSerializer.Deserialize<BiosDefaultsMapDocument>(_fileManager.ReadString(inputFile));
        ValidateBiosDefaultsMap(biosDefaultsMap, ignoreVersions);

        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        ValidateIfrDocument(ifr, ignoreVersions);

        _logger.LogInformation(
            "Read {variableCount} NVAR variables from {inputFile} and {operationCount} IFR operations from {ifrFile}",
            biosDefaultsMap.Variables.Count, inputFile, ifr.Operations.Count, ifrFile);

        var vars = _storeMapper.Map(biosDefaultsMap.Variables, ifr.Operations);
        var result = new BiosDefaultsStoreMapDocument
        {
            Version = BiosDefaultsStoreMapDocument.SupportedVersion,
            Type = BiosDefaultsStoreMapDocument.SupportedType,
            BiosDefaultsSha256 = biosDefaultsMap.SourceSha256,
            IfrSha256 = ifr.InputSha256,
            QuestionMappings = vars,
        };

        _logger.LogInformation("Writing {mappingCount} BIOS defaults store mappings to {outputFile}",
            result.QuestionMappings.Count, outputFile);
        _fileManager.Write(_jsonSerializer.Serialize(result), outputFile, true);
        return 0;
    }

    public int ApplyPatch(string inputFile, string mapFile, string patchFile, string outputFile, bool ignoreVersions)
    {
        var biosDefaults = _fileManager.ReadBytes(inputFile);
        var storeMap = _jsonSerializer.Deserialize<BiosDefaultsStoreMapDocument>(_fileManager.ReadString(mapFile));
        var patch = _jsonSerializer.Deserialize<BiosDefaultsStorePatchDocument>(_fileManager.ReadString(patchFile));
        ValidateStoreMap(storeMap, ignoreVersions);
        ValidateStorePatch(patch, ignoreVersions);

        var inputSha256 = Convert.ToHexString(SHA256.HashData(biosDefaults)).ToLowerInvariant();
        if (!string.Equals(inputSha256, storeMap.BiosDefaultsSha256, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("BIOS defaults input does not match the store map source hash", nameof(inputFile));

        _logger.LogInformation("Applying {patchCount} NVAR question patches from {patchFile}", patch.VarPatches.Count, patchFile);
        _patchApplier.Apply(biosDefaults, storeMap, patch.VarPatches);
        _logger.LogInformation("Writing patched BIOS defaults to {outputFile}", outputFile);
        _fileManager.Write(biosDefaults, outputFile, true);
        return 0;
    }

    private void ValidateBiosDefaultsMap(BiosDefaultsMapDocument biosDefaultsMap, bool ignoreVersions)
    {
        if (biosDefaultsMap.Type != BiosDefaultsMapDocument.SupportedType)
        {
            throw new ArgumentException(
                $"Expected BIOS defaults map type {BiosDefaultsMapDocument.SupportedType}, but got {biosDefaultsMap.Type}. " +
                "--ignore-versions cannot ignore a different document type.",
                nameof(biosDefaultsMap));
        }

        if (biosDefaultsMap.Version == BiosDefaultsMapDocument.SupportedVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "BIOS defaults map version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                biosDefaultsMap.Version, BiosDefaultsMapDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected BIOS defaults map version {BiosDefaultsMapDocument.SupportedVersion}, but got {biosDefaultsMap.Version}. " +
            "Use --ignore-versions only when the map schema is known to be compatible.",
            nameof(biosDefaultsMap));
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

    private void ValidateStoreMap(BiosDefaultsStoreMapDocument storeMap, bool ignoreVersions)
    {
        if (storeMap.Type != BiosDefaultsStoreMapDocument.SupportedType)
        {
            throw new ArgumentException(
                $"Expected BIOS defaults store map type {BiosDefaultsStoreMapDocument.SupportedType}, but got {storeMap.Type}. " +
                "--ignore-versions cannot ignore a different document type.",
                nameof(storeMap));
        }

        if (storeMap.Version == BiosDefaultsStoreMapDocument.SupportedVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "BIOS defaults store map version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                storeMap.Version, BiosDefaultsStoreMapDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected BIOS defaults store map version {BiosDefaultsStoreMapDocument.SupportedVersion}, but got {storeMap.Version}. " +
            "Use --ignore-versions only when the map schema is known to be compatible.",
            nameof(storeMap));
    }

    private void ValidateStorePatch(BiosDefaultsStorePatchDocument patch, bool ignoreVersions)
    {
        if (patch.Type != BiosDefaultsStorePatchDocument.SupportedType)
        {
            throw new ArgumentException(
                $"Expected BIOS defaults store patch type {BiosDefaultsStorePatchDocument.SupportedType}, but got {patch.Type}. " +
                "--ignore-versions cannot ignore a different document type.",
                nameof(patch));
        }

        if (patch.Version == BiosDefaultsStorePatchDocument.SupportedVersion)
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "BIOS defaults store patch version {actualVersion} is not the supported version {supportedVersion}; continuing because --ignore-versions was specified",
                patch.Version, BiosDefaultsStorePatchDocument.SupportedVersion);
            return;
        }

        throw new ArgumentException(
            $"Expected BIOS defaults store patch version {BiosDefaultsStorePatchDocument.SupportedVersion}, but got {patch.Version}. " +
            "Use --ignore-versions only when the patch schema is known to be compatible.",
            nameof(patch));
    }
}
