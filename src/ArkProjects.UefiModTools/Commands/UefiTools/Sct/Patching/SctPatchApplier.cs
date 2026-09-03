using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

// IDK how it works, but it works
public sealed class SctPatchApplier
{
    private static readonly byte[] SuppressIfOpcode = [0x0A, 0x82];
    private static readonly byte[] EndOpcode = [0x29, 0x02];
    private const byte DefaultOpcode = 0x5B;
    private const int DefaultValueOffset = 5;

    private readonly ILogger<SctPatchApplier> _logger;

    public SctPatchApplier(ILogger<SctPatchApplier> logger)
    {
        _logger = logger;
    }

    public void Apply(byte[] sct, IReadOnlyList<IfrOperation> operations, SctPatchDocument patches)
    {
        var defaultOffsets = patches.DefaultValuePatches.Where(x => x.Apply).Select(x => x.Offset).ToList();
        if (defaultOffsets.Count != defaultOffsets.Distinct().Count())
            throw new ArgumentException("Default value patch offsets must be unique", nameof(patches));

        foreach (var patch in patches.DefaultValuePatches.Where(x => x.Apply))
            ApplyDefaultValue(sct, operations, patch);

        var offsets = patches.SuppressIfPatches
            .Where(x => x.Apply)
            .Select(x => x.Offset)
            .ToList();

        if (offsets.Count != offsets.Distinct().Count())
            throw new ArgumentException("SuppressIf patch offsets must be unique", nameof(patches));

        // Operations retain their original IFR indices; this table tracks their byte offsets
        // after each End opcode is moved.
        var currentOffsets = operations
            .Select((operation, index) => (index, Offset: checked((int)operation.Offset)))
            .ToDictionary(x => x.index, x => x.Offset);

        foreach (var suppressOffset in offsets)
        {
            ApplyDisableSuppressIf(sct, operations, currentOffsets, suppressOffset);
        }
    }

    private void ApplyDefaultValue(byte[] sct, IReadOnlyList<IfrOperation> operations, DefaultValuePatch patch)
    {
        var defaultIndex = FindOperation(operations, IfrOpCodes.Default, patch.Offset);
        var operation = operations[defaultIndex];
        var valueOffset = checked(patch.Offset + DefaultValueOffset);
        AssertDefaultHeader(sct, patch.Offset, operation);

        var valueType = sct[valueOffset - 1];
        var valueBytes = EncodeValue(patch.Value, valueType);
        if (valueOffset + valueBytes.Length > patch.Offset + operation.Length || valueOffset + valueBytes.Length > sct.Length)
            throw new InvalidDataException($"Default at 0x{patch.Offset:X} does not have space for a {patch.Value.Type} value");

        valueBytes.CopyTo(sct, valueOffset);
        _logger.LogInformation("Patched Default value at original offset 0x{offset:X}", patch.Offset);
    }

    private static void AssertDefaultHeader(byte[] sct, int offset, IfrOperation operation)
    {
        if (operation.Fields.DefaultId is not { } defaultId || offset < 0 || offset + DefaultValueOffset > sct.Length ||
            sct[offset] != DefaultOpcode || (sct[offset + 1] & 0x7F) != operation.Length ||
            BinaryPrimitives.ReadUInt16LittleEndian(sct.AsSpan(offset + 2, sizeof(ushort))) != defaultId)
        {
            throw new InvalidDataException($"Expected Default opcode at 0x{offset:X}");
        }
    }

    private static byte[] EncodeValue(IfrTypeValue value, byte actualType) => value.Type switch
    {
        "u8" when actualType == 0x00 => [ReadNumber<byte>(value)],
        "u16" when actualType == 0x01 => WriteUInt16(ReadNumber<ushort>(value)),
        "u32" when actualType == 0x02 => WriteUInt32(ReadNumber<uint>(value)),
        "u64" when actualType == 0x03 => WriteUInt64(ReadNumber<ulong>(value)),
        "bool" when actualType == 0x04 => [ReadBoolean(value) ? (byte)1 : (byte)0],
        _ => throw new ArgumentException($"Default value type '{value.Type}' does not match the IFR value type 0x{actualType:X2}"),
    };

    private static T ReadNumber<T>(IfrTypeValue value) where T : struct, IConvertible
    {
        if (value.Value is not { ValueKind: System.Text.Json.JsonValueKind.Number } number)
            throw new ArgumentException($"Default value type '{value.Type}' requires a number");

        return (T)Convert.ChangeType(number.GetDecimal(), typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static bool ReadBoolean(IfrTypeValue value)
    {
        if (value.Value is not { ValueKind: System.Text.Json.JsonValueKind.True or System.Text.Json.JsonValueKind.False } boolean)
            throw new ArgumentException("Default value type 'bool' requires a boolean");

        return boolean.GetBoolean();
    }

    private static byte[] WriteUInt16(ushort value) { var bytes = new byte[2]; BinaryPrimitives.WriteUInt16LittleEndian(bytes, value); return bytes; }
    private static byte[] WriteUInt32(uint value) { var bytes = new byte[4]; BinaryPrimitives.WriteUInt32LittleEndian(bytes, value); return bytes; }
    private static byte[] WriteUInt64(ulong value) { var bytes = new byte[8]; BinaryPrimitives.WriteUInt64LittleEndian(bytes, value); return bytes; }

    private void ApplyDisableSuppressIf(
        byte[] sct,
        IReadOnlyList<IfrOperation> operations,
        Dictionary<int, int> currentOffsets,
        int originalSuppressOffset)
    {
        var suppressIndex = FindOperation(operations, IfrOpCodes.SuppressIf, originalSuppressOffset);
        var suppress = operations[suppressIndex];

        if (!suppress.ScopeStart)
            throw new InvalidDataException($"SuppressIf at 0x{originalSuppressOffset:X} does not open a scope");

        var conditionIndex = suppressIndex + 1;
        if (conditionIndex >= operations.Count)
            throw new InvalidDataException($"SuppressIf at 0x{originalSuppressOffset:X} has no condition");

        var condition = operations[conditionIndex];
        var conditionEndIndex = condition.ScopeStart
            ? FindMatchingEnd(operations, conditionIndex)
            : conditionIndex;

        var suppressEndIndex = FindMatchingEnd(operations, suppressIndex);
        var suppressEnd = operations[suppressEndIndex];

        var sourceOffset = currentOffsets[suppressEndIndex];
        var destinationOffset = checked(currentOffsets[conditionEndIndex] + operations[conditionEndIndex].Length);

        if (sourceOffset <= destinationOffset)
            throw new InvalidDataException($"SuppressIf at 0x{originalSuppressOffset:X} has an invalid scope layout");

        AssertOpcode(sct, currentOffsets[suppressIndex], SuppressIfOpcode, IfrOpCodes.SuppressIf);
        AssertOpcode(sct, sourceOffset, EndOpcode, IfrOpCodes.End);

        MoveRange(sct, sourceOffset, suppressEnd.Length, destinationOffset);
        UpdateOffsetsAfterMove(currentOffsets, suppressEndIndex, sourceOffset, suppressEnd.Length, destinationOffset);

        _logger.LogInformation(
            "Disabled SuppressIf at original offset 0x{offset:X}; moved End from 0x{source:X} to 0x{destination:X}",
            originalSuppressOffset, sourceOffset, destinationOffset);
    }

    private static int FindOperation(IReadOnlyList<IfrOperation> operations, string opcode, int offset)
    {
        if (offset < 0)
            throw new InvalidDataException($"{opcode} offset cannot be negative");

        for (var index = 0; index < operations.Count; index++)
        {
            if (operations[index].Opcode == opcode && operations[index].Offset == (ulong)offset)
                return index;
        }

        throw new InvalidDataException($"{opcode} at original offset 0x{offset:X} was not found");
    }

    private static int FindMatchingEnd(IReadOnlyList<IfrOperation> operations, int scopeStartIndex)
    {
        var openScopes = 1;

        for (var index = scopeStartIndex + 1; index < operations.Count; index++)
        {
            if (operations[index].ScopeStart)
                openScopes++;

            if (operations[index].Opcode != IfrOpCodes.End)
                continue;

            openScopes--;
            if (openScopes == 0)
                return index;
        }

        throw new InvalidDataException($"Scope at operation index {scopeStartIndex} has no matching End");
    }

    private static void AssertOpcode(byte[] sct, int offset, ReadOnlySpan<byte> expected, string name)
    {
        if (offset < 0 || offset + expected.Length > sct.Length ||
            !sct.AsSpan(offset, expected.Length).SequenceEqual(expected))
        {
            throw new InvalidDataException($"Expected {name} opcode at 0x{offset:X}");
        }
    }

    private static void UpdateOffsetsAfterMove(
        Dictionary<int, int> currentOffsets,
        int movedOperationIndex,
        int sourceOffset,
        int length,
        int destinationOffset)
    {
        foreach (var index in currentOffsets.Keys.ToArray())
        {
            if (index == movedOperationIndex)
            {
                currentOffsets[index] = destinationOffset;
            }
            else if (currentOffsets[index] >= destinationOffset && currentOffsets[index] < sourceOffset)
            {
                currentOffsets[index] += length;
            }
        }
    }

    private static void MoveRange<T>(T[] array, int sourceOffset, int length, int destinationOffset)
    {
        var affectedArea = array.AsSpan(destinationOffset, sourceOffset + length - destinationOffset);

        affectedArea[..^length].Reverse();
        affectedArea[^length..].Reverse();
        affectedArea.Reverse();
    }
}
