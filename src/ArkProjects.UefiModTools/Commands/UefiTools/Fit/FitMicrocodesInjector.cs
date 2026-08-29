using ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public class FitMicrocodesInjector
{
    public FitTable Inject(FitTable fitTable, MicrocodesTable table, IReadOnlyList<byte[]> microcodes)
    {
        if (table.MicrocodeFiles.Length != microcodes.Count)
            throw new ArgumentException("Microcode file and payload counts do not match", nameof(microcodes));

        var usableEnd = table.UsableEnd < 0 ? int.MaxValue : table.UsableEnd;
        if (table.UsableStart < 0 || table.UsableStart > usableEnd)
            throw new ArgumentException("Microcode usable range is invalid", nameof(table));

        var position = table.UsableStart;
        foreach (var microcode in microcodes)
        {
            if (microcode.Length > usableEnd - position)
                throw new ArgumentException("No space on payload section", nameof(microcodes));

            var placeAt = fitTable.Entries.FindIndex(0, x => x.Type == FitEntryType.UnusedEntry);
            if (placeAt < 0)
                throw new ArgumentException("Can not find any empty slot in FIT", nameof(fitTable));

            fitTable.Entries[placeAt] = new FitEntry
            {
                Type = FitEntryType.MicrocodeUpdateEntry,
                Address = checked((ulong)position + table.SectionBaseAddress),
                Size = 0,
                Version = 1,
                ChecksumValidate = false,
                Checksum = 0,
            };
            position = checked(position + microcode.Length);
        }

        return fitTable;
    }
}
