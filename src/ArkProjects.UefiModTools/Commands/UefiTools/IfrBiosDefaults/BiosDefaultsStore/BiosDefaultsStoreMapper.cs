using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaults;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaultsStore;

public class BiosDefaultsStoreMapper
{
    private static readonly HashSet<string> StorageQuestionOpcodes =
    [
        IfrOpCodes.Numeric,
        IfrOpCodes.OneOf,
        IfrOpCodes.CheckBox,
        IfrOpCodes.OrderedList,
        IfrOpCodes.String,
        IfrOpCodes.Password,
        IfrOpCodes.Date,
        IfrOpCodes.Time,
    ];

    private readonly ILogger<BiosDefaultsStoreMapper> _logger;

    public BiosDefaultsStoreMapper(ILogger<BiosDefaultsStoreMapper> logger)
    {
        _logger = logger;
    }

    public List<BiosDefaultsQuestionMapping> Map(IReadOnlyList<NvarVariableInfo> nvarVariableInfos, IReadOnlyList<IfrOperation> ifrOps)
    {
        var varStores = ReadVarStores(ifrOps);
        var variablesByName = nvarVariableInfos
            .GroupBy(x => x.Name, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.ToList());

        var mappings = new List<BiosDefaultsQuestionMapping>();
        foreach (var operation in ifrOps)
        {
            if (!StorageQuestionOpcodes.Contains(operation.Opcode))
                continue;

            var fields = operation.Fields;
            if (fields.QuestionId is null)
            {
                _logger.LogWarning("IFR {opcode} at 0x{offset:X} has no QuestionId. Skipping", operation.Opcode,
                    operation.Offset);
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

        return mappings.OrderBy(x => x.QuestionId).ToList();
    }

    private Dictionary<ushort, IfrVarStore> ReadVarStores(IReadOnlyList<IfrOperation> operations)
    {
        var varStores = new Dictionary<ushort, IfrVarStore>();
        foreach (var operation in operations.Where(x => x.Opcode is IfrOpCodes.VarStore or IfrOpCodes.VarStoreEfi))
        {
            var fields = operation.Fields;
            if (fields.VarStoreId is null)
            {
                _logger.LogWarning("IFR {opcode} at 0x{offset:X} has no VarStoreId. Skipping",
                    operation.Opcode, operation.Offset);
                continue;
            }

            if (!TryReadVarStoreName(fields.Name, out var name))
            {
                _logger.LogWarning("IFR {opcode} at 0x{offset:X} has no string VarStore name. Skipping",
                    operation.Opcode, operation.Offset);
                continue;
            }

            if (!varStores.TryAdd(fields.VarStoreId.Value, new IfrVarStore(name, fields.Size)))
                throw new InvalidDataException($"IFR contains duplicate VarStoreId {fields.VarStoreId.Value}");
        }

        _logger.LogInformation("Read {varStoreCount} IFR VarStores", varStores.Count);
        return varStores;
    }

    private static BiosDefaultsQuestionMapping CreateMapping(
        string opcode, IfrOperationFields fields, string varStoreName)
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
        var sizeMatches = candidates.Where(x => x.DataLength == varStoreSize).ToList();
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
        if (nvarVariable.Value.Length != nvarVariable.DataLength)
        {
            throw new InvalidDataException(
                $"NVAR variable {nvarVariable.Name} at 0x{nvarVariable.RecordOffset:X} has {nvarVariable.Value.Length} value bytes, " +
                $"but its record declares {nvarVariable.DataLength}. Regenerate the NVAR map with version {BiosDefaultsMapDocument.SupportedVersion}.");
        }

        if (mapping.VarStoreOffset + mapping.DataLength.Value > nvarVariable.DataLength)
        {
            mapping.Status = BiosDefaultsMappingStatus.NvarRangeExceeded;
            return;
        }

        mapping.Status = BiosDefaultsMappingStatus.Mapped;
        mapping.NvarDataOffset = nvarVariable.DataOffset + mapping.VarStoreOffset;
        mapping.Id = CreateMappingId(mapping);
        mapping.Value = FormatQuestionValue(mapping.Opcode,
            nvarVariable.Value.AsSpan(mapping.VarStoreOffset, mapping.DataLength.Value));
    }

    private void LogUnmapped(BiosDefaultsQuestionMapping mapping)
    {
        _logger.LogWarning("Could not map IFR question {questionId} ({opcode}) in VarStore {varStore} at offset 0x{offset:X}: {status}",
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
        if (fields is { Kind: "string", MaxSize: not null })
            return fields.MaxSize.Value * sizeof(char);

        if (fields.MinMaxStep is { SizeBits: > 0 } minMaxStep && minMaxStep.SizeBits % 8 == 0)
            return minMaxStep.SizeBits / 8;

        if (fields.Kind == "checkbox")
            return 1;

        return null;
    }

    private static string CreateMappingId(BiosDefaultsQuestionMapping mapping)
    {
        return $"{mapping.QuestionId:X4}-{mapping.VarStoreName}-{mapping.VarStoreOffset:X4}-{mapping.NvarDataOffset!.Value:X8}";
    }

    private static string FormatQuestionValue(string opcode, ReadOnlySpan<byte> value)
    {
        if (opcode == IfrOpCodes.CheckBox && value.Length == 1)
            return value[0] == 0 ? "false" : "true";

        if (opcode is IfrOpCodes.Numeric or IfrOpCodes.OneOf)
        {
            return value.Length switch
            {
                1 => value[0].ToString(),
                2 => BinaryPrimitives.ReadUInt16LittleEndian(value).ToString(),
                4 => BinaryPrimitives.ReadUInt32LittleEndian(value).ToString(),
                8 => BinaryPrimitives.ReadUInt64LittleEndian(value).ToString(),
                _ => Convert.ToHexString(value),
            };
        }

        if ((opcode is IfrOpCodes.String or IfrOpCodes.Password) && value.Length % sizeof(char) == 0)
            return Encoding.Unicode.GetString(value).TrimEnd('\0');

        return Convert.ToHexString(value);
    }

    private sealed record IfrVarStore(string Name, ushort? Size);
}
