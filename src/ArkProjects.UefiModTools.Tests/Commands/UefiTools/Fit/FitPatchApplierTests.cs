using ArkProjects.UefiModTools.Commands.UefiTools.Fit;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.Fit;

public class FitPatchApplierTests
{
    [Fact]
    public void ApplyClearsOldEntryThenWritesNewEntry()
    {
        var table = CreateFitTable(FitEntryType.MicrocodeUpdateEntry, FitEntryType.UnusedEntry);
        var map = CreateMap(table);
        var operations = new List<FitPatchOperation>
        {
            new() { Kind = FitPatchOperationKind.Clear, Id = "entry-0001" },
            new()
            {
                Kind = FitPatchOperationKind.Write,
                Id = "entry-0002",
                Entry = new FitEntry
                {
                    Type = FitEntryType.MicrocodeUpdateEntry,
                    Address = 0xFF001000,
                    Version = 1,
                },
            },
        };

        new FitPatchApplier().Apply(table, map, operations);

        Assert.Equal(FitEntryType.UnusedEntry, table.Entries[1].Type);
        Assert.Equal(0xFFFFFFFFUL, table.Entries[1].Address);
        Assert.Equal(FitEntryType.MicrocodeUpdateEntry, table.Entries[2].Type);
        Assert.Equal(0xFF001000UL, table.Entries[2].Address);
    }

    [Fact]
    public void ApplyRejectsHeaderEntryChange()
    {
        var table = CreateFitTable(FitEntryType.UnusedEntry);
        var map = CreateMap(table);
        var operations = new List<FitPatchOperation>
        {
            new() { Kind = FitPatchOperationKind.Clear, Id = "entry-0000" },
        };

        var error = Assert.Throws<ArgumentException>(() => new FitPatchApplier().Apply(table, map, operations));

        Assert.Contains("header", error.Message);
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

    private static FitMapDocument CreateMap(FitTable table)
    {
        return new FitMapDocument
        {
            FitSha256 = "source-hash",
            Entries = new FitMapper().Extract(table),
        };
    }
}
