using ArkProjects.UefiModTools.Commands.UefiTools.Fit;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.Fit;

public class FitMapperTests
{
    [Fact]
    public void ExtractSeparatesEntryLocationFromEntryValue()
    {
        var table = CreateFitTable();
        table.HeadGarbage = [0xFF, 0xFF];

        var entries = new FitMapper().Extract(table);

        var entry = Assert.Single(entries);
        Assert.Equal("entry-0000", entry.Id);
        Assert.Equal(0, entry.Index);
        Assert.Equal(2, entry.Offset);
        Assert.Same(table.Entries[0], entry.Entry);
    }

    private static FitTable CreateFitTable()
    {
        return new FitTable
        {
            Entries = [new FitEntry { Type = FitEntryType.FitHeaderEntry, Size = 1 }],
        };
    }
}
