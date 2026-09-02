using ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;
using Microsoft.Extensions.Logging.Abstractions;
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
                new DisableSuppressIfPatch { Disable = true, Offset = 42 },
                new DisableSuppressIfPatch { Disable = true, Offset = 42 },
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
            SuppressIfPatches = [new DisableSuppressIfPatch { Disable = true, Offset = 42 }],
        };

        var error = Assert.Throws<InvalidDataException>(() => CreateApplier().Apply([], [], patch));

        Assert.Contains("was not found", error.Message);
    }

    private static SctPatchApplier CreateApplier() => new(NullLogger<SctPatchApplier>.Instance);
}
