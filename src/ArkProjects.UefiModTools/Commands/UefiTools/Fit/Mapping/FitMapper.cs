using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;

public class FitMapper
{
    private const int FitEntrySize = 16;

    public List<FitEntryMapping> Extract(FitTable fitTable)
    {
        var tableOffset = fitTable.HeadGarbage.Length;
        return fitTable.Entries
            .Select((entry, index) => new FitEntryMapping
            {
                Id = $"entry-{index:D4}",
                Index = index,
                Offset = tableOffset + index * FitEntrySize,
                Entry = entry,
            })
            .ToList();
    }
}
