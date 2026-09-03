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
        var sct = new byte[16];
        sct[5] = 0x00;
        IfrOperation[] operations = [new IfrOperation { Opcode = IfrOpCodes.Default, Offset = 0, Length = 7 }];
        var patch = new SctPatchDocument
        {
            DefaultValuePatches =
            [
                new DefaultValuePatch
                {
                    Apply = true,
                    Offset = 0,
                    Value = new IfrTypeValue { Type = "u8", Value = JsonSerializer.SerializeToElement(1) },
                },
            ],
        };

        CreateApplier().Apply(sct, operations, patch);

        Assert.Equal((byte)1, sct[6]);
    }

    private static SctPatchApplier CreateApplier() => new(NullLogger<SctPatchApplier>.Instance);
}
