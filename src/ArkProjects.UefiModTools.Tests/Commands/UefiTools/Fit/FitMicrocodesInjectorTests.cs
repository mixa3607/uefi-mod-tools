using ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.Fit;

public class FitMicrocodesInjectorTests
{
    [Fact]
    public void InjectReplacesUnusedEntriesInPayloadOrder()
    {
        var table = CreateFitTable(FitEntryType.UnusedEntry, FitEntryType.UnusedEntry);
        var microcodes = new MicrocodesTable
        {
            SectionBaseAddress = 0xFF000000,
            UsableStart = 0x100,
            UsableEnd = 0x200,
            MicrocodeFiles = ["first.bin", "second.bin"],
        };

        var result = new FitMicrocodesInjector().Inject(table, microcodes, [[1, 2, 3], [4, 5]]);

        Assert.Equal(FitEntryType.MicrocodeUpdateEntry, result.Entries[1].Type);
        Assert.Equal(0xFF000100UL, result.Entries[1].Address);
        Assert.Equal(FitEntryType.MicrocodeUpdateEntry, result.Entries[2].Type);
        Assert.Equal(0xFF000103UL, result.Entries[2].Address);
    }

    [Fact]
    public void InjectRejectsTableWithoutUnusedEntry()
    {
        var table = CreateFitTable();
        var microcodes = new MicrocodesTable
        {
            MicrocodeFiles = ["first.bin"],
        };

        var error = Assert.Throws<ArgumentException>(() =>
            new FitMicrocodesInjector().Inject(table, microcodes, [[1]]));

        Assert.Contains("Can not find any empty slot in FIT", error.Message);
    }

    private static FitTable CreateFitTable(params FitEntryType[] entryTypes)
    {
        var entries = new List<FitEntry>
        {
            new() { Type = FitEntryType.FitHeaderEntry, Size = (uint)entryTypes.Length + 1 },
        };
        entries.AddRange(entryTypes.Select(type => new FitEntry { Type = type }));
        return new FitTable { Entries = entries };
    }
}
