using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.IfrMapping;

namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Patching;

public class BiosDefaultsPatchApplier
{
    private readonly ILogger<BiosDefaultsPatchApplier> _logger;

    public BiosDefaultsPatchApplier(ILogger<BiosDefaultsPatchApplier> logger)
    {
        _logger = logger;
    }

    public void Apply(byte[] biosDefaults, BiosDefaultsIfrMapDocument storeMap,
        IReadOnlyList<BiosDefaultsValuePatch> patches)
    {
        var mappingsById = storeMap.QuestionMappings
            .Where(x => x.Status == BiosDefaultsMappingStatus.Mapped && x.Id is not null)
            .ToDictionary(x => x.Id!, StringComparer.Ordinal);

        foreach (var patch in patches)
        {
            if (!mappingsById.TryGetValue(patch.Id, out var mapping))
                throw new ArgumentException($"No mapped NVAR question has id '{patch.Id}'", nameof(patches));

            if (mapping.NvarDataOffset is null || mapping.DataLength is null)
                throw new InvalidDataException($"Mapped NVAR question '{patch.Id}' has no data range");

            var value = EncodeValue(mapping, patch.Value);
            if (mapping.NvarDataOffset.Value < 0 ||
                mapping.NvarDataOffset.Value + value.Length > biosDefaults.Length)
            {
                throw new InvalidDataException($"Mapped NVAR question '{patch.Id}' is outside the BIOS defaults input");
            }

            value.AsSpan().CopyTo(biosDefaults.AsSpan(mapping.NvarDataOffset.Value, value.Length));
            _logger.LogInformation("Patched IFR question {questionId} at NVAR offset 0x{offset:X}",
                mapping.QuestionId, mapping.NvarDataOffset.Value);
        }
    }

    private static byte[] EncodeValue(BiosDefaultsQuestionMapping mapping, string value)
    {
        var dataLength = mapping.DataLength!.Value;
        if (mapping.Opcode == IfrOpCodes.CheckBox)
        {
            if (dataLength != 1)
                throw new InvalidDataException($"CheckBox question '{mapping.Id}' does not have a one-byte value");

            return value.ToLowerInvariant() switch
            {
                "true" or "1" => [1],
                "false" or "0" => [0],
                _ => throw new ArgumentException($"CheckBox question '{mapping.Id}' expects true, false, 1, or 0"),
            };
        }

        if (mapping.Opcode is IfrOpCodes.Numeric or IfrOpCodes.OneOf)
            return EncodeNumericValue(mapping, value);

        if (mapping.Opcode is IfrOpCodes.String or IfrOpCodes.Password)
            return EncodeStringValue(mapping, value);

        return DecodeHexValue(mapping, value);
    }

    private static byte[] EncodeNumericValue(BiosDefaultsQuestionMapping mapping, string value)
    {
        var dataLength = mapping.DataLength!.Value;
        if (dataLength is not (1 or 2 or 4 or 8))
            return DecodeHexValue(mapping, value);

        if (!TryParseUnsigned(value, out var numericValue))
            throw new ArgumentException($"Numeric question '{mapping.Id}' expects a decimal or 0x-prefixed unsigned value");

        if (dataLength < sizeof(ulong) && numericValue >= 1UL << (dataLength * 8))
            throw new ArgumentException($"Value for question '{mapping.Id}' does not fit in {dataLength} bytes");

        var bytes = new byte[dataLength];
        for (var index = 0; index < bytes.Length; index++)
            bytes[index] = (byte)(numericValue >> (index * 8));
        return bytes;
    }

    private static byte[] EncodeStringValue(BiosDefaultsQuestionMapping mapping, string value)
    {
        var stringBytes = Encoding.Unicode.GetBytes(value);
        var dataLength = mapping.DataLength!.Value;
        if (stringBytes.Length + sizeof(char) > dataLength)
            throw new ArgumentException($"String value for question '{mapping.Id}' is too long");

        var bytes = new byte[dataLength];
        stringBytes.CopyTo(bytes, 0);
        return bytes;
    }

    private static byte[] DecodeHexValue(BiosDefaultsQuestionMapping mapping, string value)
    {
        var hex = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
        if (hex.Length % 2 != 0)
            throw new ArgumentException($"Hex value for question '{mapping.Id}' has an odd number of digits");

        byte[] bytes;
        try
        {
            bytes = Convert.FromHexString(hex);
        }
        catch (FormatException exception)
        {
            throw new ArgumentException($"Question '{mapping.Id}' expects hexadecimal bytes", exception);
        }

        if (bytes.Length != mapping.DataLength)
            throw new ArgumentException($"Hex value for question '{mapping.Id}' must be exactly {mapping.DataLength} bytes");

        return bytes;
    }

    private static bool TryParseUnsigned(string value, out ulong numericValue)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return ulong.TryParse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out numericValue);

        return ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out numericValue);
    }
}
