using ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.Sct;

public class SctPatchApplierTests
{
    [Fact]
    public void ApplyRejectsDuplicateSuppressIfOffsets()
    {
        var patch = new SctPatchDocument
        {
            SuppressIfPatches =
            [
                new DisableSuppressIfPatch { Apply = true, Offset = 42 },
                new DisableSuppressIfPatch { Apply = true, Offset = 42 },
            ],
        };

        var error = Assert.Throws<ArgumentException>(() => CreateApplier().Apply([], [], patch));

        Assert.Contains("must be unique", error.Message);
    }

    [Fact]
    public void ApplyRejectsUnknownSuppressIfOffset()
    {
        var patch = new SctPatchDocument
        {
            SuppressIfPatches = [new DisableSuppressIfPatch { Apply = true, Offset = 42 }],
        };

        var error = Assert.Throws<InvalidDataException>(() => CreateApplier().Apply([], [], patch));

        Assert.Contains("was not found", error.Message);
    }

    [Fact]
    public void ApplyWritesU8DefaultValue()
    {
        var (sct, operations, patch) = CreateDefaultPatch("u8", 0x00, 1, 6);

        CreateApplier().Apply(sct, operations, patch);

        Assert.Equal((byte)1, sct[5]);
    }

    [Theory]
    [InlineData("u16", 0x01, 0x1234, 7, "3412")]
    [InlineData("u32", 0x02, 0x12345678, 9, "78563412")]
    [InlineData("u64", 0x03, 0x12345678, 13, "7856341200000000")]
    public void ApplyWritesNumericDefaultValueInLittleEndianOrder(string type, byte ifrType, ulong value, byte length, string expectedHex)
    {
        var (sct, operations, patch) = CreateDefaultPatch(type, ifrType, value, length);

        CreateApplier().Apply(sct, operations, patch);

        Assert.Equal(Convert.FromHexString(expectedHex), sct[5..length]);
    }

    private static (byte[] Sct, IfrOperation[] Operations, SctPatchDocument Patch) CreateDefaultPatch(
        string type, byte ifrType, ulong value, byte length)
    {
        var sct = new byte[length];
        sct[0] = 0x5B;
        sct[1] = length;
        sct[2] = 0;
        sct[3] = 0;
        sct[4] = ifrType;
        IfrOperation[] operations =
        [
            new IfrOperation
            {
                Opcode = IfrOpCodes.Default,
                Offset = 0,
                Length = length,
                Fields = new IfrOperationFields { DefaultId = 0 },
            },
        ];
        var patch = new SctPatchDocument
        {
            DefaultValuePatches =
            [
                new DefaultValuePatch
                {
                    Apply = true,
                    Offset = 0,
                    Value = new IfrTypeValue { Type = type, Value = JsonSerializer.SerializeToElement(value) },
                },
            ],
        };
        return (sct, operations, patch);
    }

    private static SctPatchApplier CreateApplier() => new(NullLogger<SctPatchApplier>.Instance);
}
