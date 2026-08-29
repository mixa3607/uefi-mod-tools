using ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.Microcodes;

public class MicrocodesCombinerTests
{
    [Fact]
    public void CombineWritesPayloadsInConfiguredRange()
    {
        var input = Enumerable.Repeat((byte)0xFF, 16).ToArray();
        var table = new MicrocodesTable
        {
            UsableStart = 4,
            UsableEnd = 10,
            MicrocodeFiles = ["first.bin", "second.bin"],
        };

        var result = new MicrocodesCombiner().Combine(input, table, [[1, 2, 3], [4, 5]]);

        Assert.Equal(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 1, 2, 3, 4, 5, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, result);
    }

    [Fact]
    public void CombineRejectsPayloadLargerThanUsableRange()
    {
        var table = new MicrocodesTable
        {
            UsableStart = 4,
            UsableEnd = 6,
            MicrocodeFiles = ["first.bin"],
        };

        var error = Assert.Throws<ArgumentException>(() =>
            new MicrocodesCombiner().Combine(new byte[16], table, [[1, 2, 3]]));

        Assert.Contains("No space on payload section", error.Message);
    }

    [Fact]
    public void CombineRejectsRangeOutsideInput()
    {
        var table = new MicrocodesTable
        {
            UsableStart = 4,
            UsableEnd = 17,
            MicrocodeFiles = [],
        };

        var error = Assert.Throws<ArgumentException>(() =>
            new MicrocodesCombiner().Combine(new byte[16], table, []));

        Assert.Contains("Microcode usable range is outside the input file", error.Message);
    }
}
