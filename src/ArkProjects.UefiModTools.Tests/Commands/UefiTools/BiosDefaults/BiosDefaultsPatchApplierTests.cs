using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.IfrMapping;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Patching;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.BiosDefaults;

public class BiosDefaultsPatchApplierTests
{
    [Fact]
    public void ApplyWritesNumericValueInLittleEndianOrder()
    {
        var biosDefaults = new byte[8];
        var mapping = CreateMapping(IfrOpCodes.Numeric, 2, 2, "numeric");

        CreateApplier().Apply(biosDefaults, CreateStoreMap(mapping), [new BiosDefaultsValuePatch
        {
            Id = "numeric",
            Value = "4660",
        }]);

        Assert.Equal(new byte[] { 0x34, 0x12 }, biosDefaults[2..4]);
    }

    [Fact]
    public void ApplyWritesCheckboxValue()
    {
        var biosDefaults = new byte[8];
        var mapping = CreateMapping(IfrOpCodes.CheckBox, 3, 1, "checkbox");

        CreateApplier().Apply(biosDefaults, CreateStoreMap(mapping), [new BiosDefaultsValuePatch
        {
            Id = "checkbox",
            Value = "true",
        }]);

        Assert.Equal((byte)1, biosDefaults[3]);
    }

    [Fact]
    public void ApplyWritesNullTerminatedUtf16String()
    {
        var biosDefaults = Enumerable.Repeat((byte)0xFF, 12).ToArray();
        var mapping = CreateMapping(IfrOpCodes.String, 2, 8, "string");

        CreateApplier().Apply(biosDefaults, CreateStoreMap(mapping), [new BiosDefaultsValuePatch
        {
            Id = "string",
            Value = "Hi",
        }]);

        Assert.Equal(new byte[] { 0x48, 0x00, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00 }, biosDefaults[2..10]);
    }

    private static BiosDefaultsPatchApplier CreateApplier() => new(NullLogger<BiosDefaultsPatchApplier>.Instance);

    private static BiosDefaultsIfrMapDocument CreateStoreMap(BiosDefaultsQuestionMapping mapping) => new()
    {
        Version = BiosDefaultsIfrMapDocument.SupportedVersion,
        Type = BiosDefaultsIfrMapDocument.SupportedType,
        BiosDefaultsSha256 = "unused",
        IfrSha256 = "unused",
        QuestionMappings = [mapping],
    };

    private static BiosDefaultsQuestionMapping CreateMapping(string opcode, int nvarDataOffset, int dataLength, string id) => new()
    {
        Id = id,
        QuestionId = 1,
        Opcode = opcode,
        VarStoreName = "Setup",
        VarStoreOffset = 0,
        NvarDataOffset = nvarDataOffset,
        DataLength = dataLength,
        Status = BiosDefaultsMappingStatus.Mapped,
    };
}
