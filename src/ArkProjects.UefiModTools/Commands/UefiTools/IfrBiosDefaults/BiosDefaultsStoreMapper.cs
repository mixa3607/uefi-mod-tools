using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public class BiosDefaultsStoreMapper
{
    private static readonly HashSet<string> StorageQuestionOpcodes =
    [
        "Numeric",
        "OneOf",
        "CheckBox",
        "OrderedList",
        "String",
        "Password",
        "Date",
        "Time",
    ];

    private readonly ILogger<BiosDefaultsStoreMapper> _logger;

    public BiosDefaultsStoreMapper(ILogger<BiosDefaultsStoreMapper> logger)
    {
        _logger = logger;
    }

    public BiosDefaultsStoreMapDocument Map(BiosDefaultsMapDocument biosDefaultsMap, IfrJsonDocument ifr)
    {
        ValidateBiosDefaultsMap(biosDefaultsMap);

        var varStores = ReadVarStores(ifr.Operations);
        var variablesByName = biosDefaultsMap.Variables
            .GroupBy(x => x.Name, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToList());

        var mappings = new List<BiosDefaultsQuestionMapping>();
        foreach (var operation in ifr.Operations)
        {
            if (!StorageQuestionOpcodes.Contains(operation.Opcode))
                continue;

            var fields = operation.Fields;
            if (fields.QuestionId is null)
            {
                _logger.LogWarning("IFR {opcode} at 0x{offset:X} has no QuestionId. Skipping", operation.Opcode, operation.Offset);
                continue;
            }

            if (fields.VarStoreId is null || fields.VarOffset is null)
            {
                _logger.LogDebug("IFR question {questionId} ({opcode}) has no VarStore reference. Skipping",
                    fields.QuestionId.Value, operation.Opcode);
                continue;
            }

            if (!varStores.TryGetValue(fields.VarStoreId.Value, out var varStore))
            {
                var missingStoreMapping = CreateMapping(operation.Opcode, fields, $"VarStore#{fields.VarStoreId.Value}");
                missingStoreMapping.Status = BiosDefaultsMappingStatus.UnknownVarStore;
                LogUnmapped(missingStoreMapping);
                mappings.Add(missingStoreMapping);
                continue;
            }

            var mapping = CreateMapping(operation.Opcode, fields, varStore.Name);
            if (varStore.Size is null)
            {
                mapping.Status = BiosDefaultsMappingStatus.MissingVarStoreSize;
            }
            else if (!variablesByName.TryGetValue(varStore.Name, out var candidates))
            {
                mapping.Status = BiosDefaultsMappingStatus.UnknownVarStore;
            }
            else
            {
                ApplyNvarMapping(mapping, candidates, varStore.Size.Value);
            }

            if (mapping.Status != BiosDefaultsMappingStatus.Mapped)
                LogUnmapped(mapping);
            mappings.Add(mapping);
        }

        var mappedCount = mappings.Count(x => x.Status == BiosDefaultsMappingStatus.Mapped);
        _logger.LogInformation("Created {mappingCount} question mappings; {mappedCount} mapped and {unmappedCount} unmapped",
            mappings.Count, mappedCount, mappings.Count - mappedCount);

        return new BiosDefaultsStoreMapDocument
        {
            BiosDefaultsSha256 = biosDefaultsMap.SourceSha256,
            IfrSha256 = ifr.InputSha256,
            QuestionMappings = mappings.OrderBy(x => x.QuestionId).ToList(),
        };
    }

    private static void ValidateBiosDefaultsMap(BiosDefaultsMapDocument biosDefaultsMap)
    {
        if (biosDefaultsMap.Version != BiosDefaultsMapDocument.SupportedVersion ||
            biosDefaultsMap.Type != BiosDefaultsMapDocument.SupportedType)
        {
            throw new ArgumentException(
                $"Expected {BiosDefaultsMapDocument.SupportedType} version {BiosDefaultsMapDocument.SupportedVersion}",
                nameof(biosDefaultsMap));
        }
    }

    private Dictionary<ushort, IfrVarStore> ReadVarStores(IReadOnlyList<IfrOperation> operations)
    {
        var varStores = new Dictionary<ushort, IfrVarStore>();
        foreach (var operation in operations.Where(x => x.Opcode is "VarStore" or "VarStoreEfi"))
        {
            var fields = operation.Fields;
            if (fields.VarStoreId is null)
            {
                _logger.LogWarning("IFR {opcode} at 0x{offset:X} has no VarStoreId. Skipping", operation.Opcode, operation.Offset);
                continue;
            }

            if (!TryReadVarStoreName(fields.Name, out var name))
            {
                _logger.LogWarning("IFR {opcode} at 0x{offset:X} has no string VarStore name. Skipping", operation.Opcode, operation.Offset);
                continue;
            }

            if (!varStores.TryAdd(fields.VarStoreId.Value, new IfrVarStore(name, fields.Size)))
                throw new InvalidDataException($"IFR contains duplicate VarStoreId {fields.VarStoreId.Value}");
        }

        _logger.LogInformation("Read {varStoreCount} IFR VarStores", varStores.Count);
        return varStores;
    }

    private static BiosDefaultsQuestionMapping CreateMapping(string opcode, IfrOperationFields fields, string varStoreName)
    {
        return new BiosDefaultsQuestionMapping
        {
            QuestionId = fields.QuestionId!.Value,
            Opcode = opcode,
            VarStoreName = varStoreName,
            VarStoreOffset = fields.VarOffset!.Value,
            DataLength = GetQuestionDataLength(fields),
        };
    }

    private static void ApplyNvarMapping(BiosDefaultsQuestionMapping mapping,
        IReadOnlyList<NvarVariableInfo> candidates, ushort varStoreSize)
    {
        var sizeMatches = candidates.Where(x => GetNvarDataLength(x) == varStoreSize).ToList();
        if (sizeMatches.Count == 0)
        {
            mapping.Status = BiosDefaultsMappingStatus.NvarSizeMismatch;
            return;
        }

        if (sizeMatches.Count > 1)
        {
            mapping.Status = BiosDefaultsMappingStatus.AmbiguousNvarVariable;
            return;
        }

        if (mapping.DataLength is null)
        {
            mapping.Status = BiosDefaultsMappingStatus.UnsupportedDataLength;
            return;
        }

        var nvarVariable = sizeMatches[0];
        if (mapping.VarStoreOffset + mapping.DataLength.Value > GetNvarDataLength(nvarVariable))
        {
            mapping.Status = BiosDefaultsMappingStatus.NvarRangeExceeded;
            return;
        }

        mapping.Status = BiosDefaultsMappingStatus.Mapped;
        mapping.NvarDataOffset = nvarVariable.DataOffset + mapping.VarStoreOffset;
    }

    private void LogUnmapped(BiosDefaultsQuestionMapping mapping)
    {
        _logger.LogWarning(
            "Could not map IFR question {questionId} ({opcode}) in VarStore {varStore} at offset 0x{offset:X}: {status}",
            mapping.QuestionId, mapping.Opcode, mapping.VarStoreName, mapping.VarStoreOffset, mapping.Status);
    }

    private static bool TryReadVarStoreName(JsonElement? value, out string name)
    {
        name = string.Empty;
        if (value is not { ValueKind: JsonValueKind.String })
            return false;

        name = value.Value.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(name);
    }

    private static int? GetQuestionDataLength(IfrOperationFields fields)
    {
        if (fields.Kind == "string" && fields.MaxSize is not null)
            return fields.MaxSize.Value * sizeof(char);

        if (fields.MinMaxStep is { } minMaxStep && minMaxStep.SizeBits > 0 && minMaxStep.SizeBits % 8 == 0)
            return minMaxStep.SizeBits / 8;

        if (fields.Kind == "checkbox")
            return 1;

        return null;
    }

    private static int GetNvarDataLength(NvarVariableInfo variable)
    {
        return variable.RecordOffset + variable.RecordSize - variable.DataOffset;
    }

    private sealed record IfrVarStore(string Name, ushort? Size);
}
